using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

public interface IFoundryAgentClient
{
    Task<string> SendMessageAsync(string message, CancellationToken cancellationToken);
    Task<DocumentUploadResult> UploadDocumentAsync(string fileName, string contentType, Stream content, CancellationToken cancellationToken);
}

public sealed record DocumentUploadResult(string Message, string? DocumentId);

public sealed class FoundryAgentClient(HttpClient httpClient, ILogger<FoundryAgentClient> logger) : IFoundryAgentClient
{
    private static readonly string[] ReplyProperties = ["response", "message", "output", "text"];

    public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_AGENT_CHAT_ENDPOINT");
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return $"Hello! You said: {message}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(new { message })
        };

        await AddBearerTokenAsync(request, cancellationToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Foundry chat call failed with status {StatusCode}", response.StatusCode);
            return "I could not reach the configured Foundry agent endpoint.";
        }

        return TryExtractText(body) ?? body;
    }

    public async Task<DocumentUploadResult> UploadDocumentAsync(string fileName, string contentType, Stream content, CancellationToken cancellationToken)
    {
        var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_AGENT_UPLOAD_ENDPOINT");
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return new DocumentUploadResult("Document received. Set FOUNDRY_AGENT_UPLOAD_ENDPOINT to forward files to Foundry.", null);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        using var formData = new MultipartFormDataContent();
        var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        formData.Add(streamContent, "file", fileName);
        request.Content = formData;

        await AddBearerTokenAsync(request, cancellationToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Foundry document upload failed with status {StatusCode}", response.StatusCode);
            return new DocumentUploadResult("Document upload failed against configured Foundry endpoint.", null);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return new DocumentUploadResult("Document uploaded successfully.", null);
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var id = root.TryGetProperty("id", out var idProperty) ? idProperty.GetString() : null;
        var message = root.TryGetProperty("message", out var messageProperty)
            ? messageProperty.GetString()
            : "Document uploaded successfully.";

        return new DocumentUploadResult(message ?? "Document uploaded successfully.", id);
    }

    private static string? TryExtractText(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var propertyName in ReplyProperties)
            {
                if (doc.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString();
                }
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static async Task AddBearerTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var scope = Environment.GetEnvironmentVariable("FOUNDRY_AGENT_SCOPE");
        var tokenScope = string.IsNullOrWhiteSpace(scope) ? "https://cognitiveservices.azure.com/.default" : scope;

        var credential = new DefaultAzureCredential();
        AccessToken token = await credential.GetTokenAsync(new TokenRequestContext([tokenScope]), cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }
}
