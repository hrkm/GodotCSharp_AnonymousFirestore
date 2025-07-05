using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Firebase
{
	public class Analytics
	{
		public event EventHandler AnalyticsSent;
		public event EventHandler AnalyticsFailed;

		private const string BASE_URL = "https://www.google-analytics.com";
		private string MEASUREMENT_PROTOCOL_COLLECT_URL = "/mp/collect";

		private string _apiSecret;
		private string _measurementId;
		private string _appInstanceId;
		public string UserId;
		public Analytics(string apiSecret, string measurementId, string appInstanceId)
		{
			_apiSecret = apiSecret;
			_measurementId = measurementId;
			_appInstanceId = appInstanceId;
		}

		private HttpClient _analyticsClient = new HttpClient()
		{
			BaseAddress = new Uri(BASE_URL)
		};

		/// <summary>
		/// Request body sent to Google Analytics.
		/// </summary>
		/// <param name="AppInstanceId">app instance id, should be unique per app instance</param>
		/// <param name="UserId">optional user id, if provided enables active users reports</param>
		/// <param name="Events">list of events to upload, as per the API the limit is 25 per request</param>
		public record class AnalyticsRequestBody(
			[property:JsonPropertyName("app_instance_id")]
			string AppInstanceId,
			[property:JsonPropertyName("user_id")]
			[property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			string UserId,
			[property:JsonPropertyName("events")]
			List<Event> Events);

		public record class Event(
			[property:JsonPropertyName("name")]
			string Name,
			[property:JsonPropertyName("params")]
			Dictionary<string, object> Params);

		/// <summary>
		/// Sends a screen_view analytics event.
		/// </summary>
		/// <param name="screenName">name of the screen that is viewed</param>
		/// <returns>true, if event was sent successfully</returns>
		public async Task<bool> SendScreenViewEvent(string screenName)
		{
			var data = new Event("screen_view", new Dictionary<string, object>()
				{
					{ "screen_name", screenName },
					{ "engagement_time_msec", 1 } // this enables to count active users when combined with UserId
				});
			return await SendEvent(data);
		}

		/// <summary>
		/// Sends an arbitrary event to the Google Analytics.
		/// </summary>
		/// <param name="data">event to be sent</param>
		/// /// <returns>true, if event was sent successfully</returns>
		public async Task<bool> SendEvent(Event data) {
			return await SendEvents(new List<Event>() { data });
		}

		/// <summary>
		/// Sends a list of events to Google Analytics
		/// </summary>
		/// <param name="data">list of events to upload in single query, limit is 25!</param>
		/// <returns>true, if events were sent successfully</returns>
		public async Task<bool> SendEvents(List<Event> data)
		{
			if (data.Count > 25)
			{
				AnalyticsFailed?.Invoke(this, EventArgs.Empty);
				return false;
			}
			using StringContent content = new(JsonSerializer.Serialize(new AnalyticsRequestBody(_appInstanceId, UserId, data)));
			using var response = await _analyticsClient.PostAsync(MEASUREMENT_PROTOCOL_COLLECT_URL + "?api_secret=" + _apiSecret + "&measurement_id=" + _measurementId, content);

			if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
			{
				AnalyticsFailed?.Invoke(this, EventArgs.Empty);
				return false;
			}
			AnalyticsSent?.Invoke(this, EventArgs.Empty);
			return true;
		}
	}
}
