WhatsApp Integration Prototype







Protótipo de uma API de atendimento automatizado pelo WhatsApp, desenvolvido com ASP.NET Core 8, Twilio e Google Gemini.

A aplicação recebe mensagens enviadas ao WhatsApp Sandbox da Twilio, processa o conteúdo por meio do Google Gemini ou de um fluxo conversacional local e envia a resposta ao usuário pelo próprio WhatsApp.

Projeto experimental e educacional. Antes de utilizá-lo em produção, implemente as recomendações de segurança, privacidade, persistência e observabilidade descritas neste documento.

Funcionalidades

Recebimento de mensagens por webhook da Twilio;

Envio de respostas pela API do WhatsApp da Twilio;

Integração opcional com o Google Gemini;

Funcionamento sem Gemini por meio de um fluxo conversacional local;

Controle de estado individual por número de telefone;

Coleta sequencial de nome, e-mail e profissão;

Validação básica do formato de e-mail;

Comandos menu, sair, cancelar e 0;

Swagger/OpenAPI para inspeção da API;

Dockerfile multi-stage para build e publicação.

Como funciona

sequenceDiagram
    participant U as Usuário
    participant W as WhatsApp
    participant T as Twilio
    participant A as ASP.NET Core API
    participant G as Google Gemini

    U->>W: Envia mensagem
    W->>T: Entrega ao Sandbox
    T->>A: POST /api/whatsapp/webhook
    alt Gemini configurado
        A->>G: Processa mensagem
        G-->>A: Resposta da IA
    else Gemini não configurado
        A->>A: Executa fluxo local
    end
    A->>T: Envia resposta
    T->>W: Entrega mensagem
    W-->>U: Exibe resposta

Fluxo conversacional local

Quando a chave do Gemini não está configurada, o serviço utiliza uma máquina de estados mantida em memória:

stateDiagram-v2
    [*] --> Menu
    Menu --> AguardandoNome: opção 1
    AguardandoNome --> AguardandoEmail: nome válido
    AguardandoEmail --> AguardandoProfissao: e-mail válido
    AguardandoProfissao --> Menu: cadastro concluído
    Menu --> Menu: sair ou cancelar

O estado de cada conversa utiliza o número de origem como identificador. As informações temporárias são armazenadas em ConcurrentDictionary e removidas quando o fluxo termina ou é cancelado.

Tecnologias e pacotes

Tecnologia

Finalidade

.NET 8 / ASP.NET Core

API Web e injeção de dependência

C#

Implementação da aplicação e do fluxo conversacional

Twilio SDK 7.11.3

Recebimento e envio de mensagens do WhatsApp

Google_GenerativeAI 2.6.0

Integração com o Google Gemini

Swashbuckle 6.6.2

Documentação Swagger/OpenAPI

Docker

Empacotamento e execução em contêiner

Estrutura do projeto

WhatssapSend/
├── WhatsappIntegrationPrototype.sln
└── WhatsappIntegrationPrototype/
    ├── Controllers/
    │   ├── WhatsappController.cs
    │   └── WeatherForecastController.cs
    ├── Services/
    │   └── AIService.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Program.cs
    ├── appsettings.json
    ├── Dockerfile
    └── WhatsappIntegrationPrototype.csproj

Componentes principais

WhatsappController: recebe o webhook, extrai Body e From, solicita o processamento da mensagem e envia a resposta pela Twilio;

AIService: chama o Gemini quando existe uma chave configurada ou executa o fluxo conversacional local;

Program.cs: registra controllers, Swagger e o serviço de IA;

Dockerfile: compila e publica a aplicação usando imagens oficiais do .NET 8.

Pré-requisitos

.NET SDK 8;

Conta na Twilio;

WhatsApp Sandbox habilitado na Twilio;

Chave do Google Gemini, caso deseje utilizar IA generativa;

Um túnel HTTPS, como ngrok ou Cloudflare Tunnel, para testes locais do webhook;

Docker, opcionalmente.

Configuração segura

Não grave credenciais reais em appsettings.json ou appsettings.Development.json. Utilize variáveis de ambiente, User Secrets ou um gerenciador de segredos.

As configurações esperadas pela aplicação são:

Variável

Descrição

Obrigatória

Twilio__AccountSid

Identificador da conta Twilio

Sim

Twilio__AuthToken

Token de autenticação da Twilio

Sim

Twilio__TwilioPhoneNumber

Número do WhatsApp Sandbox no formato whatsapp:+...

Sim

GeminiAI__ApiKey

Chave da API Google Gemini

Não

PowerShell

$env:Twilio__AccountSid="seu-account-sid"
$env:Twilio__AuthToken="seu-auth-token"
$env:Twilio__TwilioPhoneNumber="whatsapp:+14155238886"
$env:GeminiAI__ApiKey="sua-chave-gemini"

Windows CMD

set Twilio__AccountSid=seu-account-sid
set Twilio__AuthToken=seu-auth-token
set Twilio__TwilioPhoneNumber=whatsapp:+14155238886
set GeminiAI__ApiKey=sua-chave-gemini

Linux ou macOS

