# Arquitetura da V1

## Decisão

A V1 será um monólito modular em ASP.NET Core Razor Pages. Site público e painel
administrativo serão publicados como uma única aplicação, com o painel protegido
na rota `/admin`.

## Projetos

- `OticaVisao.Web`: interface web, composição da aplicação e endpoints.
- `OticaVisao.Domain`: entidades e regras de negócio sem dependências externas.
- `OticaVisao.Infrastructure`: PostgreSQL, Entity Framework Core e serviços externos.
- `OticaVisao.Tests`: testes de regras, integração e arquitetura.

As dependências seguem este sentido:

```text
Web -> Domain
Web -> Infrastructure -> Domain
Tests -> aplicação sob teste
```

O domínio não pode depender da camada Web nem da Infrastructure.

## Limites desta fundação

Esta etapa não adiciona banco, autenticação, entidades do catálogo ou upload. Essas
dependências serão introduzidas junto ao primeiro caso de uso que realmente precisar
delas, evitando configuração e abstrações prematuras.
