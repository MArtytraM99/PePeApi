#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PePe.API/PePe.API.csproj", "PePe.API/"]
COPY ["PePe.Base/PePe.Base.csproj", "PePe.Base/"]
COPY ["PePe.Manager/PePe.Manager.csproj", "PePe.Manager/"]
COPY ["PePe.DAO/PePe.DAO.csproj", "PePe.DAO/"]
COPY ["PePe.Service/PePe.Service.csproj", "PePe.Service/"]
RUN dotnet restore "PePe.API/PePe.API.csproj"
COPY . .
WORKDIR "/src/PePe.API"
RUN dotnet build "PePe.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PePe.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PePe.API.dll"]