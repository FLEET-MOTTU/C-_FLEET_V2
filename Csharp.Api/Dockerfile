FROM mcr.microsoft.com/dotnet/sdk:8.0 AS ef-tools-env
RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="/root/.dotnet/tools:${PATH}"
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
COPY Csharp.Api.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "Csharp.Api.dll"]
