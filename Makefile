.PHONY: run pack docker-build docker-run

run:
	dotnet run --project Shop.csproj

pack:
	dotnet run --project Shop.csproj -- --pack

docker-build:
	docker build -t shop-csharp .

docker-run:
	docker run --rm -p 127.0.0.1:8000:8000 shop-csharp
