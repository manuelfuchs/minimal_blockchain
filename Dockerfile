FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
COPY published ./app
ENTRYPOINT ["dotnet", "app/MinimalBlockchain.Api.dll"]