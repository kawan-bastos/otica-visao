# Preparação para produção

A V1 pode ser publicada como um único contêiner ASP.NET Core, acompanhado de um
PostgreSQL e um volume persistente para as fotos. O `Dockerfile` da raiz gera a
imagem de produção e respeita a variável `PORT` fornecida pela hospedagem.

## Configuração recomendada no Railway

Crie um serviço a partir deste repositório e um serviço PostgreSQL no mesmo
projeto. No serviço web, configure:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
AllowedHosts=SEU_DOMINIO;SEU_SUBDOMINIO_RAILWAY
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_PRIVATE_URL}}
Database__ApplyMigrations=true
FrameImageStorage__Path=/data/frame-images
AdminAccounts__Accounts__0__DisplayName=Carlos
AdminAccounts__Accounts__0__Email=EMAIL_ADMINISTRATIVO
AdminAccounts__Accounts__0__Password=SENHA_EXCLUSIVA_DE_PRODUCAO_COM_12_OU_MAIS_CARACTERES
```

O nome `Postgres` na referência da conexão deve corresponder ao nome dado ao
serviço de banco. A senha administrativa de produção deve ser diferente da senha
usada no computador e nunca deve ser adicionada ao repositório.

Anexe um volume ao serviço web com ponto de montagem `/data`. Sem esse volume,
as fotos enviadas pelo painel desaparecem quando o serviço reinicia ou recebe
uma nova versão. Configure `/health` como caminho de health check; ele só retorna
sucesso quando a aplicação consegue acessar o banco.

O encaminhamento de cabeçalhos é habilitado porque o HTTPS termina no proxy da
hospedagem. Essa opção deve ser usada apenas atrás do proxy gerenciado, e não ao
expor o contêiner diretamente à internet.

## Banco e backups

Com `Database__ApplyMigrations=true`, as migrations pendentes são aplicadas antes
de o site aceitar tráfego. Para esta V1 haverá somente uma instância do site.
Antes de armazenar dados de clientes ou pedidos, contrate ou configure backups
recorrentes do PostgreSQL e teste uma restauração.

## Itens que dependem do responsável

- criar a conta da hospedagem e escolher o plano;
- cadastrar o meio de pagamento;
- definir uma senha administrativa exclusiva de produção;
- revisar os limites de gastos;
- comprar ou apontar o domínio quando a versão temporária estiver aprovada.
