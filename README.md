# Ótica Visão

Sistema web da Ótica Visão de Piabetá. A V1 terá um catálogo público de armações e
um painel administrativo responsivo para manutenção dos produtos.

## Estado atual

Protótipo visual responsivo da Home concluído, ainda com produtos, preços e dados
de contato ilustrativos. O projeto não deve ser publicado em produção.

## Tecnologias

- .NET 10 LTS
- ASP.NET Core Razor Pages
- xUnit
- PostgreSQL e Entity Framework Core serão adicionados na etapa de persistência

## Estrutura

```text
src/
  OticaVisao.Web/
  OticaVisao.Domain/
  OticaVisao.Infrastructure/
tests/
  OticaVisao.Tests/
```

As responsabilidades e a direção das dependências estão descritas em
[`docs/architecture.md`](docs/architecture.md).

## Executar localmente

Requer o SDK .NET definido em `global.json`.

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet run --project src/OticaVisao.Web
```
