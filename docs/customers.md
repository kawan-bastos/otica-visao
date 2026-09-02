# Cadastro interno de clientes

O cadastro de clientes é uma ficha administrativa independente das contas usadas para entrar no site. Assim, um cliente atendido presencialmente pode ser registrado sem possuir senha ou conta online.

## Escopo atual

- acesso restrito à função administrativa;
- busca por nome, telefone, e-mail ou CPF;
- cadastro e edição de nome, telefone, CPF, data de nascimento, endereço completo, e-mail opcional e observações;
- validação dos dígitos verificadores do CPF e bloqueio de CPF duplicado;
- atalho do telefone para iniciar uma conversa no WhatsApp;
- datas de criação e última atualização registradas em UTC;
- persistência na tabela `customers` do PostgreSQL.

As observações são destinadas apenas a lembretes comuns de atendimento. Receitas, prescrições e dados médicos não devem ser registrados nesse campo. Esses dados exigirão uma etapa específica de segurança e LGPD.

O CPF é armazenado somente com números. CPF, data de nascimento e endereço são dados pessoais e ficam disponíveis apenas no painel administrativo. A migração mantém as novas colunas opcionais no banco para não invalidar fichas criadas anteriormente, mas a tela exige o preenchimento ao criar ou atualizar um cliente.

## Separação das contas online

`ApplicationUser` representa uma identidade que pode autenticar no site. `Customer` representa a ficha comercial mantida pela ótica. Uma associação opcional entre esses registros poderá ser criada futuramente quando houver um fluxo real que necessite dela, como acompanhamento de pedidos pelo cliente.
