# Ótica Visão de Piabetá

Sistema web da Ótica Visão de Piabetá que reúne a vitrine pública de armações, a área do cliente e o painel de operação da loja. O cliente pode conhecer os modelos, salvar favoritos e reservar uma armação para experimentar; a equipe administra catálogo, estoque, atendimentos, vendas, pedidos de lentes e resultados financeiros.

O site foi pensado para computador, tablet e celular. A escolha, experimentação e conclusão da compra continuam ligadas ao atendimento da loja: **não há checkout nem pagamento online**.

## Funcionalidades

### Site público e área do cliente

- Home com armações em destaque, páginas institucionais, contato, localização, privacidade e termos.
- Catálogo com busca por código, marca, modelo ou cor e filtros por tipo, formato e público. Somente armações ativas, publicadas e com estoque aparecem para o visitante.
- Página da armação com foto, preço, medidas disponíveis e contato pelo WhatsApp.
- Cadastro e login de clientes vinculados à ficha comercial, com consulta de endereço por CEP e preenchimento manual como alternativa.
- Minha Conta para editar dados, trocar senha, consultar favoritos, reservas e compras/pedidos, além de excluir o acesso à conta.
- Recuperação de senha por código enviado ao e-mail cadastrado; requer SMTP configurado.
- Favoritos e reservas de armações por 24 horas, com acompanhamento na conta. A reserva não equivale a uma compra.

### Painel administrativo

- Catálogo e estoque: cadastro e edição de armações, preço, medidas, publicação, foto principal e ajuste de quantidade. A foto pode ser recortada com prévia do enquadramento no catálogo.
- Clientes: consulta e edição das fichas comerciais, associação com contas do site e proteção do histórico de vendas.
- Vendas: rascunhos, itens de armação e lentes, receitas, laboratórios, formas de pagamento, conclusão, cancelamento e estorno auditado. A movimentação de itens atualiza o estoque.
- Reservas: acompanhamento das solicitações dos clientes.
- Pedidos de lentes: etapas de produção, previsão de entrega, observações, documentos e histórico de mudanças.
- Relatórios: faturamento, vendas, ticket médio, produtos, estoque baixo e custos por categoria, incluindo salários, com orçamento mensal.
- Equipe: contas administrativas com permissões por painel e registro de ações em auditoria.

## Tecnologias e estrutura

- .NET 10, ASP.NET Core Razor Pages e ASP.NET Core Identity.
- PostgreSQL com Entity Framework Core e migrations.
- HTML, CSS e JavaScript na interface responsiva.
- xUnit para testes automatizados; Dockerfile para publicação em contêiner.

```text
src/
  OticaVisao.Web/             # páginas, endpoints e configuração
  OticaVisao.Application/     # casos de uso e contratos
  OticaVisao.Domain/          # entidades e regras de negócio
  OticaVisao.Infrastructure/  # banco, autenticação e serviços externos
tests/
  OticaVisao.Tests/
scripts/                       # backup e restauração
docs/                          # documentação técnica e operacional
```

O projeto é um monólito modular: o site público e o painel `/Admin` são executados na mesma aplicação. Veja [a arquitetura](docs/architecture.md).

## Executar localmente

Pré-requisitos: SDK .NET indicado em [`global.json`](global.json) e PostgreSQL acessível. Configure a conexão completa **fora do Git**, por exemplo com variável de ambiente no PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=otica_visao;Username=postgres;Password=SUA_SENHA"
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/OticaVisao.Infrastructure --startup-project src/OticaVisao.Web
dotnet run --project src/OticaVisao.Web
```

Para criar um administrador local, configure nome, e-mail e senha em *User Secrets* antes de iniciar a aplicação. O procedimento está em [autenticação](docs/authentication.md). Para testar a recuperação de senha, configure também o serviço SMTP; os nomes das configurações estão em [`.env.example`](.env.example). Não coloque credenciais reais no README, no `appsettings.json` ou em commits.

Execute os testes com:

```powershell
dotnet test
```

## Pontos importantes antes de publicar

- O catálogo contém registros de demonstração; substitua-os e confira preços, fotos e estoque antes de divulgar o site. Consulte [o catálogo administrativo](docs/catalog-application.md).
- São necessários PostgreSQL, armazenamento persistente para fotos, documentos de laboratório e chaves de Data Protection, além de SMTP para recuperação de senha. Um contêiner sem volume persistente pode perder esses arquivos ao reiniciar.
- Em produção, use HTTPS, domínio permitido explícito, segredos no provedor e backups recorrentes do banco e dos arquivos. A aplicação valida configurações críticas ao iniciar. Veja [produção](docs/production.md), [segurança](docs/security.md) e [backup e recuperação](docs/backup-and-recovery.md).
- Dados pessoais, receitas e documentos exigem acesso restrito e cuidados de privacidade. Revise os textos de Política de Privacidade e Termos de Uso antes da publicação definitiva.
- `/health` verifica a disponibilidade da aplicação e do banco; as migrations podem ser aplicadas na inicialização quando `Database__ApplyMigrations=true`.

## Documentação por assunto

- [Persistência e migrations](docs/persistence.md)
- [Fotos das armações](docs/frame-images.md)
- [Catálogo público](docs/public-catalog.md)
- [Clientes e contas](docs/customers.md)
- [Vendas](docs/sales.md) · [Estornos](docs/sale-reversals.md)
- [Pedidos de lentes](docs/laboratory-orders.md)
- [Relatórios](docs/reports.md)
- [Auditoria](docs/auditing.md)
- [Validação da V1](docs/v1-validation.md)
