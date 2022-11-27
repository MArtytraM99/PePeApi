#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
COPY bin/Release/netcoreapp3.1/publish/ app/
WORKDIR /app

CMD ASPNETCORE_URLS=http://*:$PORT dotnet PePe.API.dll
