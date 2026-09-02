# Ótica Visão

Sistema web da Ótica Visão de Piabetá. A V1 terá um catálogo público de armações e
um painel administrativo responsivo para manutenção dos produtos.

## Estado atual

Home responsiva, domínio, persistência e manutenção administrativa do catálogo
concluídos. O painel possui autenticação, mas os produtos ainda são fictícios e a
configuração de produção ainda não foi realizada.

## Tecnologias

- .NET 10 LTS
- ASP.NET Core Razor Pages
- xUnit
- PostgreSQL
- Entity Framework Core com o provedor Npgsql

## Estrutura

```text
src/
  OticaVisao.Web/
  OticaVisao.Application/
  OticaVisao.Domain/
  OticaVisao.Infrastructure/
tests/
  OticaVisao.Tests/
```

As responsabilidades e a direção das dependências estão descritas em
[`docs/architecture.md`](docs/architecture.md).
O contexto do banco, a configuração local e os comandos de migration estão em
[`docs/persistence.md`](docs/persistence.md).
Os casos de uso e a listagem administrativa estão em
[`docs/catalog-application.md`](docs/catalog-application.md).
A proteção do painel e a criação segura do acesso administrativo estão em
[`docs/authentication.md`](docs/authentication.md).

## Executar localmente

Requer o SDK .NET definido em `global.json`.

```powershell
dotnet restore
dotnet tool restore
dotnet build --no-restore
dotnet test --no-build
dotnet run --project src/OticaVisao.Web
```
