# Kallots
Um AI Agent assistente para windows

---

## Stack e Arquitetura 
Linguagem: C# (.NET)

Detecção de Voz (Wake Word): Vosk (Offline, Leve, Streaming)

Compreensão de Voz (STT): Whisper.net (Offline, Preciso, Batching)

Cérebro (LLM): LLaMA 70B via Groq API (Nuvem, Rápido)

Voz (TTS): Edge TTS (Nuvem, Alta qualidade)

Ações: System.Diagnostics.Process (Nativo do C# para comandos Windows)

Arquitetura: Modular com Inversão de Dependência (Interfaces para isolar I/O).

---

## Backlog do MVP (Kallots)
Step 1: Project Setup & Core Interfaces

Criar o projeto C# (vamos usar um Worker Service, que é ideal para rodar silenciosamente em background no Windows).

Definir as interfaces base (ex: ISttProvider, ILlmProvider, ITtsProvider, ICommandExecutor) para garantir a Inversão de Dependência desde o dia zero.

Step 2: The Ear (Wake Word Detection)

Integrar a biblioteca Vosk.

Configurar a captura contínua do microfone.

Criar o loop que escuta o ambiente e dispara um evento (gatilho) apenas quando ouvir a palavra "Kallots".

Step 3: The Transcriber (Speech-to-Text)

Integrar o Whisper.net.

Criar o mecanismo que grava o áudio logo após o gatilho do Vosk.

Converter esse áudio gravado em texto estruturado (string).

Step 4: The Brain (LLM Integration)

Configurar a comunicação HTTP (REST) com a API do Groq (LLaMA 70B).

Criar um System Prompt forte para que o Kallots entenda que é um assistente Windows e retorne intenções claras em vez de longos textos.

Step 5: The Hands (OS Execution)

Implementar a classe que utiliza System.Diagnostics.Process para executar as intenções mapeadas (ex: abrir VS Code, abrir navegador).

Step 6: The Voice (Edge TTS)

Criar a integração com o Edge TTS.

Desenvolver o fluxo de boot do sistema ("Olá Usuário...").

Fazer o sistema "falar" antes e depois de executar um comando.

Step 7: The Orchestrator & OS Boot

Amarrar todos os serviços na classe principal (Worker).

Configurar o registro do Windows ou a pasta Startup para que o executável inicie automaticamente e de forma invisível no login.

---
