using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AI
{
    /// <summary>
    /// Client wrapper for the DeepSeek chat completion API.
    /// </summary>
    public class DeepSeekLLM : ILLM
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new DeepSeek client with the provided credentials.
        /// </summary>
        /// <param name="apiKey">API key used for authentication.</param>
        /// <param name="endpoint">Optional API endpoint override.</param>
        /// <exception cref="ArgumentNullException">Endpoint or API key is null or whitespace.</exception>
        public DeepSeekLLM(string apiKey, string endpoint = "https://api.deepseek.com/v1/chat/completions")
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentNullException(nameof(endpoint));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            _endpoint = endpoint;
            _apiKey = apiKey;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        /// <summary>
        /// Sends a chat completion request to DeepSeek.
        /// </summary>
        /// <param name="systemPrompt">System prompt establishing behavior.</param>
        /// <param name="userPrompt">User content for the model.</param>
        /// <param name="maxTokens">Maximum tokens to generate.</param>
        /// <returns>Model response content.</returns>
        /// <exception cref="ArgumentNullException">Prompts are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxTokens is zero or negative.</exception>
        /// <exception cref="NoInternetException">Network cannot reach the API.</exception>
        /// <exception cref="TimeoutException">Request times out.</exception>
        /// <exception cref="ApiException">API returns an error response.</exception>
        public async Task<string> SendRequestAsync(string systemPrompt, string userPrompt, uint maxTokens = 256)
        {
            if (systemPrompt == null) throw new ArgumentNullException(nameof(systemPrompt));
            if (userPrompt == null) throw new ArgumentNullException(nameof(userPrompt));
            if (maxTokens <= 0) throw new ArgumentOutOfRangeException(nameof(maxTokens), "maxTokens must be > 0");

            var payload = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = maxTokens
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(_endpoint, content);
                await HandleApiErrorAsync(response);

                string responseJson = await response.Content.ReadAsStringAsync();

                try
                {
                    using JsonDocument doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;

                    JsonElement choices = root.GetProperty("choices");
                    if (choices.GetArrayLength() == 0)
                    {
                        throw new Exception("DeepSeek API returned no choices.");
                    }

                    JsonElement first = choices[0];
                    string? result = first.GetProperty("message").GetProperty("content").GetString();
                    return result ?? string.Empty;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to parse DeepSeek API response: {ex.Message}. Raw response: {responseJson}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new NoInternetException(
                    "Cannot reach DeepSeek servers. Check internet connection.",
                    ex
                );
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException("Request to DeepSeek timed out.", ex);
            }
        }

        /// <summary>
        /// Maps API failures to domain-specific exceptions.
        /// </summary>
        /// <param name="response">HTTP response to inspect.</param>
        /// <exception cref="InvalidApiKeyException">Authentication fails.</exception>
        /// <exception cref="NoTokensException">Subscription or quota issues.</exception>
        /// <exception cref="RateLimitException">Request rate exceeded.</exception>
        /// <exception cref="ApiException">Unhandled error status.</exception>
        private static async Task HandleApiErrorAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string body = await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new InvalidApiKeyException();

                case HttpStatusCode.PaymentRequired:
                case HttpStatusCode.Forbidden:
                    throw new NoTokensException();

                case (HttpStatusCode)429:
                    throw new RateLimitException();

                case HttpStatusCode.BadRequest:
                    throw new ApiException("Bad request sent to DeepSeek.");

                default:
                    throw new ApiException(
                        $"DeepSeek API error ({(int)response.StatusCode}): {body}"
                    );
            }
        }
    }
}
