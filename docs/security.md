# Segurança e operação

## Limites de requisições

- login de cliente e administrador: 10 tentativas por IP a cada 5 minutos;
- cadastro: 5 tentativas por IP a cada 15 minutos;
- catálogo e pesquisa pública: 60 requisições por IP por minuto;
- imagens públicas: 120 requisições por IP por minuto;
- health check: 30 requisições por IP por minuto;
- alterações autenticadas: 60 requisições por usuário por minuto.

Respostas bloqueadas usam HTTP 429 e informam `Retry-After` quando o limitador disponibiliza o tempo. Atrás de proxy, habilite os forwarded headers somente no provedor gerenciado para que o endereço usado pelo limitador represente o visitante.

## Serviços externos

- ViaCEP: preenchimento opcional do endereço, com timeout de 6 segundos e preenchimento manual como fallback;
- Google Maps: mapa e rota; o endereço em texto continua disponível em caso de falha;
- WhatsApp e Instagram: links externos, sem dependência do servidor.

Não há gateway de pagamento ou API externa indispensável ao funcionamento do servidor.

## Publicação

- mantenha `ASPNETCORE_ENVIRONMENT=Production` e HTTPS no proxy;
- use domínio explícito em `AllowedHosts`;
- preserve fotos, documentos e chaves de Data Protection no volume `/data`;
- mantenha conexão, senha administrativa e demais segredos somente nas variáveis protegidas do provedor;
- execute backup antes de migrations e teste a restauração em ambiente isolado;
- revise Política de Privacidade e Termos de Uso com orientação jurídica antes da publicação definitiva.
