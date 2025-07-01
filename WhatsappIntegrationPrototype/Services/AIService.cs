using GenerativeAI; 

namespace WhatsappIntegrationPrototype.Services
{
    public class AIService
    {
        private readonly ILogger<AIService> _logger;
        private readonly string _geminiApiKey;
        private readonly GenerativeModel _generativeModel; // Para usar a API Gemini

        public AIService(ILogger<AIService> logger, string geminiApiKey)
        {
            _logger = logger;
            _geminiApiKey = geminiApiKey;

            // Inicializa o modelo Gemini APENAS se a chave de API for fornecida.
            // Isso permite que a lógica simulada funcione se a chave estiver vazia.
            if (!string.IsNullOrEmpty(_geminiApiKey))
            {
                // Mude esta linha:
                // _generativeModel = new GenerativeModel(_geminiApiKey, model: "gemini-pro");

                // Para uma destas opções:
                // Opção 1 (mais recente e geralmente a mais recomendada, se disponível para sua conta):
                _generativeModel = new GenerativeModel(_geminiApiKey, model: "gemini-1.5-pro-latest");

                // Opção 2 (se a 1 não funcionar, tente o modelo de texto mais básico, mas ainda bom):
                // _generativeModel = new GenerativeModel(_geminiApiKey, model: "models/text-bison-001");
                // (Note que este último é "text-bison-001" e não "gemini" - é um modelo mais antigo, mas pode funcionar)
            }
        }

        public async Task<string> ProcessMessageWithAI(string userMessage, string userId)
        {
            _logger.LogInformation($"Mensagem recebida para AI Service: '{userMessage}' do usuário '{userId}'");

            if (!string.IsNullOrEmpty(_geminiApiKey) && _generativeModel != null)
            {
                // --- Usar Google Gemini API (Opção real de IA) ---
                try
                {
                    var response = await _generativeModel.GenerateContentAsync(userMessage);
                    // O Gemini pode retornar múltiplos candidatos ou partes. Pegamos a primeira parte de texto.
                    var textResponse = response.Candidates[0].Content.Parts[0].Text;
                    _logger.LogInformation($"Resposta do Gemini: {textResponse}");
                    return textResponse;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao chamar a API do Google Gemini. Verifique a chave de API e a conectividade.");
                    return "Desculpe, tive um problema ao me conectar com a inteligência artificial. Por favor, tente novamente.";
                }
            }
            else
            {
                // --- Lógica de IA simulada em C# (Opção fallback ou para teste inicial sem Gemini) ---
                string response;
                string lowerMessage = userMessage.ToLower();

                if (lowerMessage.Contains("olá") || lowerMessage.Contains("oi"))
                {
                    response = "Olá! Como posso ajudar você hoje?";
                }
                else if (lowerMessage.Contains("ajuda"))
                {
                    response = "Estou aqui para ajudar. Qual é a sua dúvida?";
                }
                else if (lowerMessage.Contains("obrigado") || lowerMessage.Contains("valeu"))
                {
                    response = "De nada! Fico feliz em ajudar.";
                }
                else
                {
                    response = $"Recebi sua mensagem: '{userMessage}'. No momento, minha IA ainda está aprendendo. Pode perguntar outra coisa?";
                }

                _logger.LogInformation($"Resposta simulada da IA: '{response}'");
                return response;
            }
        }
    }
}