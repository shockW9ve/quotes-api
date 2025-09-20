.PHONY: restore build test run watch clean format

restore:
	dotnet restore Quotes.sln

build: restore
	dotnet build Quotes.sln -c Debug --nologo

test:
	dotnet test tests/Quotes.Api.Tests/Quotes.Api.Tests.csproj --nologo

run:
	dotnet run --project src/Quotes.Api/Quotes.Api.csproj --no-launch-profile

watch:
	dotnet watch --project src/Quotes.Api/Quotes.Api.csproj run

clean:
	dotnet clean Quotes.sln

format:
	dotnet format
