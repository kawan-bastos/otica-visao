# Backup e recuperação

O backup reúne o banco PostgreSQL e as fotos das armações em um pacote com data e hora. Por padrão, os pacotes ficam em `Documentos\OticaVisaoBackups`, fora do repositório Git.

## Criar um backup local

Com o PostgreSQL instalado e a conexão configurada nos User Secrets, execute na raiz do projeto:

```powershell
.\scripts\Backup-OticaVisao.ps1
```

O pacote contém `database.dump`, `frame-images.zip` e `manifest.json`. Ele inclui dados pessoais e hashes de senha, portanto deve ser guardado em local privado e nunca enviado ao GitHub.

## Restaurar

Feche o site antes da restauração. Informe exatamente o diretório criado pelo backup:

```powershell
.\scripts\Restore-OticaVisao.ps1 -BackupDirectory "C:\caminho\do\backup" -ConfirmRestore
```

A restauração substitui o conteúdo do banco e a pasta de fotos. O parâmetro obrigatório `-ConfirmRestore` reduz o risco de execução acidental.

## Produção

O provedor deverá manter backups automáticos do PostgreSQL. As fotos também precisam estar em volume persistente e ter cópia separada. Antes da publicação, será realizado um ensaio de restauração em um banco isolado para confirmar que os arquivos podem ser recuperados.
