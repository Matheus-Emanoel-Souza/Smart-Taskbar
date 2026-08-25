# Smart Taskbar

Barra de tarefas inteligente para Windows: agrupa janelas abertas por **contexto de uso**
(Trabalho, Desenvolvimento, Internet...), não apenas por aplicativo. Duas janelas do mesmo
Chrome — uma em `localhost:3000`, outra no YouTube — podem viver em contextos diferentes.

Portátil (ZIP → extrai → executa), sem instalador, sem admin, self-contained.

> Status: **MVP V0.1** em desenvolvimento. Ver [Limitações atuais](#limitações-atuais) e [Roadmap](#roadmap).

## O problema

A barra de tarefas nativa do Windows agrupa por processo. Quem trabalha com dezenas de
janelas abertas — Chrome com 15 abas de propósitos diferentes, três instâncias do VS Code,
terminais, Slack — perde tempo procurando "qual Chrome era a do projeto X". O Smart Taskbar
deixa o usuário definir contextos e regras que classificam cada janela individualmente,
por processo *e* por título.

## Arquitetura

```
SmartTaskbar
│
├── SmartTaskbar.Core            Domínio puro (sem Windows/WPF): modelos, regras, contextos
│   ├── Models/                  WindowInfo, Context, Rule, WindowAssignment
│   ├── Rules/                   RuleEngine, RuleManager
│   ├── Contexts/                ContextManager, WindowAssignmentManager
│   ├── Classification/          WindowClassifier (combina tudo acima)
│   └── Persistence/              Interfaces de repositório (implementadas em Infrastructure)
│
├── SmartTaskbar.Windows         Integração Win32 (P/Invoke)
│   ├── Win32/                   NativeMethods, constantes
│   ├── WindowManager/            Enumeração, filtro, ativação, ícones
│   ├── ProcessManager/            Resolução de processo/executável por PID
│   └── WindowEvents/             WindowEventWatcher (SetWinEventHook, sem polling)
│
├── SmartTaskbar.Infrastructure   Persistência, configurações, logging
│   ├── Persistence/               Repositórios JSON
│   ├── Settings/                  PortablePathProvider, SettingsService
│   └── Logging/                   Serilog
│
├── SmartTaskbar.App              WPF (MVVM), composição via DI
│   ├── Views/                     BarWindow, ContextPanelWindow, ContextEditWindow
│   ├── ViewModels/                 CommunityToolkit.Mvvm
│   └── Services/                   TaskbarOrchestrator (liga eventos → classificação → UI)
│
└── SmartTaskbar.Tests            xUnit — Core e Infrastructure (sem dependência de Windows real)
```

Dependência flui numa direção só: `App → Windows/Infrastructure → Core`. O `Core` não conhece
Win32 nem WPF — é testável sem uma máquina Windows "de verdade" rodando janelas reais.

### Decisões técnicas

- **JSON em vez de SQLite** para persistência. O volume de dados (contextos, regras,
  atribuições) é pequeno e a leitura/escrita é rara (interação do usuário, não hot path).
  SQLite exigiria a DLL nativa `e_sqlite3.dll` embarcada no publish single-file, complicando
  o empacotamento portátil sem ganho real nesta escala.
- **Eventos nativos (`SetWinEventHook`) em vez de polling** para detectar mudança de janela.
  Zero consumo de CPU parado; reage no instante em que o Windows notifica. Roda numa thread
  dedicada com seu próprio loop de mensagens, desacoplada do Dispatcher do WPF.
- **Debounce de 150ms** no `TaskbarOrchestrator`: eventos chegam em rajada (várias janelas
  mudando juntas); reenumerar a cada evento individual seria desperdício.
- **Ícone via `System.Drawing.Icon.ExtractAssociatedIcon`**, cacheado por caminho de
  executável — extração é I/O relativamente caro e o mesmo `.exe` se repete entre janelas.
- **`LibraryImport` (P/Invoke via source generator) por padrão**, caindo para `DllImport`
  clássico só onde o gerador não suporta o marshalling (callbacks com delegate, `StringBuilder`
  — ver `SYSLIB1051`).
- **Atribuição manual de janela → contexto por chave estável** (`executável + título`, não o
  HWND) — o handle nativo some quando a janela fecha; a chave sobrevive a reaberturas.

## Tecnologias

C# 13 (LangVersion `latest`) · .NET 9 (`net9.0-windows`, ver [nota sobre .NET 10](#nota-net-10))
· WPF · Win32 API (P/Invoke) · CommunityToolkit.Mvvm · Microsoft.Extensions.DependencyInjection
· Serilog · System.Text.Json · xUnit

### Nota .NET 10

O projeto foi especificado para **.NET 10 LTS / C# 14**, mas a máquina de desenvolvimento só
tinha o SDK do .NET 9 disponível no momento da implementação. O código está pronto para migrar:
basta trocar `net9.0`/`net9.0-windows` para `net10.0`/`net10.0-windows` nos `.csproj` assim que
o SDK 10 estiver instalado — nenhuma API específica de versão foi usada de propósito.

## Como compilar

Pré-requisitos: [.NET SDK 9](https://dotnet.microsoft.com/download) (ou 10, ver nota acima),
Windows 10/11.

```powershell
dotnet build
```

## Como gerar a versão Portable

```powershell
dotnet publish src/SmartTaskbar.App/SmartTaskbar.App.csproj `
  -c Release -r win-x64 --self-contained true `
  -o publish/win-x64
```

Gera `publish/win-x64/SmartTaskbar.exe` — um único executável (~68 MB, runtime .NET embutido),
sem instalador, sem exigir .NET no computador de destino, sem admin. `.pdb` ao lado são símbolos
de depuração opcionais; apagar não afeta a execução.

Para distribuir: zipar a pasta `publish/win-x64` inteira (ou só o `.exe`, que já é autocontido).

## Como executar

Extrair o ZIP e rodar `SmartTaskbar.exe`. Não há instalação. Configurações e dados ficam em
`Data/` ao lado do executável (ou em `%LOCALAPPDATA%\SmartTaskbar` se essa pasta não for
gravável — comum em ambientes corporativos restritos).

## Como testar

### Automatizado (Core e Infrastructure, sem depender de janelas Windows reais)

```powershell
dotnet test
```

Cobre motor de regras, gerenciamento de contextos, classificação de janela (incluindo o caso
central: duas janelas do mesmo processo em contextos diferentes) e persistência JSON.

### Manual (requer interface e Win32 real — **não é automatizável com segurança**)

> Não execute o app automaticamente durante uma sessão de IA — rode você mesmo e observe.

1. Rode `dotnet run --project src/SmartTaskbar.App` (ou o `.exe` publicado).
2. A barra flutuante deve aparecer perto da parte inferior da tela, com o contexto
   "Sem contexto" mostrando a contagem de janelas abertas.
3. Abra algumas janelas (navegador, editor, terminal) e confirme que a contagem no
   "Sem contexto" atualiza sozinha, sem precisar clicar em nada.
4. Clique em "+" na barra, crie um contexto (ex.: "🧑‍💻 Desenvolvimento").
5. Clique em "Sem contexto" para abrir o painel, clique com o botão direito numa janela
   e escolha "Mover para" → o contexto criado. A janela deve sumir do painel de
   "Sem contexto" e a contagem do novo contexto deve subir.
6. Clique na janela dentro do painel do novo contexto → deve restaurar/focar a janela real.
7. Feche a janela real (Alt+F4 nela) → confirme que ela some do painel/contagem sem precisar
   reabrir o app.
8. Arraste a barra para outra posição, feche e reabra o app → a posição deve persistir.
9. Clique com o botão direito num contexto na barra → teste "Editar...", "Mover para cima/baixo"
   e "Excluir" (exceto em "Sem contexto", que não pode ser excluído).

## Consumo aproximado observado

Ainda não medido formalmente nesta máquina (sem SDK .NET 10 e sem sessão gráfica interativa
disponível durante a implementação). A arquitetura foi desenhada para permitir medir:

- **RAM/CPU em idle**: esperado baixo — sem polling, sem timers agressivos; o único trabalho
  em repouso é a thread do `WindowEventWatcher` bloqueada em `GetMessage` (custo ~zero).
- **Tempo de detecção de mudança de janela**: limitado pelo debounce de 150ms + tempo de
  `EnumWindows` (tipicamente poucos milissegundos com dezenas de janelas).
- **Tempo de inicialização**: beneficiado por `PublishReadyToRun` (JIT antecipado).

Recomendação para o próximo passo: medir com Task Manager / `dotnet-counters` numa sessão real
e documentar números concretos aqui.

## Limitações atuais

- Sem ícone de bandeja (system tray) — fechar a barra encerra o app. Fácil de adicionar
  (`NotifyIcon` via Windows Forms interop ou biblioteca WPF), fora do escopo do V0.1.
- "Reorganizar contextos" é via menu (mover para cima/baixo), não drag-and-drop —
  drag-and-drop está no roadmap V0.2 do próprio produto.
  Escolhido para MVP porque cumpre "reorganizar contextos" com muito menos superfície de bugs
  (posicionamento de drop, ordenação de rejeitados) sem custo de usabilidade grande — a barra
  costuma ter poucos contextos.
- Regras cobrem só uma condição simples (`campo operador valor`), sem composição AND/OR —
  isso é V0.3 no roadmap.
- Ícone de janelas UWP/virtualizadas pode não ser resolvido (caminho de executável nem sempre
  é o pacote real) — o app segue sem ícone nesses casos em vez de falhar.
- Testado apenas por compilação e testes automatizados nesta sessão; validação manual da UI
  real (item anterior) ainda pendente — ver seção "Como testar → Manual".

## Roadmap

- **V0.2**: drag-and-drop de janelas entre contextos, atalhos de teclado, customização visual,
  fixação da barra nas bordas, inicialização automática opcional.
- **V0.3**: identificação de URLs, reconhecimento de arquivos abertos, regras compostas
  (múltiplas condições), perfis.
- **V0.4**: aprendizado local de padrões de uso (sugerir contextos a partir de janelas
  usadas juntas com frequência), sem IA externa.
- **V1**: avaliar integração mais profunda com a barra de tarefas nativa, só depois do
  conceito validado com uso real.
