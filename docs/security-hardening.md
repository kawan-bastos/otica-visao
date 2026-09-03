# Segurança antes da produção

## Proteções aplicadas

- formulários de login e cadastro permitem no máximo 10 envios por endereço IP a cada 5 minutos;
- o bloqueio individual do ASP.NET Identity continua ativo após 5 senhas incorretas;
- páginas administrativas e de conta recebem `no-store` e não devem ser mantidas no cache do navegador;
- cookies de autenticação são HTTP-only, usam HTTPS obrigatório em produção e expiram;
- formulários Razor utilizam proteção antifalsificação automaticamente;
- uploads continuam limitados por tamanho, extensão e conteúdo;
- erros inesperados exibem uma mensagem neutra e um código de atendimento, sem detalhes internos;
- cabeçalhos impedem interpretação indevida de conteúdo e abertura do site em frames externos.

## Validação de inicialização

Em ambiente `Production`, o sistema se recusa a iniciar se detectar conexão local, migrations desativadas, armazenamento de fotos não persistente, `AllowedHosts` aberto, ausência de administrador ou senha administrativa com menos de 12 caracteres.

Logs técnicos e auditoria não devem registrar senhas, documentos completos, endereços ou conteúdo de formulários. Detalhes de exceções permanecem disponíveis somente no ambiente local de desenvolvimento.
