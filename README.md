# Meu Save Editor

Editor pessoal de saves para Euro Truck Simulator 2 e American Truck Simulator. Esta é uma derivação do **TS SE Tool**, com a interface e os recursos do projeto original, nome próprio e gravação de saves reforçada.

O projeto original foi criado por LIPtoH e publicado sob a licença Apache 2.0. Os avisos de autoria e a licença original foram mantidos. Consulte [LICENSE](./LICENSE) e [ANALISE_TECNICA.md](./ANALISE_TECNICA.md).

## Recursos

- Modo ETS Bus para ajustar nível e habilidades do perfil, estatísticas da carreira e ônibus do save.
- Selecionar ETS2 ou ATS, perfis e saves locais; adicionar pastas personalizadas.
- Editar nome, nível, experiência, habilidades, dinheiro, cores e outras opções do perfil.
- Renomear e clonar perfis; importar e exportar configurações de perfil.
- Administrar cidades visitadas, garagens, caminhões e motoristas da empresa.
- Contratar, dispensar e editar habilidades de motoristas de IA.
- Reparar e abastecer caminhões; editar veículos, pintura, acessórios e placa.
- Reparar reboques e copiar ou compartilhar configurações de caminhão e reboque.
- Criar e editar trabalhos do Mercado de Frete, com filtros de origem, destino, carga e reboque.
- Fazer ajustes básicos no Mercado de Cargas.
- Copiar e compartilhar posição do caminhão e rotas de GPS; agrupar posições para controle de comboio.
- Usar as traduções disponíveis, unidades, moedas e opções do programa.

## Segurança ao editar

Use um save **local**, feche o jogo e mantenha o backup criado pelo editor. Não selecione autosave para editar. A rotina desta derivação prepara o conteúdo antes de gravar, preserva dados SII desconhecidos e troca cada arquivo somente depois que o arquivo temporário está pronto. O editor também cria `game_backup.sii`, `info_backup.sii` e `profile_backup.sii`.

Steam Cloud não pode ser editado diretamente pelo TS SE Tool: desative “Usar Steam Cloud” no perfil e trabalhe em um save local. O formato dos saves pode mudar com atualizações do jogo; confira os limites e o procedimento em [ANALISE_TECNICA.md](./ANALISE_TECNICA.md).

## Gerar e abrir o programa

O diretório `dist` contém o executável e os recursos necessários (`img`, `lang`, `libs` e `updater`), então essa pasta pode ser usada separadamente do TS.SE.Tool original. Para recompilar, execute no Windows:

```powershell
.\build_and_run.ps1
```

O script restaura os pacotes NuGet, usa o compilador Roslyn, compila a solução e atualiza `dist\MeuSaveEditor.exe`. Requer Windows, MSBuild e referências do .NET Framework 4.7.2; baixa o NuGet CLI oficial caso não esteja instalado. Ele usa os recursos de `dist` como fonte quando a pasta original não estiver ao lado do projeto.

Para criar o instalador e o pacote portátil da versão compilada, execute `powershell -ExecutionPolicy Bypass -File .\installer\Build-Installer.ps1`. O instalador instala por usuário, cria atalhos e registra a desinstalação em Configurações do Windows. O tutorial está em [TUTORIAL_INSTALACAO.md](./TUTORIAL_INSTALACAO.md).

As verificações automáticas de atualização foram desativadas porque não há servidor de atualização para esta cópia pessoal. A consulta manual mostra a versão upstream, mas não instala outro executável.

## Licença e créditos

O código-base é derivado do TS SE Tool sob Apache License 2.0. Bibliotecas de terceiros têm suas próprias licenças; o pacote de distribuição inclui o arquivo de licenças fornecido junto com a versão local. Este programa não é afiliado à SCS Software.
