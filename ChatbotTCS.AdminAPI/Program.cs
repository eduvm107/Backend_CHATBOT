using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar MongoDB Settings desde appsettings.json
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

// Registrar todos los servicios como Singleton
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddSingleton<MensajeAutomaticoService>();
builder.Services.AddSingleton<DocumentoService>();
builder.Services.AddSingleton<ActividadService>();
builder.Services.AddSingleton<UsuarioService>();
builder.Services.AddSingleton<ConfiguracionService>();
builder.Services.AddSingleton<ConversacionService>();

// Configurar OllamaService con HttpClient
builder.Services.AddHttpClient<IOllamaService, OllamaService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var ollamaUrl = config["Ollama:Url"] ?? "http://localhost:11434";
    client.BaseAddress = new Uri(ollamaUrl);
    client.Timeout = TimeSpan.FromSeconds(
        int.Parse(config["Ollama:TimeoutSeconds"] ?? "30")
    );
});

// Registrar OllamaService como singleton con configuración
builder.Services.AddSingleton<IOllamaService>(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(OllamaService));
    var logger = serviceProvider.GetRequiredService<ILogger<OllamaService>>();
    var config = serviceProvider.GetRequiredService<IConfiguration>();

    return new OllamaService(
        httpClient,
        logger,
        config["Ollama:Url"] ?? "http://localhost:11434",
        config["Ollama:ModelName"] ?? "llama-tcs"
    );
});

// Add services to the container.
builder.Services.AddControllers();

// Configurar CORS para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Agregar Swagger para documentación
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ChatbotTCS Admin API",
        Version = "v1",
        Description = "API completa para gestionar el Chatbot de Onboarding TCS - FAQs, Documentos, Actividades, Usuarios, Configuración y Conversaciones"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatbotTCS Admin API v1");
    });
}

// Habilitar CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
