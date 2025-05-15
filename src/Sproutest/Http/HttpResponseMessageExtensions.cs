using Sproutest.Extensions;
using System.Text.Json;

namespace Sproutest.Http;

public static class HttpResponseMessageExtensions
{
    public static async Task EnsureSuccessAsync(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var method = response.RequestMessage?.Method.Method;
            var path = response.RequestMessage?.RequestUri?.PathAndQuery;
            var content = await response.GetFormatedJsonAsync();
            var message = $"{method} {path} failed with status {(int)response.StatusCode} {response.StatusCode}.\nResponse:\n{content}";
            throw new Exception(message);
        }
    }

    private static async Task<string> GetFormatedJsonAsync(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(content))
        {
            return "<empty>";
        }

        if (content.IsValidJson())
        {
            var deserilized = JsonSerializer.Deserialize<JsonDocument>(content, JsonOptions.Default);
            return JsonSerializer.Serialize(deserilized, JsonOptions.Default);
        }

        return content;
    }
}