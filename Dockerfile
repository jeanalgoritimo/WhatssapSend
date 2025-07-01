# Etapa de Build: Usa a imagem do SDK do .NET para compilar a aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências NuGet
COPY ["WhatsappIntegrationPrototype.csproj", "./"]
RUN dotnet restore "WhatsappIntegrationPrototype.csproj"

# Copia todo o código fonte restante
COPY . .

# Publica a aplicação para a pasta 'out'
RUN dotnet publish "WhatsappIntegrationPrototype.csproj" -c Release -o /app/out --no-restore

# Etapa de Runtime: Usa a imagem do runtime do .NET para executar a aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia os arquivos publicados da etapa de build
COPY --from=build /app/out .

# Expõe a porta 80 (padrão para aplicações web)
EXPOSE 80

# Define o ponto de entrada da aplicação
ENTRYPOINT ["dotnet", "WhatsappIntegrationPrototype.dll"]