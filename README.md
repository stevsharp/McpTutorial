# VetClinic MCP Tutorial (ASP.NET Core)

A hands-on example of the [Model Context Protocol](https://modelcontextprotocol.io/) in .NET: an **ASP.NET Core MCP server** that exposes a vet clinic's appointments, and a **console client** that calls it directly and through an LLM agent.

Built with the official [MCP C# SDK](https://csharp.sdk.modelcontextprotocol.io/) over the Streamable HTTP transport.

```
┌──────────────┐  tool calls  ┌──────────────┐   HTTP /mcp   ┌──────────────┐     DI     ┌──────────────────┐
│ LLM provider │ ◄──────────► │  Client app  │ ◄───────────► │  MCP server  │ ─────────► │  Your services   │
│ (IChatClient)│              │  McpClient   │               │ ASP.NET Core │            │ AppointmentSvc   │
└──────────────┘              └──────────────┘               └──────────────┘            └──────────────────┘
```

## Features

**Server** (`VetClinic.McpServer`)

| Type | Name | Description |
| --- | --- | --- |
| Tool | `get_upcoming_appointments` | Lists a clinic's upcoming appointments (read-only) |
| Tool | `book_appointment` | Books a new appointment |
| Tool | `cancel_appointment` | Cancels an appointment (marked destructive) |
| Resource | `clinic://{clinicId}/hours` | A clinic's opening hours |
| Resource | `clinic://services` | Services offered across the network |
| Prompt | `daily_briefing` | Front-desk summary of today's schedule |

**Client** (`VetClinic.McpClient`)

- Connects with `HttpClientTransport` and `McpClient.CreateAsync`
- Lists and calls tools, reads resources, gets prompts
- Interactive chat agent: the LLM calls the server's tools through `Microsoft.Extensions.AI`

## Project structure

```
McpTutorial/
├── McpTutorial.sln
├── VetClinic.McpServer/
│   ├── Program.cs
│   ├── Services/AppointmentService.cs
│   ├── Tools/AppointmentTools.cs
│   ├── Resources/ClinicResources.cs
│   └── Prompts/ClinicPrompts.cs
└── VetClinic.McpClient/
    └── Program.cs
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later (.NET 9 if you keep `System.Threading.Lock`)
- [Node.js](https://nodejs.org/), only for the MCP Inspector
- An LLM: an OpenAI API key, **or** [Ollama](https://ollama.com/) for a free local model

## Getting started

### 1. Clone and build

```bash
git clone https://github.com/<your-user>/McpTutorial.git
cd McpTutorial
dotnet build
```

### 2. Run the server

```bash
cd VetClinic.McpServer
dotnet run
```

Note the URL in the console output, for example:

```
Now listening on: https://localhost:7229
```

The MCP endpoint is that URL plus `/mcp`, e.g. `https://localhost:7229/mcp`.

> If you get a certificate error, trust the development certificate once:
> `dotnet dev-certs https --trust`

### 3. Point the client at the server

In `VetClinic.McpClient/Program.cs`, set the endpoint to match the server **exactly**, including `https`:

```csharp
var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri("https://localhost:7229/mcp"),
    TransportMode = HttpTransportMode.StreamableHttp,
    Name = "VetClinic"
});
```

### 4. Configure the LLM

**Option A: OpenAI** (key from [platform.openai.com/api-keys](https://platform.openai.com/api-keys); requires API billing credit)

Store the key with User Secrets so it never lands in the repo:

```bash
cd VetClinic.McpClient
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-..."
```

**Option B: Ollama** (free, local, no key)

```bash
ollama pull llama3.2
dotnet add package OllamaSharp
```

```csharp
IChatClient chat = new OllamaApiClient(new Uri("http://localhost:11434"), "llama3.2")
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();
```

Use a model that supports tool calling (`llama3.2`, `qwen2.5`, ...).

### 5. Run the client

With the server still running:

```bash
cd VetClinic.McpClient
dotnet run
```

Try asking:

- *What's on the schedule this week?*
- *Book Milo, a rabbit owned by Eleni S., tomorrow at 11:00 for a check-up.*
- *Cancel Rex's appointment.*

> **Visual Studio:** right-click the solution → *Configure Startup Projects* → *Multiple startup projects*, and start the server before the client.

## Testing with the MCP Inspector

```bash
npx @modelcontextprotocol/inspector
```

Set **Transport** to *Streamable HTTP*, **URL** to your `/mcp` endpoint, and click **Connect**. You can then run every tool, resource and prompt from the browser.

## How it works

Server registration (`VetClinic.McpServer/Program.cs`):

```csharp
builder.Services.AddSingleton<IAppointmentService, InMemoryAppointmentService>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

var app = builder.Build();
app.MapMcp("/mcp");
app.Run();
```

A tool is a plain method with attributes; the SDK builds the JSON schema from its parameters:

```csharp
[McpServerToolType]
public class AppointmentTools(IAppointmentService service)
{
    [McpServerTool(ReadOnly = true)]
    [Description("Lists upcoming appointments for a clinic.")]
    public IReadOnlyList<Appointment> GetUpcomingAppointments(
        [Description("The clinic ID")] int clinicId,
        [Description("How many days ahead to look, 1 to 30")] int days = 7)
        => service.GetUpcoming(clinicId, Math.Clamp(days, 1, 30));
}
```

On the client, MCP tools plug straight into any `IChatClient`:

```csharp
var tools = await client.ListToolsAsync();
var response = await chat.GetResponseAsync(history, new ChatOptions { Tools = [.. tools] });
```

## Troubleshooting

| Symptom | Cause | Fix |
| --- | --- | --- |
| `HttpIOException: The response ended prematurely` | Client uses `http://` on the server's HTTPS port, or the wrong port | Copy the exact URL from the server's "Now listening on" line and add `/mcp` |
| `The SSL connection could not be established` | Dev certificate not trusted | `dotnet dev-certs https --trust`, then restart both apps |
| Connection refused | Server not running, or client started first | Start the server first |
| 404 Not Found | Path mismatch | `MapMcp()` serves the root; `MapMcp("/mcp")` serves `/mcp` |
| 404 / error when opening `/mcp` in a browser | Expected: MCP uses POST | Test with the client or the Inspector |
| Tool not found | Using the C# method name | Use the snake_case name from `ListToolsAsync()` |
| `OPENAI_API_KEY` is null | Variable set after Visual Studio started | Restart Visual Studio, or use User Secrets |
| `McpClientFactory` / `SseClientTransport` not found | Old preview sample code | Use `McpClient.CreateAsync` and `HttpClientTransport` |

## Next steps

- Replace `InMemoryAppointmentService` with a real data layer (EF Core)
- Protect the endpoint with JWT: `app.MapMcp("/mcp").RequireAuthorization()`
- Add integration tests with `WebApplicationFactory` + `McpClient`
- Connect the server to Claude Desktop or VS Code as a remote MCP server

## Resources

- [MCP C# SDK documentation](https://csharp.sdk.modelcontextprotocol.io/)
- [MCP C# SDK on GitHub](https://github.com/modelcontextprotocol/csharp-sdk)
- [Model Context Protocol specification](https://modelcontextprotocol.io/)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)

## License

MIT
