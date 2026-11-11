using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri("https://localhost:7229/mcp"),
    TransportMode = HttpTransportMode.StreamableHttp,
    Name = "VetClinic"
});

await Task.Delay(2000); // Wait for the server to start

await using var client = await McpClient.CreateAsync(transport);
Console.WriteLine($"Connected to {client.ServerInfo.Name}");

// 1. Discover tools
var tools = await client.ListToolsAsync();
foreach (var tool in tools)
    Console.WriteLine($"tool  {tool.Name}: {tool.Description}");

// 2. Call a tool
CallToolResult result = await client.CallToolAsync(
    "get_upcoming_appointments",
    new Dictionary<string, object?> { ["clinicId"] = 12, ["days"] = 7 });

foreach (var block in result.Content.OfType<TextContentBlock>())
    Console.WriteLine(block.Text);

if (result.IsError == true)
    Console.WriteLine("The tool reported an error.");

// 3. Read a resource
var hours = await client.ReadResourceAsync("clinic://12/hours");
foreach (var item in hours.Contents.OfType<TextResourceContents>())
    Console.WriteLine($"hours {item.Text}");

// 4. Get a prompt
var prompt = await client.GetPromptAsync(
    "daily_briefing",
    new Dictionary<string, object?> { ["clinicId"] = 12 });
foreach (var message in prompt.Messages)
    if (message.Content is TextContentBlock text)
        Console.WriteLine($"prompt [{message.Role}] {text.Text}");
