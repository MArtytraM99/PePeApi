#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["PePe.API/PePe.API.csproj", "PePe.API/"]
COPY ["Pepe.Base/PePe.Base.csproj", "Pepe.Base/"]
COPY ["Pepe.Service/PePe.Service.csproj", "Pepe.Service/"]
RUN dotnet restore "PePe.API/PePe.API.csproj"
COPY . .
WORKDIR "/src/PePe.API"
RUN dotnet build "PePe.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PePe.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PePe.API.dll"]