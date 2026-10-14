using WheelhouseCanary.Api.Configurations;

// Build and configure the host.
var builder = WebApplication.CreateBuilder(args);
builder.Configure();

// Build and configure the app.
var app = builder.Build();
await app.ConfigureAsync();

// Run.
app.Run();

// Opens the top-level Program to the E2E test host.
public partial class Program;
