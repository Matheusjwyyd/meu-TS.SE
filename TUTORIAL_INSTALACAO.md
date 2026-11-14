# Tutorial de instalação e uso — Meu Save Editor

## Requisitos

- Windows 10 ou 11.
- Microsoft .NET Framework 4.7.2 ou posterior. Se o programa não iniciar, instale pelo [site oficial da Microsoft](https://dotnet.microsoft.com/pt-br/download/dotnet-framework/net472).
- Um save local do Euro Truck Simulator 2 ou do American Truck Simulator.

## Instalar pelo Setup

1. Baixe `MeuSaveEditor-Setup.exe` na seção **Releases** do repositório.
2. Execute o instalador. Ele instala apenas para sua conta do Windows, sem solicitar privilégios de administrador.
3. Mantenha ou escolha a pasta de instalação e selecione se deseja criar um atalho na área de trabalho.
4. Clique em **Instalar**. Ao terminar, abra pelo atalho criado ou pelo menu Iniciar.

O Setup não é assinado digitalmente. Se o Windows mostrar um aviso de editor desconhecido, confirme que o arquivo foi baixado da página de Releases deste repositório antes de prosseguir.

## Usar a versão portátil

1. Baixe `MeuSaveEditor-Portable.zip` na seção **Releases**.
2. Extraia a pasta para um local gravável, como Documentos.
3. Abra `MeuSaveEditor.exe` dentro da pasta extraída. Mantenha os arquivos e pastas ao lado do executável.

## Preparar o save com segurança

1. Feche o ETS 2 ou ATS antes de editar.
2. Use um save local e mantenha uma cópia de segurança. Steam Cloud não pode ser editado diretamente pelo programa; use um perfil/salvamento local.
3. Evite escolher o autosave. A edição deve ser feita em um save manual.
4. Abra o Meu Save Editor, selecione ETS 2 ou ATS no canto direito e escolha o perfil e o salvamento.
5. Se o salvamento estiver criptografado, use **Descriptografar** e aguarde a conclusão.
6. Clique em **Carregar** e espere o programa terminar de decodificar.
7. Faça as alterações desejadas e clique em **Salvar**. Aguarde a mensagem de conclusão antes de fechar o editor.

O editor cria arquivos de backup junto ao salvamento antes de gravar. Ainda assim, mantenha uma cópia própria do save original.

## Modo ETS Bus

1. Abra um perfil do ETS 2 e selecione **ETS Bus** no canto direito.
2. Na aba **Perfil**, ajuste o nível e as habilidades da carreira de ônibus.
3. Na aba **Ônibus**, selecione um ônibus do save para editar peças, condição e combustível.
4. A aba **Estatísticas** mostra as estatísticas da carreira de ônibus.

O modo ETS Bus funciona apenas com perfil e salvamento do ETS 2.

## Desinstalar

No Windows, abra **Configurações → Aplicativos → Aplicativos instalados**, procure **Meu Save Editor** e escolha **Desinstalar**. Também é possível usar o atalho **Desinstalar Meu Save Editor** no menu Iniciar.

A desinstalação remove os arquivos do programa e os atalhos criados. Os backups dos saves, que ficam na pasta do jogo, não são removidos.

## Problemas comuns

- **O programa não abre:** instale ou atualize o [.NET Framework 4.7.2](https://dotnet.microsoft.com/pt-br/download/dotnet-framework/net472) e tente novamente.
- **O save não aparece:** confirme que selecionou o perfil e o jogo corretos, que o save é local e que o jogo está fechado.
- **Erro ao salvar:** feche o jogo, use um save manual não sincronizado pelo Steam Cloud e carregue o arquivo novamente.
- **Precisa restaurar o original:** recupere a cópia de segurança criada antes da gravação.
