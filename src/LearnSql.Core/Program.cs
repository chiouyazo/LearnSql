using LearnSql;
using LearnSql.Sql;
using LearnSql.Sql.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

builder.Services.AddSingleton<DatabaseProvider>();

WebAssemblyHost host = builder.Build();
DatabaseProvider databaseProvider = host.Services.GetRequiredService<DatabaseProvider>();
await databaseProvider.Initialize();
ErrorTranslator.SetLanguage(Language.MsSql);

await host.RunAsync();
