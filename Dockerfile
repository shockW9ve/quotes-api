# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /src
COPY ./Quotes.sln ./
COPY ./src/Quotes.Api/Quotes.Api.csproj ./src/Quotes.Api/
COPY ./tests/Quotes.Api.Tests/Quotes.Api.Tests.csproj ./tests/Quotes.Api.Tests/
RUN dotnet restore
COPY . .
RUN dotnet publish src/Quotes.Api/Quotes.Api.csproj -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Quotes.Api.dll"]

