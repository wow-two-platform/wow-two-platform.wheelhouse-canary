using WheelhouseCanary.Api.Configurations;
using WoW.Two.Sdk.Beta.Be.Data;

// Build and configure the host.
var builder = WebApplication.CreateBuilder(args);
builder.Configure();

// Build and configure the app; `migrate <verb>` exits with the verb's code before any startup work runs.
var app = builder.Build();
if (MigrateCommand.IsCommand(args))
    return await app.RunMigrateCommandAsync(args);
await app.ConfigureAsync();

// Run.
await app.RunAsync();
return 0;

// Opens the top-level Program to the E2E test host.
public partial class Program;
