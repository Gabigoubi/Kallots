# Kallots

---

## 📌 Apresentação do Projeto

O **Kallots** é um assistente virtual híbrido de desktop projetado para rodar diretamente em segundo plano no sistema operacional. O projeto nasceu sob a filosofia de desenvolvimento incremental, evoluindo através de escopos pequenos, controlados e focados em arquitetura limpa, escalabilidade e manutenibilidade, evitando complexidade prematura.

A grande força do Kallots está na sua arquitetura modular híbrida: o processamento pesado de áudio (reconhecimento de voz e transcrição) é feito localmente na máquina do usuário para garantir privacidade, enquanto a tomada de decisões e o parse de intenções complexas utilizam modelos de linguagem avançados (LLMs) na nuvem por meio de APIs de altíssima velocidade.

---

## 🛠️ Stack Utilizada

- **Plataforma Core:** .NET 8.0 (Worker Service nativo do Windows)
- **Linguagem:** C# (Abordagem fortemente tipada e orientada a interfaces)
- **Reconhecimento Acústico (Wake Word):** Vosk (Modelo leve otimizado para português)
- **Transcrição de Comandos (STT):** Whisper.net (Execução em C++ não gerenciado via bindings .NET)
- **Cérebro Lógico (LLM):** Groq API (`llama-3.1-8b-instant`) com inferência de ultra-baixa latência
- **Áudio e Captura:** NAudio para manipulação de streams de microfone em tempo real
- **Síntese de Voz (TTS):** Edge-TTS (Integração via Python para geração de áudio neural de alta fidelidade)

---

## 🚀 Funcionalidades Atuais (MVP v1.0 / v1.1)

O escopo atual do MVP está validado, blindado e operacional nas seguintes frentes:

- [x] **Ouvido em Loop Contínuo:** Monitoramento em segundo plano sem travamento da interface principal do terminal.
- [x] **Fuzzy Matching Fonético:** O motor do detector de palavras aceita aproximações fonéticas para contornar falhas comuns de ruído em português (ex: intercepta "oi calotes" ou "motos" e converte para o gatilho correto "Kallots").
- [x] **UX de Latência Zero (Earcon):** Resposta imediata por meio de um feedback sonoro nativo do Windows (Bip por hardware) assim que o nome é reconhecido, dispensando esperas assíncronas de TTS no gatilho.
- [x] **Tranca de Concorrência (Race Condition Lock):** Mecanismo de estado estruturado que impede o disparo de novos gatilhos ou gravações enquanto um comando anterior já está em processamento no pipeline.
- [x] **Executor Estático de Tarefas:** Mapeamento duro (_hardcoded_) de intenções capaz de disparar processos reais do Windows:
- `OPEN_VSCODE` -> Inicializa o Visual Studio Code.
- `OPEN_BROWSER` -> Abre o navegador padrão do sistema em uma URL pré-definida.
- `OPEN_CALCULATOR` -> Dispara a calculadora nativa do sistema operacional.

- [x] **Telemetria de Debug Extensa:** Logs detalhados no terminal que cronometram com precisão de milissegundos o tempo exato gasto em cada fase (Gravação, Transcrição Whisper, Resposta do Groq e Execução).

---

## 📐 Arquitetura Atual e Fluxo do MVP

O Kallots é estruturado sob as diretrizes de Injeção de Dependência nativa do .NET, segregando as capacidades físicas do assistente através de interfaces desacopladas (`IWakeWordDetector`, `ISttProvider`, `ILlmProvider`, `ICommandExecutor`, `ITtsProvider`).

```
[Vosk (Ouvido)] ──> [Fuzzy Match] ──> [Windows Beep (Feedback)]
                                             │
[Whisper (STT)] <── [Gravação 5s] <─── [Tranca de Segurança]
       │
       └──> [Groq API (Cérebro)] ──> [CommandExecutor] ──> [Ação no Windows OS]

```

---

