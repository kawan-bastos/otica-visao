# Backup e recuperação

O backup reúne o banco PostgreSQL, as fotos das armações e os documentos de laboratório em um pacote com data e hora. Por padrão, os pacotes ficam em `Documentos\OticaVisaoBackups`, fora do repositório Git.

## Criar um backup local

Com o PostgreSQL instalado e a conexão configurada nos User Secrets, execute na raiz do projeto:

```powershell
.\scripts\Backup-OticaVisao.ps1
```

O pacote contém `database.dump`, `frame-images.zip`, `laboratory-documents.zip` e `manifest.json`. Ele inclui dados pessoais, documentos e hashes de senha, portanto deve ser criptografado, guardado em local privado e nunca enviado ao GitHub.

## Restaurar

Feche o site antes da restauração. Informe exatamente o diretório criado pelo backup:

```powershell
.\scripts\Restore-OticaVisao.ps1 -BackupDirectory "C:\caminho\do\backup" -ConfirmRestore
```

A restauração substitui o conteúdo do banco, a pasta de fotos e os documentos do laboratório. O parâmetro obrigatório `-ConfirmRestore` reduz o risco de execução acidental. Faça primeiro um ensaio em banco e diretórios isolados.

## Produção

O provedor deverá manter backups automáticos do PostgreSQL. Fotos e documentos também precisam estar em volume persistente e ter cópia separada. Antes da publicação, realize um ensaio de restauração em um banco isolado para confirmar que todos os arquivos podem ser recuperados.
