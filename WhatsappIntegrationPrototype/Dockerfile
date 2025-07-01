# Etapa de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Limpa o cache NuGet dentro do contêiner Docker antes de qualquer operação NuGet
RUN dotnet nuget locals all --clear

# Copia o arquivo de projeto e o NuGet.config (agora na mesma pasta)
COPY ["WhatsappIntegrationPrototype.csproj", "nuget.config", "./"]
RUN dotnet restore "WhatsappIntegrationPrototype.csproj"

# Copia todo o código fonte restante
COPY . .

# Publica a aplicação
RUN dotnet publish "WhatsappIntegrationPrototype.csproj" -c Release -o /app/out --no-restore

# Etapa de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "WhatsappIntegrationPrototype.dll"]