# Catálogo público

O catálogo está disponível em `/Frames` e exibe somente armações que atendem às
três condições: produto ativo, publicação habilitada e estoque maior que zero.

## Recursos

- busca por código, marca, modelo ou cor;
- filtros por tipo, formato e público adulto ou infantil;
- página de detalhes de cada armação;
- foto principal ou indicação de foto ainda não cadastrada;
- medidas quando estiverem disponíveis;
- botão do WhatsApp com código e modelo preenchidos na mensagem;
- seleção de até quatro produtos disponíveis na Home.

Os filtros são aplicados diretamente na consulta ao PostgreSQL. Uma URL antiga de
produto deixa de abrir quando a armação é desativada, despublicada ou fica sem
estoque, evitando divulgar uma opção indisponível.

O catálogo serve para descoberta e contato. Preço e estoque devem ser confirmados
com a equipe, e a escolha final continua presencial para experimentação, medidas e
ajustes.
