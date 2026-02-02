FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Cambrian.sln ./
COPY src ./src
COPY tests ./tests

RUN dotnet restore "src/Cambrian.Api/Cambrian.Api.csproj"
RUN dotnet publish "src/Cambrian.Api/Cambrian.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Use PORT environment variable from Render, fallback to 3000 for local/AWS
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-3000}
EXPOSE ${PORT:-3000}

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Cambrian.Api.dll"]
