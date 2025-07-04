using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Firebase
{
	public class Firestore
	{
		public event EventHandler<FirestoreDocument> DocumentCreated;

		private const string BASE_URL = "https://firestore.googleapis.com";
		private const string DOCUMENTS_URL = "/databases/(default)/documents/"; // to be appended with collection ID
		private const string DOCUMENT_ID_QUERY = "?documentId="; // to be appended with user ID
		private string _projectPart = "/v1/projects/"; // to be appended with project ID

		/// <summary>
		/// Class representing a Firestore Document.
		/// </summary>
		/// <param name="Fields">dictionary of fields with string as key and *FirestoreValue as value</param>
		/// <param name="Name">name of the document, leave null when creating new document</param>
		/// <param name="CreateTime">timestamp of when the document was created</param>
		/// <param name="UpdateTime">timestamp of when the document was last updated</param>
		public record class FirestoreDocument(
			[property:JsonPropertyName("fields")]
			Dictionary<string, object> Fields,
			[property:JsonPropertyName("name")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string Name = null,
			[property:JsonPropertyName("createTime")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string CreateTime = null,
			[property:JsonPropertyName("updateTime")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string UpdateTime = null);

		public record class StringFirestoreValue([property:JsonPropertyName("stringValue")]
			string value);
		public record class IntegerFirestoreValue([property:JsonPropertyName("integerValue")]
			string value);
		public record class DoubleFirestoreValue([property:JsonPropertyName("doubleValue")]
			double value);
		public record class BooleanFirestoreValue([property:JsonPropertyName("booleanValue")]
			bool value);

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
		/// <param name="userId">user id, from <see cref="Firebase.Auth.LoginResponse.UserId"/></param>
		/// <param name="document">contents of the document</param>
		public async void CreateDocument(string collectionId, string userId, FirestoreDocument document)
		{
			using StringContent content = new(JsonSerializer.Serialize(document));
			using var response = await _documentsClient.PostAsync(_projectPart + DOCUMENTS_URL + collectionId + DOCUMENT_ID_QUERY + userId, content);
			response.EnsureSuccessStatusCode();

			var responseContent = await response.Content.ReadFromJsonAsync<FirestoreDocument>();
			DocumentCreated?.Invoke(this, responseContent);
		}
	}
}
