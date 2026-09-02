# Arquitetura da V1

## Decisão

A V1 será um monólito modular em ASP.NET Core Razor Pages. Site público e painel
administrativo serão publicados como uma única aplicação, com o painel protegido
na rota `/admin`.

## Projetos

- `OticaVisao.Web`: interface web, composição da aplicação e endpoints.
- `OticaVisao.Application`: casos de uso e contratos necessários ao catálogo.
- `OticaVisao.Domain`: entidades e regras de negócio sem dependências externas.
- `OticaVisao.Infrastructure`: PostgreSQL, Entity Framework Core e serviços externos.
- `OticaVisao.Tests`: testes de regras, integração e arquitetura.

As dependências seguem este sentido:

```text
Web -> Application -> Domain
Web -> Infrastructure -> Application
Infrastructure -> Domain
Tests -> aplicação sob teste
```

O domínio não pode depender da camada Web nem da Infrastructure.

## Estado atual

O catálogo já possui domínio, persistência PostgreSQL e casos de uso. O painel ainda
não possui autenticação, formulários de manutenção ou upload de imagens; esses itens
serão adicionados em etapas próprias.
