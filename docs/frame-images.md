# Fotos das armações

Cada armação pode possuir uma foto principal. No cadastro, a foto é obrigatória;
na edição, enviar uma nova foto substitui a anterior.

## Segurança do upload

- formatos permitidos: JPG, PNG e WebP;
- tamanho máximo por foto: 5 MB;
- extensão e assinatura binária são verificadas no servidor;
- o nome enviado pelo usuário nunca é usado no armazenamento;
- o sistema gera um identificador aleatório para cada arquivo;
- os arquivos ficam em `App_Data/frame-images`, fora do diretório público da aplicação;
- somente o endpoint `/frame-images/{arquivo}` entrega imagens com nome e formato válidos;
- arquivos locais enviados são ignorados pelo Git.

O banco armazena apenas o nome seguro do arquivo. Em desenvolvimento, as imagens
ficam no disco local. Essa implementação segue um contrato de armazenamento próprio,
permitindo trocar o disco por um serviço de objetos na hospedagem sem alterar as
regras do catálogo nem os formulários.

## Antes da produção

O disco local pode ser perdido em hospedagens que usam instâncias descartáveis.
Antes da publicação, será necessário escolher uma hospedagem e configurar um serviço
persistente de arquivos, além de política de backup. Verificação antivírus poderá ser
adicionada conforme o provedor e o risco operacional definidos.
