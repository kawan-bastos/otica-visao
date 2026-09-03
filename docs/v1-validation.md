# Validação de usabilidade da V1

## Validação final — 03/09/2026

- os 90 testes existentes passaram em configuração `Release`;
- Home, catálogo, filtros, detalhes, páginas institucionais, privacidade, login, cadastro e health check responderam sem erro no sistema local;
- acesso anônimo ao painel redirecionou corretamente para o login administrativo;
- Home, catálogo, login do cliente e login administrativo foram revisados no navegador sem erros visuais aparentes;
- foi adicionada uma bateria permanente de smoke tests para as rotas públicas essenciais e para detalhes de armações existentes e inexistentes;
- backup e restauração já foram comprovados em banco isolado;
- configurações inseguras impedem a inicialização em produção.

Esta validação não cria, altera ou exclui dados reais. O teste manual dos fluxos administrativos com o responsável da loja permanece como aceite final, pois envolve decisões comerciais e uso no tablet real.

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
