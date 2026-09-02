# Casos de uso do catálogo

## Operação diária do estoque

O painel administrativo apresenta totais de produtos disponíveis, com estoque
baixo e sem estoque. A busca localiza por código, marca, modelo ou cor, enquanto
o filtro separa as principais situações do catálogo.

Os botões `−` e `+` retiram ou adicionam uma unidade diretamente na listagem. A
operação passa pelas regras do domínio: o estoque nunca fica negativo e, ao chegar
a zero, a armação deixa automaticamente de ser considerada disponível no catálogo
público. O cadastro não é excluído e pode receber novas unidades depois.

A camada `OticaVisao.Application` fica entre a interface web e a persistência. Ela
coordena os casos de uso sem conhecer Razor Pages, Entity Framework ou PostgreSQL.

## Operações disponíveis

O `FrameCatalogService` permite:

- listar as armações em ordem de marca e modelo;
- cadastrar uma armação, impedindo códigos repetidos;
- alterar o preço de uma armação;
- adicionar unidades ao estoque.
- carregar e editar todos os dados de uma armação existente.

Os códigos são armazenados em letras maiúsculas. Unidades iguais continuam usando
o mesmo código e aumentam a quantidade em estoque.

O `FrameRepository` implementa o contrato da aplicação usando o
`ApplicationDbContext`. Essa separação permite testar as regras sem precisar abrir
uma conexão com o PostgreSQL.

## Dados iniciais

A migration `SeedCatalog` adiciona cinco armações fictícias para desenvolvimento.
Elas usam códigos de `ARM-001` a `ARM-005`, preço de R$ 219 e estoque unitário. Esses
registros não representam o estoque real da loja e podem ser substituídos depois.

## Consulta administrativa

A rota `/Admin/Frames` apresenta a listagem do painel, com código, modelo, tipo,
preço, estoque e disponibilidade. A partir dela é possível cadastrar uma armação e
editar seus dados, medidas, estoque, ativação e publicação.
O cadastro também recebe uma foto principal; detalhes de validação e armazenamento
estão em [`frame-images.md`](frame-images.md).

O painel é protegido por autenticação e permite apenas usuários com a função
administrativa. A configuração do acesso está descrita em
[`authentication.md`](authentication.md).
