using Microsoft.AspNetCore.Mvc;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using WhatsappIntegrationPrototype.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http; // Necessário para HttpContext

namespace WhatsappIntegrationPrototype.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    // Não herdamos mais de TwilioController
    public class WhatsappController : ControllerBase // Herdamos de ControllerBase para funcionalidades de API
    {
        private readonly ILogger<WhatsappController> _logger;
        private readonly IConfiguration _configuration;
        private readonly AIService _aiService;
        private readonly string _twilioPhoneNumber;
        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;

        public WhatsappController(ILogger<WhatsappController> logger, IConfiguration configuration, AIService aiService)
        {
            _logger = logger;
            _configuration = configuration;
            _aiService = aiService;

            // Carrega as credenciais do Twilio das configurações
            _twilioAccountSid = _configuration["Twilio:AccountSid"];
            _twilioAuthToken = _configuration["Twilio:AuthToken"];
            _twilioPhoneNumber = _configuration["Twilio:TwilioPhoneNumber"];

            // Inicializa o cliente Twilio globalmente.
            // É importante que isso seja feito apenas uma vez na aplicação.
            // O ideal é que essa inicialização esteja no Program.cs ou em um serviço Singleton.
            // Por simplicidade do protótipo, estamos fazendo aqui no construtor.
            Twilio.TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            // O Twilio envia os dados como form-urlencoded.
            // Acessamos o corpo da requisição através de HttpContext.Request.Form.
            var incomingMessage = HttpContext.Request.Form["Body"].ToString();
            var fromNumber = HttpContext.Request.Form["From"].ToString();

            _logger.LogInformation($"Mensagem recebida de {fromNumber}: {incomingMessage}");

            // 1. Processar a mensagem com o Serviço de IA
            string iaResponseText = await _aiService.ProcessMessageWithAI(incomingMessage, fromNumber);

            // 2. Enviar a resposta de volta para o usuário via Twilio WhatsApp API
            try
            {
                var message = await MessageResource.CreateAsync(
                    to: new PhoneNumber(fromNumber),
                    from: new PhoneNumber(_twilioPhoneNumber),
                    body: iaResponseText
                );
                _logger.LogInformation($"Mensagem enviada para {fromNumber}. SID: {message.Sid}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem via Twilio WhatsApp API. Verifique suas credenciais e o formato do número.");
                // Em caso de erro ao enviar para o Twilio, podemos retornar um erro 500 para o Twilio.
                // Mas, como já estamos respondendo ao usuário, talvez não seja estritamente necessário.
                // Para o protótipo, apenas logamos o erro.
            }

            // Retorna um OK vazio. O Twilio precisa de uma resposta HTTP 200 OK para saber que o webhook foi processado.
            // Não precisamos retornar TwiML se estamos enviando a mensagem programaticamente.
            return Ok();
        }
    }
}