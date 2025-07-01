# ... (início do Dockerfile)

# Etapa de Build: Usa a imagem do SDK do .NET para compilar a aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Limpa o cache NuGet (pode ajudar a evitar referências a caminhos locais)
RUN dotnet nuget locals all --clear

# Copia o arquivo de projeto e restaura as dependências NuGet
COPY ["WhatsappIntegrationPrototype.csproj", "./"]
COPY ["nuget.config", "./"]   
RUN dotnet restore "WhatsappIntegrationPrototype.csproj"

# Copia todo o código fonte restante
COPY . .

# Publica a aplicação para a pasta 'out'
RUN dotnet publish "WhatsappIntegrationPrototype.csproj" -c Release -o /app/out --no-restore

# ... (final do Dockerfile)