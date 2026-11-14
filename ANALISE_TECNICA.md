# Análise técnica do TS SE Tool 0.3.13.0

## Escopo observado

A pasta fornecida é um pacote compilado: contém `TS SE Tool.exe`, bibliotecas, imagens, idiomas, atualizador e um guia PDF, mas não contém os projetos C# originais. O changelog local identifica a versão `0.3.13.0` e diz que ela adicionou blocos de save das versões 1.60, 1.61 e 1.62 e corrigiu o carregamento de avatares locais e online.

O código-fonte público está publicado em [ti0cl/-TS-SE-Tool-sequel](https://github.com/ti0cl/-TS-SE-Tool-sequel), sob Apache 2.0. O snapshot da tag `v0.3.13.0` contém marcadores de conflito Git em vários arquivos C# e no projeto, então não é uma base compilável confiável como está. Além disso, houve um relato público de save v97 zerado durante a gravação e de atributos de save descartados. A causa e a correção estão documentadas no [problema #148](https://github.com/LIPtoH/TS-SE-Tool/issues/148) e na [proposta de correção #147](https://github.com/LIPtoH/TS-SE-Tool/pull/147).

Esta cópia usa a base limpa da proposta #147, que adiciona gravação por arquivo temporário, preservação de atributos desconhecidos e validação de round-trip para saves v97. Mantive as áreas funcionais do editor e a estrutura visual WinForms do TS SE Tool, personalizei o nome e acrescentei substituição atômica do arquivo. A exibição de avatar também existe nesta base; não foi validada com perfil online neste computador.

## Funcionalidades do TS SE Tool

| Área | O que oferece |
|---|---|
| Perfis e saves | Alternar ETS2/ATS, localizar perfis/saves locais, usar diretórios personalizados e abrir a pasta do save. |
| Perfil | Editar nível, XP, habilidades, nome e cores; renomear/clonar perfil; importar/exportar configurações. |
| Empresa | Ajustar saldo, visitar cidades, comprar/melhorar/vender garagens, administrar caminhões e contratar/dispensar motoristas. |
| Motoristas | Editar habilidades de IA e organizar a lista de motoristas. |
| Caminhão | Selecionar e trocar caminhão ativo, reparar, abastecer, editar veículo e placa; copiar/compartilhar pintura e configurações. |
| Reboque | Reparar, selecionar e trocar o reboque ativo; copiar/compartilhar detalhes e pintura. |
| Mercado de Frete | Criar/editar trabalhos, escolher carga, origem/destino, empresa, reboque, distância e urgência; usar filtros e seleção aleatória. |
| Mercado de Cargas | Fazer ajustes básicos na lista de cargas e usar ações de randomizar/restaurar. A própria interface original marcava essa área como incompleta. |
| Ferramentas de comboio | Copiar/colar posição do caminhão e rota GPS; reunir posições de vários saves em um pacote para Convoy Control. |
| Preferências | Idiomas, unidades, moedas, opções de trabalhos e pastas de save. |

## Arquitetura

- Aplicativo desktop C# WinForms para Windows, direcionado ao .NET Framework 4.7.2.
- `FormMain` e os métodos em `Forms/MainTabs` implementam a interface e as abas.
- `MethodsDecodeSave.cs` decodifica saves SII; `CustomClasses/Save/Items` contém os modelos tipados dos blocos `SiiNunit`.
- `MethodsReadWrite.cs` aplica as alterações, cria os backups e grava `profile.sii`, `info.sii` e `game.sii`.
- `OriginalBlockMerge.cs` preserva linhas que o modelo tipado não conhece, evitando descartar campos adicionados por versões recentes do jogo.
- SQL Server Compact armazena cache de dados do jogo; SII Decrypt e SharpZipLib dão suporte à decodificação e ao processamento de arquivos.
- O aplicativo grava configuração, log e log de erros no diretório de execução. O atualizador original é um projeto separado.

## Limites e compatibilidade

- O programa deve trabalhar em saves locais. Desative Steam Cloud para o perfil antes de editar.
- A versão 0.3.13.0 anuncia compatibilidade com ETS2/ATS 1.62. O código desta derivação reconhece savefile v97 e preserva campos não modelados.
- A proposta #147 registra uma validação do pipeline sem perda em 21 saves ATS v97; esse resultado é do autor da proposta, não um teste executado neste computador.
- Não abri saves de jogo do usuário nem verifiquei uma gravação dentro do ETS2/ATS nesta entrega. Faça primeiro uma cópia manual e valide o resultado no jogo.
- O atualizador automático do upstream foi desativado para que um pacote oficial não substitua esta cópia personalizada. O verificador manual só informa releases upstream.

## Licença

O TS SE Tool está sob Apache License 2.0. Os avisos de autoria foram mantidos nos arquivos originais e os arquivos modificados estão marcados como alterações da derivação pessoal. As dependências e os recursos do pacote têm avisos próprios; mantenha as licenças junto com qualquer distribuição.
