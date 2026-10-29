var builder = WebApplication.CreateBuilder(args);

//Routes et destinations lues dans la section "ReverseProxy" de appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//Le navigateur appelle la Gateway directement : c'est elle qui doit autoriser le client Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowBlazorClient");

app.MapReverseProxy();

app.Run();
