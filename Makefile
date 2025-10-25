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

.PHONY: docker-build docker-run docker-push

docker-build:
	docker build -t quotes-api:local .

docker-run:
	docker run --rm -p 8080:8080 \
	 -e ConnectionStrings__Default="Host=localhost;Port=5432;Database=quotes_dev;Username=postgres;Password=postgres" \
	 quotes-api:local

.PHONY: compose-up compose-down

compose-up:
	docker compose up -d --build

compose-down:
	docker compose down -v

.PHONY: kind-load k8s-apply

kind-load:
	docker build -t quotes-api:local .
	kind load docker-image quotes-api:local --name quotes

k8s-apply:
	kubectl apply -f k8s/namespace.yaml
	kubectl apply -f k8s/postgres.yaml
	kubectl apply -f k8s/api.yaml
	kubectl apply -f k8s/ingress.yaml

