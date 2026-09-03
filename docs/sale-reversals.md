# Estorno de vendas

Vendas concluídas não são apagadas diretamente. O estorno preserva o registro da operação e exige um motivo para auditoria.

Ao estornar:

- a venda recebe a situação “Estornada”, com data, motivo e identificação do administrador responsável;
- o valor deixa automaticamente de participar dos relatórios, que consideram apenas vendas concluídas;
- todas as quantidades da venda retornam ao estoque;
- pedidos de lentes relacionados são cancelados e recebem uma entrada no histórico;
- a venda não pode ser alterada ou estornada novamente.

Depois do estorno, o administrador pode excluir permanentemente o registro. Essa segunda ação possui uma confirmação própria e também remove os pedidos de laboratório relacionados. A ficha do cliente é preservada.
