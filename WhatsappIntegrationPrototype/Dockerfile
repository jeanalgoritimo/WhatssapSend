# Etapa de Build: Usa a imagem do SDK do .NET para compilar a aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Limpa o cache NuGet dentro do contêiner Docker antes de qualquer operação NuGet
RUN dotnet nuget locals all --clear

# Copia o arquivo de projeto e o NuGet.config (AGORA na mesma pasta, como discutido antes)
# Certifique-se de que o nuget.config que você moveu para esta pasta está LIMPO
COPY ["WhatsappIntegrationPrototype.csproj", "nuget.config", "./"]
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
