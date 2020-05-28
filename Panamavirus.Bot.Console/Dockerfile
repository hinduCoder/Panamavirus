#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Panamavirus.Bot.Console.csproj", "Panamavirus.Bot.Console/"]
RUN dotnet restore "Panamavirus.Bot.Console/Panamavirus.Bot.Console.csproj"
COPY . .
RUN dotnet build "Panamavirus.Bot.Console.csproj" -c Release -o /app/build

#RUN dotnet tool install --global dotnet-ef
#RUN dotnet ef database update

FROM build AS publish
RUN dotnet publish "Panamavirus.Bot.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Panamavirus.Bot.Console.dll"]