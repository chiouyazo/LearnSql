using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LearnSql;
using LearnSql.Sql;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<DatabaseProvider>();

WebAssemblyHost host = builder.Build();
DatabaseProvider databaseProvider = host.Services.GetRequiredService<DatabaseProvider>();
await databaseProvider.Initialize();

await host.RunAsync();