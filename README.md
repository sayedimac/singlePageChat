# singlePageChat

Blazor WebAssembly + Azure Functions (.NET 10) starter for a chat UI backed by an Azure AI Foundry agent.

## Projects

- `src/web`: static Blazor WebAssembly chat interface
- `src/api`: Azure Functions backend



## Features

- Chat prompt endpoint (`POST /api/chat`)
- Name greeting endpoint (`GET /api/chat/greet/{name}`)
- Document upload endpoint (`POST /api/chat/documents`)
- Voice recording in browser and upload to the same document endpoint

## Azure Foundry configuration

Set these environment variables for the Functions app:

- `FOUNDRY_AGENT_CHAT_ENDPOINT` (required for real chat forwarding)
- `FOUNDRY_AGENT_UPLOAD_ENDPOINT` (required for real document forwarding)
- `FOUNDRY_AGENT_SCOPE` (optional; defaults to `https://cognitiveservices.azure.com/.default`)

Authentication to Foundry uses `DefaultAzureCredential`.

## Run locally

```bash
dotnet build singlePageChat.slnx
dotnet run --project src/web
```

To run the Functions app locally, install Azure Functions Core Tools v4+.
