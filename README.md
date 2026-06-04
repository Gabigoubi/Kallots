<div align="center">
  
  <h1>🤖 Kallots</h1>
  <h3>Assistente de Desktop Híbrido | Automação, IA & Arquitetura de Sistemas</h3>
  
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&weight=600&size=20&duration=4000&pause=1000&color=00ADD8&center=true&vCenter=true&width=600&lines=Assistente+de+Desktop+H%C3%ADbrido;Constru%C3%ADdo+em+.NET+8;Intelig%C3%AAncia+Artificial+Modular;Execu%C3%A7%C3%A3o+de+Baixa+Lat%C3%AAncia" alt="Typing SVG" />

  <p align="center">
    <img src="https://img.shields.io/badge/Status-Em_Desenvolvimento-00ADD8?style=for-the-badge" alt="Status" />
    <img src="https://img.shields.io/badge/Vers%C3%A3o-MVP_v1.0-6DB33F?style=for-the-badge" alt="Versão" />
    <img src="https://img.shields.io/badge/Licen%C3%A7a-MIT-F7DF1E?style=for-the-badge" alt="Licença" />
  </p>
</div>

---

## 👨‍💻 Sobre o Projeto

O **Kallots** é um assistente virtual híbrido de desktop projetado para rodar nativamente em segundo plano no Windows. O projeto nasceu com uma filosofia rígida de prototipagem incremental: evoluir através de escopos pequenos, hiper controlados e com foco absoluto em manutenibilidade e arquitetura limpa, evitando qualquer complexidade prematura.

A grande força do ecossistema do Kallots está na sua **arquitetura modular**. O processamento de áudio acústico roda localmente (Edge Computing) para garantir privacidade e consumo de recursos próximo a zero em repouso, enquanto a tomada de decisões e inferência de intenções utilizam modelos de linguagem avançados (LLMs) em nuvem, garantindo respostas inteligentes em milissegundos.

---

## 🧰 Stack & Ecossistema

<div align="center">

### 💻 Core, Plataforma & Linguagem

<img src="https://img.shields.io/badge/.NET_8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8"/>
<img src="https://img.shields.io/badge/C%23_12.0-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#"/>
<img src="https://img.shields.io/badge/Windows_OS-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="Windows"/>

### 🧠 Inteligência Artificial & Motores de Áudio

<img src="https://img.shields.io/badge/Vosk_API-3776AB?style=for-the-badge&logoColor=white" alt="Vosk"/>
<img src="https://img.shields.io/badge/Whisper.net-10A37F?style=for-the-badge&logo=openai&logoColor=white" alt="Whisper"/>
<img src="https://img.shields.io/badge/Groq_Cloud-F55036?style=for-the-badge&logoColor=white" alt="Groq"/>
<img src="https://img.shields.io/badge/Llama_3.1-0466C8?style=for-the-badge&logo=meta&logoColor=white" alt="Llama 3"/>
<img src="https://img.shields.io/badge/Edge_TTS-333333?style=for-the-badge&logo=python&logoColor=white" alt="Edge TTS"/>

</div>

<br>

> ⚡ **Destaques Arquiteturais:** Injeção de Dependências, Interfaces Desacopladas, Proteção contra Race Conditions (Locks), Telemetria de Milissegundos e Fuzzy Matching Fonético.

---

## 🚀 O que já está rodando (MVP v1.0)

O escopo atual do MVP está validado, blindado e altamente responsivo nas seguintes frentes:

- [x] **Loop Acústico Contínuo:** Worker Service rodando em background sem travar a interface principal.
- [x] **Fuzzy Matching Fonético:** Motor de ativação inteligente que aceita aproximações fonéticas para contornar ruídos e sotaques em português (interceptando _"calotes"_ ou _"motos"_ e acionando o gatilho _"Kallots"_).
- [x] **Earcon de Latência Zero:** Feedback de UX instantâneo por meio de um _beep_ direto no hardware do Windows, dispensando o processamento assíncrono de voz apenas para o gatilho.
- [x] **Tranca de Concorrência (Mutex/Lock):** Mecanismo de estado rígido que impede o disparo de novos gatilhos físicos enquanto um pipeline de comando já está em andamento, protegendo o uso de memória não gerenciada.
- [x] **Telemetria de Debug:** Logs rastreáveis no terminal utilizando `Stopwatch` para auditar a velocidade exata (em milissegundos) da captura de áudio, transcrição e resposta da API.
- [x] **Executor Estático de Sistema:** Mapeamento duro (_hardcoded_) capaz de interagir com o SO:
  - 🖥️ `OPEN_VSCODE` -> Inicializa o VS Code.
  - 🌐 `OPEN_BROWSER` -> Abre o navegador padrão.
  - 🧮 `OPEN_CALCULATOR` -> Dispara a calculadora nativa.

---

## 📐 Fluxo Arquitetural

```text
[ Microfone (NAudio) ]
       │
       ▼
[ Vosk API (Ouvido Leve) ] ──(Fuzzy Match)──> [ Earcon Bip (UX) ]
                                                   │
[ Whisper (STT Pesado) ] <───(Gravação 5s) <───────┘ (Tranca de Estado Acionada)
       │
       ▼
[ Groq API (Cérebro Nuvem) ] ──(JSON Intenção)──> [ CommandExecutor ]
                                                   │
                                                   ▼
[ Edge TTS (Resposta Vocal) ] <──(Feedback)── [ Ação Executada no SO ]
```
