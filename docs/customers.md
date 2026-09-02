# Cadastro interno de clientes

O cadastro de clientes é uma ficha administrativa independente das contas usadas para entrar no site. Assim, um cliente atendido presencialmente pode ser registrado sem possuir senha ou conta online.

## Escopo atual

- acesso restrito à função administrativa;
- busca por nome, telefone ou e-mail;
- cadastro e edição de nome, telefone, e-mail opcional e observações;
- atalho do telefone para iniciar uma conversa no WhatsApp;
- datas de criação e última atualização registradas em UTC;
- persistência na tabela `customers` do PostgreSQL.

As observações são destinadas apenas a lembretes comuns de atendimento. Receitas, prescrições e dados médicos não devem ser registrados nesse campo. Esses dados exigirão uma etapa específica de segurança e LGPD.

## Separação das contas online

`ApplicationUser` representa uma identidade que pode autenticar no site. `Customer` representa a ficha comercial mantida pela ótica. Uma associação opcional entre esses registros poderá ser criada futuramente quando houver um fluxo real que necessite dela, como acompanhamento de pedidos pelo cliente.