export Twilio__AccountSid="seu-account-sid"
export Twilio__AuthToken="seu-auth-token"
export Twilio__TwilioPhoneNumber="whatsapp:+14155238886"
export GeminiAI__ApiKey="sua-chave-gemini"

User Secrets no desenvolvimento

Dentro da pasta WhatsappIntegrationPrototype:

dotnet user-secrets init
dotnet user-secrets set "Twilio:AccountSid" "seu-account-sid"
dotnet user-secrets set "Twilio:AuthToken" "seu-auth-token"
dotnet user-secrets set "Twilio:TwilioPhoneNumber" "whatsapp:+14155238886"
dotnet user-secrets set "GeminiAI:ApiKey" "sua-chave-gemini"

Executando localmente

Clone o repositório:

git clone https://github.com/jeanalgoritimo/WhatssapSend.git
cd WhatssapSend/WhatsappIntegrationPrototype

Configure as credenciais usando uma das opções anteriores.

Restaure os pacotes:

dotnet restore

Execute a API:

dotnet run

Acesse o Swagger:

https://localhost:7128/swagger

A aplicação também possui o perfil HTTP em http://localhost:5062.

Configurando o webhook da Twilio

O webhook implementado pelo projeto é:

POST /api/whatsapp/webhook
Content-Type: application/x-www-form-urlencoded

Para testá-lo localmente:

Inicie a API;

Exponha a porta local usando um túnel HTTPS;

No painel da Twilio, abra a configuração do WhatsApp Sandbox;

No campo When a message comes in, informe:

https://seu-endereco-publico/api/whatsapp/webhook

Selecione o método POST;

Envie menu ou oi para o número do Sandbox.

Exemplos de conversa

Usuário: menu

Assistente:
1 - Coletar meus dados (Nome, Email, Profissão)
2 - Fazer uma pergunta geral (em breve)
0 - Sair/Cancelar

Usuário: 1
Assistente: Para começar, digite seu nome completo.

Usuário: Maria Silva
Assistente: Agora, digite seu e-mail.

Usuário: maria@example.com
Assistente: Por fim, qual é sua profissão?

Usuário: Desenvolvedora
Assistente: Exibe o resumo e encerra o fluxo.

Executando com Docker

Na raiz do repositório:

docker build -t whatsapp-integration ./WhatsappIntegrationPrototype

docker run --rm -p 8080:80 \
  -e ASPNETCORE_URLS=http://+:80 \
  -e Twilio__AccountSid="seu-account-sid" \
  -e Twilio__AuthToken="seu-auth-token" \
  -e Twilio__TwilioPhoneNumber="whatsapp:+14155238886" \
  -e GeminiAI__ApiKey="sua-chave-gemini" \
  whatsapp-integration

Swagger no contêiner:

http://localhost:8080/swagger

Segurança e privacidade

Antes de usar a aplicação fora de um ambiente de estudo:

Revogue imediatamente qualquer credencial que tenha sido publicada no GitHub;

Remova segredos também do histórico Git, não apenas do commit atual;

Valide a assinatura enviada pela Twilio antes de aceitar o webhook;

Não registre números, mensagens ou dados pessoais integralmente nos logs;

Adicione autenticação e autorização aos endpoints administrativos;

Aplique rate limiting e proteção contra abuso;

Defina política de retenção e tratamento de dados conforme a LGPD;

Restrinja o Swagger em produção;

Configure timeouts, retentativas e circuit breaker para serviços externos;

Utilize um cofre de segredos em produção.

Limitações atuais

O estado das conversas fica somente na memória;

Reiniciar a aplicação apaga todas as conversas em andamento;

Múltiplas instâncias não compartilham o mesmo estado;

Os dados coletados não são persistidos;

A assinatura do webhook da Twilio ainda não é validada;

O Swagger está habilitado em todos os ambientes;

O serviço registra mensagens e números nos logs;

O fluxo local possui apenas coleta de nome, e-mail e profissão;

A opção de pergunta geral do menu ainda não foi implementada;

O projeto ainda contém arquivos padrão do template, como WeatherForecast.

Roadmap

Webhook para mensagens da Twilio;

Resposta pelo WhatsApp Sandbox;

Fluxo conversacional com máquina de estados;

Integração opcional com Google Gemini;

Dockerfile multi-stage;

Validar assinatura do webhook da Twilio;

Migrar credenciais para Secret Manager;

Persistir conversas e dados em banco;

Adicionar Redis para estado distribuído;

Implementar testes unitários e de integração;

Remover código padrão WeatherForecast;

Restringir Swagger por ambiente;

Adicionar observabilidade e métricas;

Criar pipeline de CI/CD.

Aviso sobre o WhatsApp

Este projeto utiliza o WhatsApp por meio da plataforma Twilio. Ele não é afiliado, patrocinado ou endossado pela WhatsApp LLC ou pela Meta. Para uso comercial, verifique os termos, políticas, modelos de mensagem e regras vigentes da Twilio e do WhatsApp Business.

Autor

Jean Paiva da Silva

Desenvolvedor de Software com experiência em .NET, C#, APIs, integrações, mensageria, cloud e modernização de sistemas, atualmente aprofundando conhecimentos em Engenharia de Inteligência Artificial.
 
