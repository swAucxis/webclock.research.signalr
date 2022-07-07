FROM mcr.microsoft.com/dotnet/aspnet:6.0

COPY ./DockerBuild ./app

WORKDIR /app

ENTRYPOINT  ["dotnet", "Webclock.Research.SignalR.dll"]