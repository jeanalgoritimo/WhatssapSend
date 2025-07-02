using GenerativeAI;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace WhatsappIntegrationPrototype.Services
{
    public enum ConversationState
    {
        None,             // Estado inicial: não estamos esperando nenhuma informação específica
        AwaitingName,     // Estamos esperando o nome do usuário
        AwaitingEmail,    // Estamos esperando o email do usuário
        AwaitingProfession // Estamos esperando a profissão do usuário
    }

    public record UserData(string UserId)
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Profession { get; set; } = string.Empty;
    }

    public class AIService
    {
        private readonly ILogger<AIService> _logger;
        private readonly string _geminiApiKey;
        private readonly GenerativeModel _generativeModel;
        private readonly ConcurrentDictionary<string, ConversationState> _conversationState;
        private readonly ConcurrentDictionary<string, UserData> _userData;

        public AIService(ILogger<AIService> logger, string geminiApiKey)
        {
            _logger = logger;
            _geminiApiKey = geminiApiKey;

            if (!string.IsNullOrEmpty(_geminiApiKey))
            {
                _generativeModel = new GenerativeModel(_geminiApiKey, model: "gemini-1.5-pro-latest");
            }

            _conversationState = new ConcurrentDictionary<string, ConversationState>();
            _userData = new ConcurrentDictionary<string, UserData>();
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
                string lowerMessage = userMessage.ToLower().Trim();

                // Obtém o estado atual da conversa para este usuário. Se não existir, define como None.
                ConversationState currentState = _conversationState.GetOrAdd(userId, ConversationState.None);
                // Obtém ou cria um objeto UserData para este usuário.
                UserData currentUserData = _userData.GetOrAdd(userId, new UserData(userId));

                // --- Opção Global de Sair/Cancelar ---
                if (lowerMessage == "sair" || lowerMessage == "cancelar" || lowerMessage == "0")
                {
                    _conversationState.TryRemove(userId, out _); // Remove o estado da conversa
                    _userData.TryRemove(userId, out _); // Limpa os dados temporários
                    return "Ok, a conversa foi reiniciada. Como posso ajudar você agora? Digite 'menu' para ver as opções.";
                }

                // A lógica principal da conversa é baseada no estado atual
                switch (currentState)
                {
                    case ConversationState.None:
                        // Se não estamos esperando nada, processamos comandos gerais ou iniciamos o fluxo
                        if (lowerMessage.Contains("olá") || lowerMessage.Contains("oi") || lowerMessage.Contains("boa noite") 
                            || lowerMessage.Contains("bom dia") || lowerMessage.Contains("boa tarde"))
                        {
                            response = "Olá! Como posso ajudar você hoje? Bem - vindo!"+
                                       "Por favor, escolha uma opção digitando o número correspondente:\n" +
                                       "1 - Coletar meus dados (Nome, Email, Profissão)\n" +
                                       "2 - Fazer uma pergunta geral (em breve)\n" +
                                       "0 - Sair/Cancelar";
                        }
                        else if (lowerMessage.Contains("ajuda"))
                        {
                            response = "Estou aqui para ajudar.Bem - vindo!" +
                                       "Por favor, escolha uma opção digitando o número correspondente:\n" +
                                       "1 - Coletar meus dados (Nome, Email, Profissão)\n" +
                                       "2 - Fazer uma pergunta geral (em breve)\n" +
                                       "0 - Sair/Cancelar";
                        }
                        else if (lowerMessage.Contains("obrigado") || lowerMessage.Contains("valeu"))
                        {
                            response = "De nada! Fico feliz em ajudar.";
                        }
                        else if (lowerMessage.Contains("qual seu nome") || lowerMessage.Contains("seu nome"))
                        {
                            response = "Eu sou um modelo de linguagem, criado pelo Google.";
                        }
                        // Opções do menu
                        else if (lowerMessage == "1" || lowerMessage.Contains("coletar dados"))
                        {
                            _conversationState[userId] = ConversationState.AwaitingName; // Define o próximo estado
                            response = "Ótimo! Para começar, por favor, digite seu nome completo:";
                        }
                        else if (lowerMessage == "menu")
                        {
                            response = "Olá! Como posso ajudar você hoje?\n\n" +
                                       "Por favor, escolha uma opção digitando o número correspondente:\n" +
                                       "1 - Coletar meus dados (Nome, Email, Profissão)\n" +
                                       "2 - Fazer uma pergunta geral (em breve)\n" +
                                       "0 - Sair/Cancelar";
                        }
                        else
                        {
                            // Mensagem padrão se não for um comando conhecido e não estiver em um fluxo
                            response = "Bem-vindo! Digite 'menu' para ver as opções de como posso te ajudar.";
                        }
                        break;

                    case ConversationState.AwaitingName:
                        if (!string.IsNullOrWhiteSpace(userMessage) && userMessage.Length > 1)
                        {
                            currentUserData.Name = userMessage.Trim();
                            _conversationState[userId] = ConversationState.AwaitingEmail;
                            response = $"Obrigado, {currentUserData.Name}! Agora, por favor, digite seu email:";
                        }
                        else
                        {
                            response = "Parece que você não digitou um nome válido. Por favor, digite seu nome completo ou 'sair' para cancelar:";
                        }
                        break;

                    case ConversationState.AwaitingEmail:
                        if (Regex.IsMatch(userMessage.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        {
                            currentUserData.Email = userMessage.Trim();
                            _conversationState[userId] = ConversationState.AwaitingProfession;
                            response = $"Email '{currentUserData.Email}' registrado. Por fim, qual é a sua profissão?";
                        }
                        else
                        {
                            response = "Formato de email inválido. Por favor, digite um email válido ou 'sair' para cancelar:";
                        }
                        break;

                    case ConversationState.AwaitingProfession:
                        if (!string.IsNullOrWhiteSpace(userMessage) && userMessage.Length > 1)
                        {
                            currentUserData.Profession = userMessage.Trim();
                            _conversationState[userId] = ConversationState.None; // Reseta o estado para None (fim do fluxo)

                            response = $"Perfeito! Seus dados coletados:\n" +
                                       $"Nome: {currentUserData.Name}\n" +
                                       $"Email: {currentUserData.Email}\n" +
                                       $"Profissão: {currentUserData.Profession}\n" +
                                       "Obrigado por fornecer suas informações! Posso ajudar em algo mais? Digite 'menu' para ver as opções.";

                            _userData.TryRemove(userId, out _); // Limpa os dados do usuário após a conclusão do fluxo.
                        }
                        else
                        {
                            response = "Por favor, digite sua profissão ou 'sair' para cancelar:";
                        }
                        break;

                    default:
                        response = "Desculpe, algo deu errado com o estado da conversa. Por favor, digite 'menu' para ver as opções.";
                        _conversationState.TryRemove(userId, out _);
                        _userData.TryRemove(userId, out _);
                        break;
                }

                _logger.LogInformation($"Resposta simulada da IA: '{response}'");
                return response;
            }
        }
    }
}
