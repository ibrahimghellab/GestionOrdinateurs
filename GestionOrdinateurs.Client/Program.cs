using Blazored.SessionStorage;
using GestionOrdinateurs.Client;
using GestionOrdinateurs.Client.Services;
using GestionOrdinateurs.Client.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();

//Même instance pour les deux : les pages qui notifient la connexion et Blazor qui lit l'état
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddHttpClient<IAuthentificationService, AuthentificationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Apis:Authentification"] ?? "");
});

builder.Services.AddTransient<JwtHandler>();

builder.Services.AddHttpClient<IOrdinateurApiService, OrdinateurApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Apis:Gateway"] ?? "");
})
.AddHttpMessageHandler<JwtHandler>();

builder.Services.AddHttpClient<IReparationApiService, ReparationApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Apis:Gateway"] ?? "");
})
.AddHttpMessageHandler<JwtHandler>();

await builder.Build().RunAsync();
