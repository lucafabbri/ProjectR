using Microsoft.OpenApi;
using ProjectR.DI;
using ProjectR.Sample.Application.DTOs;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Aggiungi la scansione e registrazione dei mapper
builder.Services.AddMappers(typeof(ProductDto).Assembly);

// *** 1. Configurazione avanzata di Swagger/OpenAPI ***
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Aggiunge informazioni generali all'interfaccia di Swagger
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ProjectR Sample API",
        Description = "An ASP.NET Core Web API for managing Products and Reviews."
    });

    // Configura Swashbuckle per usare i commenti XML del codice sorgente
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});


var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
// *** 2. Abilita il middleware di Swagger solo in ambiente di sviluppo ***
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Rende l'interfaccia di Swagger disponibile alla radice dell'applicazione (es. http://localhost:5033)
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

