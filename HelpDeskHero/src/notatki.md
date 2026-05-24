# Utworzenie projektu .NET dla aplikacji HelpDeskHero
## Utworzenie solution
dotnet new sln -n HelpDeskHero
służy do utworzenia nowego solution (rozwiązania) w ekosystemie .NET.
To jest główny plik, który:
- grupuje projekty (.csproj)
- pozwala zarządzać całą aplikacją

Solution to kontener na wiele projektów, które mogą być ze sobą powiązane. Na przykład, w aplikacji HelpDeskHero.
np.:
HelpDeskHero.sln
 ├── HelpDeskHero.API
 ├── HelpDeskHero.Application
 ├── HelpDeskHero.Domain
 └── HelpDeskHero.Infrastructure

## Utworzenie projektów
dotnet new classlib -n HelpDeskHero.Shared -o .\src\HelpDeskHero.Shared -f net10.0
dotnet new webapi -n HelpDeskHero.Api -o .\src\HelpDeskHero.Api -f net10.0
dotnet new blazorwasm -n HelpDeskHero.UI -o .\src\HelpDeskHero.UI -f net10.0

## Dodanie projektów do solution
dotnet sln .\HelpDeskHero.slnx add .\src\HelpDeskHero.Shared\HelpDeskHero.Shared.csproj
dotnet sln .\HelpDeskHero.slnx add .\src\HelpDeskHero.Api\HelpDeskHero.Api.csproj
dotnet sln .\HelpDeskHero.slnx add .\src\HelpDeskHero.UI\HelpDeskHero.UI.csproj

## Dodanie referencji między projektami
dotnet add .\src\HelpDeskHero.Api\HelpDeskHero.Api.csproj reference .\src\HelpDeskHero.Shared\HelpDeskHero.Shared.csproj
dotnet add .\src\HelpDeskHero.UI\HelpDeskHero.UI.csproj reference .\src\HelpDeskHero.Shared\HelpDeskHero.Shared.csproj

## Dodanie pakietów NuGet do projektu API
dotnet add .\src\HelpDeskHero.Api\HelpDeskHero.Api.csproj package Swashbuckle.AspNetCore
dotnet add .\src\HelpDeskHero.Api\HelpDeskHero.Api.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add .\src\HelpDeskHero.Api\HelpDeskHero.Api.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add .\src\HelpDeskHero.Api\HelpDeskHero.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer

Nuget to system zarządzania pakietami dla platformy .NET. Pozwala na łatwe dodawanie, aktualizowanie i usuwanie bibliotek (pakietów) w projekcie. Pakiety te mogą zawierać funkcjonalności, które ułatwiają rozwój aplikacji.


## Błędy
warn: Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer[8]
      The ASP.NET Core developer certificate is not trusted. For information about trusting the ASP.NET Core developer certificate, see https://aka.ms/aspnet/https-trust-dev-cert
naprawa komendą: 
dotnet dev-certs https --clean
dotnet dev-certs https --trust