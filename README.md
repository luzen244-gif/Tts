# LzKeyMap

Mapeador de teclado e mouse para Android — C# (.NET MAUI, `net8.0-android`) + um leitor nativo em C++.
Esta é a versão **completa (Fase 1 + Fase 2 num projeto só)**.

> **Origem do código:** a Fase 1 original não estava disponível quando a Fase 2 foi feita. Esta Fase 1 foi **reescrita do zero
> a partir do README original** (mesmas pastas, mesmos componentes, mesmas regras de honestidade) — não é o código original.

## Como compilar e rodar
1. .NET SDK 8 + workload: `dotnet workload install maui-android` (ou Visual Studio 2022 com *.NET Multi-platform App UI*).
2. `dotnet build -t:Run -f net8.0-android` (aparelho Android 8.0+ / API 26+ com depuração USB).
3. Ative no aparelho: **Sobreposição** (Permissões) e o serviço de **Acessibilidade "LzKeyMap"**.
4. Mouse (opcional, exige **Root**): compile `native/` com o NDK (veja `native/README.md`) e troque `Resources/Raw/evdev_reader`.
5. Shizuku (opcional): `.aar` em `libs/` + descomentar o `ItemGroup` do `LzKeyMap.csproj` e o provider no manifest.

## O que foi VERIFICADO e o que NÃO foi
| Verificado aqui | Não verificado (precisa do seu build/aparelho) |
|---|---|
| `evdev_reader.cpp` compila com g++ sem avisos e o parser foi testado com eventos sintéticos (soma de `REL_X/Y` por `SYN_REPORT`, botões, auto-repeat ignorado, `ERR`+exit 2 sem mouse) | **Todo o C#**: não havia .NET SDK nem SDK do Android neste ambiente — nada foi compilado nem executado |
| XAMLs bem formados, `x:Class` com code-behind, namespaces e chaves/parênteses balanceados nos 48 `.cs` | Qualquer comportamento em aparelho real (gestos, overlay, foreground service, root, mouse) |

Espere corrigir alguns erros de compilação de binding do Android no primeiro build (nomes de membros mudam entre versões do workload).

## O que cada parte faz (e seus limites reais)
**Motor** (`Core/Engine/MappingEngine.cs`, sem dependência de Android)
- Analógico (4 teclas, 8 direções), Tiro (segura; o mouse arrasta o dedo = mira, limitado ao raio), Toque, Pressionar,
  Vista (mouse → arraste que recomeça no centro ao chegar na borda), Macro (auto-toque enquanto a tecla estiver pressionada).
- Auto-repeat de tecla não duplica toques; soltar sempre libera; Stop/Reload soltam tudo. Chamadas ao executor saem de uma thread própria.
- A tecla do componente **Vista** alterna "mira" ↔ "seta" (com a seta ligada o mouse não move a Vista). **O desenho da seta ainda não existe.**
- `ViewMotionProcessor` aplica sensibilidade X/Y, inversão, zona morta, suavização e velocidade do mouse. O ritmo da Vista é
  `min(frequência escolhida, limite do método de execução)` — a tela de Vista mostra a diferença, nunca a esconde.

**Métodos de execução** (`Core/Execution`)
| Método | Toque | Limites |
|---|---|---|
| Acessibilidade | `dispatchGesture` com gestos contínuos; multitoque real (um gesto com todos os ponteiros ativos) | ~60 Hz; só teclado (não vê o mouse) |
| Root | `su` + `input` | ~6 Hz; **um** toque segurado por vez; verificação roda `su` (pode pedir permissão) |
| Shizuku | reflexão em `rikka.shizuku.Shizuku.newProcess` + `input` | idem Root; exige as `.aar`; sem elas = `Unsupported` |
| ADB | cliente ADB próprio (RSA, shell persistente) em `127.0.0.1:5555` | idem Root; **não implementa o pareamento do Depuração Sem Fio (Android 11+)** — só `adb tcpip 5555` feito antes por um PC |

Só vira `Available` depois de uma sonda real do `input`. Com 125–2000 Hz selecionável, Root/Shizuku/ADB entregam ~6 Hz de verdade.
A captura do **mouse** é independente do método de toque e exige Root (`evdev_reader` + `EVIOCGRAB`).

**Telas:** Home (apps escolhidos, INICIAR/PARAR, motor, permissões, dispositivo), Escolher aplicativo, Gerenciador de HUDs
(criar/editar/renomear/duplicar/excluir/importar/exportar/voltar backup/usar neste jogo), Configurações: Vista, Dispositivos
(com TESTAR DISPOSITIVO) e Permissões (com VERIFICAR AGORA). **Overlay:** barra Sair/Adicionar/Salvar/Config + editor
(arrastar, redimensionar, opacidade, tecla, duplicar, excluir pelo "x", bloquear) + diálogo "Configurações: Vista".
Toda gravação de HUD passa por `ProfileStore`; coordenadas sempre relativas (0..1).

**Armazenamento:** `ProfileStore` grava em JSON atômico (`formatVersion` 1); Excluir sempre cria backup antes; Importar valida
(arquivo inválido / versão incompatível) e **nunca sobrescreve** um HUD existente.

## Estrutura
```
Core/Models      HudProfile, HudComponent, ViewSettings, InputCodes
Core/Engine      MappingEngine, ViewMotionProcessor
Core/Execution   IInputExecutor, ExecutionManager, Accessibility/Root/Shizuku/Adb, Adb/AdbProtocol
Core/Permissions PermissionManager      Core/Devices DeviceManager, RawInputService, InputEventHub
Core/Storage     ProfileStore, ChosenAppsStore
Services         LzKeyMapAccessibilityService, MappingForegroundService, FloatingToolbarService
Platforms/Android  Overlay (editor nativo), Impl (catálogo de apps, SAF, launchers, EngineController), Resources, Manifest
ViewModels, Views, Converters, Resources/Styles/LzTheme.xaml
native/jni       evdev_reader.cpp + Android.mk/Application.mk
```

## Pendências conhecidas
- Desenhar/alternar a **seta** do mouse no jogo.
- Pareamento do Depuração Sem Fio (ADB, Android 11+) e multitoque em Root/Shizuku/ADB.
- Binário `evdev_reader` real (hoje é placeholder de texto, e o app diz isso).
- Editar as 4 teclas do Analógico e os passos de Macro no editor (hoje: atribuir por JSON/importação; Macro = auto-toque).
- Medir de verdade os Hz sustentáveis de cada método (os números acima são estimativas).
