# Validação de usabilidade da V1

Esta etapa verifica o caminho principal do site público e do painel em execução
local com PostgreSQL.

## Cenários verificados

- Home, catálogo, detalhes, páginas institucionais e privacidade respondem sem erro;
- catálogo lista somente armações publicadas, ativas e com estoque;
- filtros de busca, tipo, formato e público mantêm os resultados esperados;
- acesso anônimo ao painel redireciona para o login;
- login apresenta campos e mensagens acessíveis;
- navegação indica a página atual;
- Home, catálogo e login se adaptam a viewport de tablet;
- listagem administrativa troca a tabela larga por cartões em telas pequenas;
- formulários de cadastro e edição usam controles adequados para toque.

## Validação antes da produção

Antes da publicação, o responsável pela loja deve testar no tablet real o login,
o cadastro de uma armação com foto, a edição de estoque e a saída do painel. Esse
teste confirma também câmera, seletor de arquivos e comportamento do navegador do
dispositivo, que não podem ser reproduzidos integralmente no computador.
