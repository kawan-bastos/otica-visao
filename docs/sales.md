# Vendas

O módulo de vendas começa com um rascunho persistente ligado obrigatoriamente a uma ficha de cliente. Um rascunho representa um atendimento iniciado, não uma venda concluída.

## Escopo desta etapa

- acesso restrito ao painel administrativo;
- listagem de vendas e situação atual;
- busca do cliente por nome, telefone, CPF ou e-mail;
- seleção de uma ficha existente;
- cadastro do cliente dentro do fluxo da nova venda;
- criação transacional da ficha e do rascunho quando o cliente é novo;
- proteção do vínculo histórico: uma ficha com venda não pode ser apagada pelo banco.

Os estados iniciais são `Draft`, `Completed` e `Cancelled`. Nesta etapa, apenas `Draft` é criado e exibido como “Em andamento”.

## Próxima evolução

O rascunho receberá itens de armação, dados de lentes e laboratório. Depois serão implementados promoção, pagamento, conclusão e baixa automática de estoque.
