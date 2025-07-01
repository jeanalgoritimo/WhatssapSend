using WhatsappIntegrationPrototype.Services; // Para poder usar o AIService
  

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Adicionar esta seção para registrar o AIService ---
builder.Services.AddSingleton<AIService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<AIService>>(); // Pega o logger
    var configuration = sp.GetRequiredService<IConfiguration>(); // Pega as configurações
    var geminiApiKey = configuration["GeminiAI:ApiKey"]; // Obtém a chave do Gemini do appsettings.json

    // Cria e retorna uma instância do AIService, passando o logger e a chave de API
    return new AIService(logger, geminiApiKey);
});
// --- Fim da seção a ser adicionada ---


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Garante que o tráfego seja HTTPS

app.UseAuthorization();

app.MapControllers(); // Mapeia os controllers para as rotas

app.Run(); // Inicia a aplicação