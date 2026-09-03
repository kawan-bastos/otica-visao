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

Os estados são `Draft`, `Completed` e `Cancelled`, exibidos como “Em andamento”, “Concluída” e “Cancelada”.

## Itens, lentes e estoque

- cada item mantém uma fotografia do código, marca, modelo, cor e preços usados na venda;
- a venda aceita quantidade e vários modelos de armação;
- no óculos completo, a armação recebe automaticamente o valor promocional de R$ 39;
- lentes registram descrição, preço unitário e laboratório Padrão Optical ou Imperial Lab;
- armações sem lentes mantêm o preço do catálogo;
- adicionar um item reserva a quantidade no estoque imediatamente;
- remover o item devolve a quantidade ao estoque;
- alterações futuras no catálogo não modificam valores de rascunhos existentes.

## Pagamento e encerramento

- a venda pode ser concluída por Pix, cartão de débito ou cartão de crédito;
- crédito aceita de 1 a 10 parcelas sem juros; Pix e débito são registrados à vista;
- a conclusão exige pelo menos um item e preserva o valor final como histórico;
- a conclusão mantém o estoque já reservado, sem realizar uma segunda baixa;
- o cancelamento possui uma confirmação separada e devolve todas as unidades reservadas;
- vendas concluídas e canceladas ficam disponíveis somente para consulta, sem alteração de itens ou situação.

## Próxima evolução

O próximo passo é acompanhar o pedido enviado ao laboratório, incluindo laboratório responsável, datas e situação da produção das lentes.
