# Cadastro interno de clientes

O cadastro de clientes é a ficha comercial usada no atendimento e nas vendas. Ela pode nascer de uma conta criada pelo próprio cliente no site ou de um atendimento presencial.

## Escopo atual

- acesso restrito à função administrativa;
- busca por nome, telefone, e-mail ou CPF;
- cadastro e edição de nome, telefone, CPF, data de nascimento, endereço completo, e-mail opcional e observações;
- validação dos dígitos verificadores do CPF e bloqueio de CPF duplicado;
- atalho do telefone para iniciar uma conversa no WhatsApp;
- datas de criação e última atualização registradas em UTC;
- persistência na tabela `customers` do PostgreSQL.
- vínculo opcional e exclusivo com uma conta do site;
- identificação da origem da ficha no painel: conta do site ou atendimento presencial.
- preenchimento assistido de rua, bairro, cidade e UF pela consulta do CEP ao ViaCEP, mantendo todos os campos editáveis e permitindo preenchimento manual se o serviço estiver indisponível.
- a área de clientes é destinada à consulta e edição; não oferece cadastro administrativo isolado.

As observações são destinadas apenas a lembretes comuns de atendimento. Receitas, prescrições e dados médicos não devem ser registrados nesse campo. Esses dados exigirão uma etapa específica de segurança e LGPD.

O CPF é armazenado somente com números. CPF, data de nascimento e endereço são dados pessoais e ficam disponíveis apenas no painel administrativo. A migração mantém as novas colunas opcionais no banco para não invalidar fichas criadas anteriormente, mas a tela exige o preenchimento ao criar ou atualizar um cliente.

## Integração com as contas online

`ApplicationUser` continua representando a identidade que pode entrar no site, enquanto `Customer` representa a ficha comercial. Os registros são diferentes, mas ficam ligados pelo identificador interno da conta.

Ao se cadastrar no site, o cliente informa CPF, data de nascimento, telefone e endereço. Se não houver ficha com o CPF, o sistema cria a conta e a ficha juntas. Se a ficha já tiver sido criada durante uma venda presencial, o vínculo só é permitido quando CPF, telefone e data de nascimento coincidirem e a ficha ainda não estiver associada. A operação usa uma transação: conta e ficha são gravadas juntas ou nenhuma delas é gravada.

O cadastro presencial acontece exclusivamente dentro de “Nova venda”, depois que a busca confirma que o cliente ainda não existe. Contas criadas no site continuam gerando uma ficha mesmo antes da primeira compra.

Quando um cliente exclui sua conta pelo site, a ligação com `ApplicationUser` é removida e a ficha permanece no painel como atendimento presencial. Isso preserva a base necessária para o futuro histórico de vendas, garantias e obrigações comerciais.
