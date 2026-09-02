# Autenticação do painel

O painel em `/Admin` é protegido pelo ASP.NET Core Identity e exige a função
`Administrator`. Visitantes não autenticados são enviados para
`/Admin/Account/Login`.

## Proteções configuradas

- senhas armazenadas somente como hash pelo Identity;
- senha mínima de 12 caracteres, com maiúscula, minúscula, número e símbolo;
- e-mail único para cada administrador;
- bloqueio por 15 minutos após cinco tentativas incorretas;
- cookie HTTP-only, renovação deslizante e duração máxima de oito horas;
- logout disponível somente por requisição POST;
- bloqueio de URLs externas no redirecionamento após o login.

## Criar os dois acessos no desenvolvimento

As contas iniciais são lidas da configuração segura no primeiro início do site.
Não coloque e-mails ou senhas em `appsettings.json`. No terminal, dentro do projeto,
defina os segredos substituindo os exemplos pelos dados escolhidos:

```powershell
dotnet user-secrets set "AdminAccounts:Accounts:0:DisplayName" "Seu nome" --project src/OticaVisao.Web
dotnet user-secrets set "AdminAccounts:Accounts:0:Email" "seu-email@exemplo.com" --project src/OticaVisao.Web
dotnet user-secrets set "AdminAccounts:Accounts:0:Password" "SUA-SENHA-FORTE" --project src/OticaVisao.Web

dotnet user-secrets set "AdminAccounts:Accounts:1:DisplayName" "Nome do seu pai" --project src/OticaVisao.Web
dotnet user-secrets set "AdminAccounts:Accounts:1:Email" "email-do-seu-pai@exemplo.com" --project src/OticaVisao.Web
dotnet user-secrets set "AdminAccounts:Accounts:1:Password" "OUTRA-SENHA-FORTE" --project src/OticaVisao.Web
```

Depois de aplicar a migration e iniciar o site, as contas ausentes serão criadas.
Executar novamente não duplica usuários. Alterar o segredo depois da criação não
troca automaticamente a senha existente.

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/OticaVisao.Infrastructure --startup-project src/OticaVisao.Web
dotnet run --project src/OticaVisao.Web
```

Em produção, use variáveis de ambiente ou o gerenciador de segredos da hospedagem,
nunca o arquivo de configuração versionado.
