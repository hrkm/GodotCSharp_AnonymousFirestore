using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Firebase
{
	public class Auth
	{
		public event EventHandler<LoginResponse> LoggedIn;
		public event EventHandler<TokenRefreshResponse> TokenRefreshed;
		public event EventHandler LoggedOut;

		private const string USER_AUTH_FILENAME = "user://user.auth";
		private const string BASE_URL = "https://identitytoolkit.googleapis.com";
		private const string REFRESH_TOKEN_URL = "https://securetoken.googleapis.com";
		private string SIGNUP_REQUEST_URL = "/v1/accounts:signUp?key=";
		private string REFRESH_REQUEST_URL = "/v1/token?key=";

		private string _apiKey;

		private LoginResponse _data;

		/// <summary>
		/// Create a new instance of Firebase.Auth using the provided API key.
		/// </summary>
		/// <param name="apiKey">key to use with the requests</param>
		public Auth(string apiKey)
		{
			_apiKey = apiKey;
		}

		private HttpClient _singupClient = new HttpClient()
		{
			BaseAddress = new Uri(BASE_URL)
		};

		private HttpClient _refreshClient = new HttpClient()
		{
			BaseAddress = new Uri(REFRESH_TOKEN_URL)
		};

		/// <summary>
		/// Response from the <see cref="LoginAnonymously">.
		/// </summary>
		public record class LoginResponse(
			string Kind,
			string IdToken,
			string RefreshToken,
			long ExpiresIn,
			[property:JsonPropertyName("LocalId")]
			string UserId);

		/// <summary>
		/// Response from the <see cref="ManualTokenRefresh(string)">.
		/// </summary>
		public record class TokenRefreshResponse(
			[property:JsonPropertyName("access_token")]
			string AccessToken,
			[property:JsonPropertyName("expires_in")]
			long ExpiresIn,
			[property:JsonPropertyName("token_type")]
			string TokenType,
			[property:JsonPropertyName("refresh_token")]
			string RefreshToken,
			[property:JsonPropertyName("id_token")]
			string IdToken,
			[property:JsonPropertyName("user_id")]
			string UserId,
			[property:JsonPropertyName("project_id")]
			long ProjectId);

		/// <summary>
		/// Send REST request to Firebase.Authentication to sign up for a new anonymous account.
		/// </summary>
		public async void LoginAnonymously(bool saveOnSuccess = false)
		{
			using StringContent content = new(JsonSerializer.Serialize(new { returnSecureToken = true }), Encoding.UTF8, "application/json");
			using var response = await _singupClient.PostAsync(SIGNUP_REQUEST_URL + _apiKey, content);
			response.EnsureSuccessStatusCode();

			_data = await response.Content.ReadFromJsonAsync<LoginResponse>();
			if (saveOnSuccess)
			{
				SaveAuth(_data);
			}
			LoggedIn?.Invoke(this, _data);
		}

		/// <summary>
		/// Remove all locally stored authorization data.
		/// </summary>
		public void Logout()
		{
			RemoveAuth();
			LoggedOut?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Send REST request to refresh the authorization data.
		/// </summary>
		/// <param name="refreshToken">required token used to retrieve a new token</param>
		public async void ManualTokenRefresh(string refreshToken, bool saveOnSuccess = false)
		{
			using StringContent content = new(JsonSerializer.Serialize(new { grant_type = "refresh_token", refresh_token = refreshToken }), Encoding.UTF8, "application/json");
			using var response = await _refreshClient.PostAsync(REFRESH_REQUEST_URL + _apiKey, content);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadFromJsonAsync<TokenRefreshResponse>();
			_data = new LoginResponse("", responseContent.AccessToken, responseContent.RefreshToken, responseContent.ExpiresIn, responseContent.UserId);
			if (saveOnSuccess)
			{
				SaveAuth(_data);
			}
			TokenRefreshed?.Invoke(this, responseContent);
		}

		/// <summary>
		/// Saves the login data to an encrypted file in the userspace.
		/// </summary>
		private void SaveAuth(LoginResponse data)
		{
			var file = Godot.FileAccess.OpenEncryptedWithPass(USER_AUTH_FILENAME, Godot.FileAccess.ModeFlags.Write, _apiKey);
			file.StoreLine(JsonSerializer.Serialize(data));
		}

		/// <summary>
		/// Loads the locally cached login data and requests a new token via <see cref="ManualTokenRefresh(string)"/>;
		/// </summary>
		public void LoadAuth()
		{
			var file = Godot.FileAccess.OpenEncryptedWithPass(USER_AUTH_FILENAME, Godot.FileAccess.ModeFlags.Read, _apiKey);
			var data = JsonSerializer.Deserialize<LoginResponse>(file.GetLine());
			ManualTokenRefresh(data.RefreshToken, true);
		}

		private void RemoveAuth()
		{
			if (Godot.FileAccess.FileExists(USER_AUTH_FILENAME))
				Godot.DirAccess.RemoveAbsolute(USER_AUTH_FILENAME);
		}
	}
}
