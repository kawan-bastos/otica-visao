# Persistência do catálogo

O projeto usa Entity Framework Core como ORM e o provedor Npgsql para armazenar
os dados no PostgreSQL. O `ApplicationDbContext` representa a sessão de acesso ao
banco e, nesta primeira etapa, expõe a coleção de armações.

## Esquema inicial

A migration `InitialCatalog` cria a tabela `frames` com:

- identificador interno em UUID;
- código comercial único, compartilhado pelas unidades do mesmo modelo;
- marca, modelo, cor, tipo, formato e público;
- preço e quantidade em estoque;
- medidas opcionais da armação;
- indicadores de produto ativo e publicado.

As migrations são o histórico versionado das mudanças de estrutura do banco.
Gerá-las não altera nenhum banco; a alteração só ocorre quando uma migration é
aplicada explicitamente.

## Configuração local

O arquivo `appsettings.Development.json` contém apenas host, porta, banco e usuário
de desenvolvimento. Senhas não devem ser adicionadas ao repositório. Defina a
conexão completa em uma variável de ambiente antes de executar o site:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=otica_visao;Username=postgres;Password=SUA_SENHA"
```

Com um PostgreSQL local disponível, restaure a ferramenta e aplique as migrations:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/OticaVisao.Infrastructure --startup-project src/OticaVisao.Web
```

Para criar uma migration depois de modificar o modelo:

```powershell
dotnet tool run dotnet-ef migrations add NomeDaAlteracao --project src/OticaVisao.Infrastructure --startup-project src/OticaVisao.Web --output-dir Persistence/Migrations
```
