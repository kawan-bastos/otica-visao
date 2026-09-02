# Casos de uso do catálogo

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

O painel é protegido por autenticação e permite apenas usuários com a função
administrativa. A configuração dos dois acessos está descrita em
[`authentication.md`](authentication.md).
