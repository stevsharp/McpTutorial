using VetClinic.McpServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

builder.Services.AddSingleton<IAppointmentService, InMemoryAppointmentService>();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run();
