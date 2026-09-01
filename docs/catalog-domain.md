# Domínio do catálogo

## Identificadores

Cada registro possui dois identificadores com finalidades diferentes:

- `Id`: identificador técnico único gerado pelo sistema.
- `Code`: código da etiqueta usado pela loja.

Unidades idênticas compartilham o mesmo código e são controladas pelo campo
`StockQuantity`. A unicidade do código entre produtos diferentes será garantida
pelo banco de dados na etapa de persistência.

## Disponibilidade pública

Uma armação está disponível somente quando as três condições forem atendidas:

```text
ativa + publicada + quantidade maior que zero
```

Quando a última unidade é retirada do estoque, a armação fica automaticamente
indisponível. Desativar uma armação também remove sua publicação.

## Preço e promoção

`Price` representa o preço normal da armação. A condição promocional de R$ 39 na
compra dos óculos completos não altera esse valor e será modelada separadamente
quando o sistema precisar administrar promoções.

## Fora desta etapa

- Entity Framework Core e PostgreSQL.
- Unicidade persistida do código.
- Upload de fotografias.
- Painel administrativo.
- Pedidos e laboratórios.
