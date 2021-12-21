debug: fmt build test

fmt:
	dotnet format src/**/*.csproj
	dotnet format tests/*.csproj

build:
	dotnet build src/**/*.csproj

test:
	dotnet test	tests

.PHONEY: debug build fmt test
