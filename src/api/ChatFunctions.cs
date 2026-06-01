using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public sealed class ChatFunctions(IFoundryAgentClient foundryAgentClient)
{
    [Function("GreetByName")]
    public IActionResult GreetByName(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/greet/{name?}")] HttpRequest request,
        string? name)
    {
        var value = string.IsNullOrWhiteSpace(name) ? "there" : name;
        return new OkObjectResult(new GreetingResponse($"Hello {value}!"));
    }

    [Function("Chat")]
    public async Task<IActionResult> Chat(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var chatRequest = await JsonSerializer.DeserializeAsync<ChatRequest>(request.Body, cancellationToken: cancellationToken);
        if (chatRequest is null || string.IsNullOrWhiteSpace(chatRequest.Message))
        {
            return new BadRequestObjectResult(new { error = "A non-empty message is required." });
        }

        var response = await foundryAgentClient.SendMessageAsync(chatRequest.Message, cancellationToken);
        return new OkObjectResult(new ChatResponse(response));
    }

    [Function("UploadDocument")]
    public async Task<IActionResult> UploadDocument(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat/documents")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return new BadRequestObjectResult(new { error = "Upload requires multipart/form-data." });
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.FirstOrDefault();
        if (file is null)
        {
            return new BadRequestObjectResult(new { error = "No file uploaded." });
        }

        await using var stream = file.OpenReadStream();
        var result = await foundryAgentClient.UploadDocumentAsync(file.FileName, file.ContentType, stream, cancellationToken);
        return new OkObjectResult(new UploadResponse(result.Message, result.DocumentId));
    }

    private sealed record ChatRequest(string Message);
    private sealed record ChatResponse(string Response);
    private sealed record GreetingResponse(string Greeting);
    private sealed record UploadResponse(string Message, string? DocumentId);
}
