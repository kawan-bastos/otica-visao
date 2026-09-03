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

## Itens, lentes e estoque

- cada item mantém uma fotografia do código, marca, modelo, cor e preços usados na venda;
- a venda aceita quantidade e vários modelos de armação;
- no óculos completo, a armação recebe automaticamente o valor promocional de R$ 39;
- lentes registram descrição, preço unitário e laboratório Padrão Optical ou Imperial Lab;
- armações sem lentes mantêm o preço do catálogo;
- adicionar um item reserva a quantidade no estoque imediatamente;
- remover o item devolve a quantidade ao estoque;
- alterações futuras no catálogo não modificam valores de rascunhos existentes.

## Próxima evolução

Serão implementados pagamento, conclusão e cancelamento. A conclusão revalidará os dados da venda; o cancelamento devolverá ao estoque todos os itens ainda reservados.