## 📈 Futuro do Projeto & Roadmap

O Kallots está sendo construído progressivamente. A tabela abaixo diferencia as conquistas atuais do planejamento técnico para as próximas versões.

| Versão   | Módulo / Funcionalidade        | Estado       | Descrição                                                                                                                         |
| -------- | ------------------------------ | ------------ | --------------------------------------------------------------------------------------------------------------------------------- |
| **v1.0** | **Arquitetura Base**           | Concluído    | Implementação das interfaces, pipeline de áudio híbrido local/nuvem.                                                              |
| **v1.1** | **Telemetria & UX**            | Concluído    | Injeção de `ILogger`, medições com `Stopwatch` e feedback via `Console.Beep`.                                                     |
| **v1.2** | **Varredura Dinâmica de Apps** | ⏳ Planejado | Substituição do bloco `switch` fixo por um leitor nativo do diretório _Start Menu_ do Windows para abrir qualquer app instalado.  |
| **v2.0** | **Function Calling & Memória** | ⏳ Planejado | Evolução do provedor de LLM para suportar ferramentas (Agentes) e manutenção de histórico de sessão em RAM para conversação real. |
| **v2.1** | **Integração de WebScraping**  | ⏳ Planejado | Capacidade de consultar APIs de busca ou extrair informações em tempo real da internet sob demanda do usuário.                    |

---

## 🎯 Visão de Longo Prazo

O objetivo final do Kallots é tornar-se um assistente de desktop autônomo e de tempo integral (_full-time agent_). Em seu estado maduro, o agente não apenas abrirá softwares, mas interagirá de ponta a ponta com o ecossistema do usuário através de fluxos automatizados complexos e seguros.

> **Exemplo de Caso de Uso Futuro:**
> _Usuário:_ "Kallots, abra o WhatsApp, encontre a conversa do João, escreva que a reunião foi adiada para as 15h e aguarde minha confirmação para enviar."
> _Kallots:_ Executa a navegação visual ou via API, preenche as informações em sandbox e aguarda o trigger físico do usuário para conclusão.

---

## 💻 Como Executar o Projeto

### Pré-requisitos Técnicos

1. **SDK do .NET 8.0** instalado na máquina.
2. **Python 3.x** configurado nas variáveis de ambiente (`PATH`) com a biblioteca `edge-tts` instalada mundialmente:

```powershell
pip install edge-tts

```

3. Modelos de IA locais inseridos manualmente na estrutura de arquivos do projeto:

- O modelo do Vosk (versão pequena de 30MB) deve ser extraído diretamente em: `src/Kallots.Worker/Models/Vosk/` (certifique-se de que os arquivos como `final.mdl` e `am` não fiquem presos em subpastas).
- O modelo do Whisper (`ggml-base.bin`) deve ser inserido em: `src/Kallots.Worker/Models/Whisper/`.

### Configuração de Segurança (User Secrets)

Para evitar o vazamento acidental de chaves de API no GitHub, o projeto utiliza o gerenciador de segredos do .NET. Navegue até a pasta do Worker e configure sua credencial do Groq:

```powershell
cd src/Kallots.Worker
dotnet user-secrets init
dotnet user-secrets set "GroqApiKey" "SUA_CHAVE_COMPLETA_DO_GROQ_AQUI"

```

### Inicialização em Ambiente de Desenvolvimento

Por motivos de segurança corporativa do ecossistema .NET, os segredos de usuário são bloqueados em modo de Produção. Certifique-se de forçar a flag de ambiente ao inicializar o assistente via console:

```powershell
dotnet clean
dotnet build
dotnet run --environment Development

```

---

## 📄 Considerações Finais

O Kallots prova que é totalmente viável construir soluções de assistentes virtuais de alta fidelidade e tempo de resposta baixo utilizando hardware convencional, desde que as fronteiras entre o processamento local (Edge) e o processamento em nuvem (Cloud) sejam bem desenhadas arquiteturalmente.

---
