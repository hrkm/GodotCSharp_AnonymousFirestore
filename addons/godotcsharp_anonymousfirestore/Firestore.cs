using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Firebase
{
	public class Firestore
	{
		public event EventHandler<FirestoreDocument> DocumentCreated;
		public event EventHandler<FirestoreDocument> DocumentFetched;
		public event EventHandler<FirestoreDocument> DocumentPatched;

		private const string BASE_URL = "https://firestore.googleapis.com";
		private const string DOCUMENTS_URL = "/databases/(default)/documents/"; // to be appended with collection ID
		private const string DOCUMENT_ID_QUERY = "?documentId="; // to be appended with user ID
		private const string UPDATE_MASK_QUERY = "updateMask.fieldPaths="; // to be appended with update mask
		private string _projectPart = "/v1/projects/"; // to be appended with project ID

		/// <summary>
		/// Class representing a Firestore Document.
		/// </summary>
		/// <param name="Fields">dictionary of fields with string as key and *FirestoreValue as value</param>
		/// <param name="Name">name of the document, leave null when creating new document</param>
		/// <param name="CreateTime">timestamp of when the document was created</param>
		/// <param name="UpdateTime">timestamp of when the document was last updated</param>

		/// <summary>
		/// Create a new instance to interact with Firestore.
		/// </summary>
		/// <param name="projectId">the id of the project as it appears in https://console.firebase.google.com/project/{project-id}</param>
		/// <param name="token">authenticated user token to use with the requests, from <see cref="Firebase.Auth.LoginResponse.IdToken"/></param>
		public Firestore(string projectId, string token)
		{
			_projectPart += projectId;
			_documentsClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		private HttpClient _documentsClient = new HttpClient()
		{
			BaseAddress = new Uri(BASE_URL),
		};

		/// <summary>
		/// Create a new document named with the user id under the given collection. If the collection does not exist, Firebase will create it.
		/// </summary>
		/// <param name="collectionId">name of the collection to store the document under</param>
		/// <param name="document">contents of the document</param>
		public async void CreateDocument(string collectionId, FirestoreDocument document, string documentId)
		{
			using StringContent content = new(JsonSerializer.Serialize(document));
			using var response = await _documentsClient.PostAsync(_projectPart + DOCUMENTS_URL + collectionId + DOCUMENT_ID_QUERY + documentId, content);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadFromJsonAsync<FirestoreDocument>();
			DocumentCreated?.Invoke(this, responseContent);
		}

		public async Task<FirestoreDocument> GetDocument(string documentPath)
		{
			using var response = await _documentsClient.GetAsync(_projectPart + DOCUMENTS_URL + documentPath);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				var responseContent = await response.Content.ReadFromJsonAsync<FirestoreDocument>();
				DocumentFetched?.Invoke(this, responseContent);
				return responseContent;
			}
			// for whatever reason the document fetch failed
			return null;
		}

		public async void PatchDocument(string documentPath, FirestoreDocument document, string updateMask)
		{
			var queryParams = String.Join("&" + UPDATE_MASK_QUERY, updateMask.Split(","));
			using StringContent content = new(JsonSerializer.Serialize(document));
			using var response = await _documentsClient.PatchAsync(_projectPart + DOCUMENTS_URL + documentPath + "?" + UPDATE_MASK_QUERY + queryParams, content);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadFromJsonAsync<FirestoreDocument>();
			DocumentPatched?.Invoke(this, responseContent);
		}
	}
}
