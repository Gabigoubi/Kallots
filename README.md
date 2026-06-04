<div align="center">
  <h1>👋 Olá, eu sou Gabriel de Abreu</h1>
  <h3>Engenheiro de Software | Full Stack | Focado em Arquitetura & Resolução de Problemas</h3>
  
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&weight=600&size=20&duration=4000&pause=1000&color=ED8B00&center=true&vCenter=true&width=600&lines=Prototipagem+de+Sistemas;Arquitetura+de+Software;Java,+Go,+Python;Resolvendo+B.O.+diariamente" alt="Typing SVG" />
  
  <p align="center">
    <a href="https://www.linkedin.com/in/gabriel-de-abreu-4a6804378/">
      <img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="LinkedIn" />
    </a>
  </p>
  <img src="https://komarev.com/ghpvc/?username=Gabigoubi&label=Visitas+no+Perfil&color=ED8B00&style=flat" alt="Contador de Views" />
</div>

---

## 👨‍💻 Sobre Mim

Sou um desenvolvedor com pensamento voltado para otimização, eficiência e arquitetura de software. Com uma base acadêmica sólida em **prototipagem de sistemas**, meu objetivo no dia a dia é mentalizar processos complexos, desenvolver, integrar e resolver o B.O., seja qual for o ecossistema.

Acredito que linguagens e frameworks são apenas ferramentas na maleta de um engenheiro, e o segredo é sempre puxar a ferramenta certa para o trabalho certo. Apesar do meu amor declarado pelo **Java** (preferindo sempre estruturar código e documentação técnica em inglês para manter o mais alto padrão), amo construir microsserviços em **Go** _(APENAS SE NECESSÁRIO KKKK)_, subir APIs em Python e, principalmente, debugar, sendo este último o meu esporte preferido.

---

## 🧰 Minha Caixa de Ferramentas

<div align="center">

### 💻 Linguagens Principais

<img src="https://img.shields.io/badge/Java-ED8B00?style=for-the-badge&logo=openjdk&logoColor=white" alt="Java"/>
<img src="https://img.shields.io/badge/Go-00ADD8?style=for-the-badge&logo=go&logoColor=white" alt="Go"/>
<img src="https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white" alt="Python"/>
<img src="https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black" alt="JavaScript"/>

### ⚙️ Backend, APIs & Frameworks

<img src="https://img.shields.io/badge/Spring_Boot-6DB33F?style=for-the-badge&logo=spring-boot&logoColor=white" alt="Spring Boot"/>
<img src="https://img.shields.io/badge/FastAPI-005571?style=for-the-badge&logo=fastapi&logoColor=white" alt="FastAPI"/>
<img src="https://img.shields.io/badge/Node.js-43853D?style=for-the-badge&logo=node.js&logoColor=white" alt="Node.js"/>

### 🗄️ Dados, Infra & Arquitetura

<img src="https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL"/>
<img src="https://img.shields.io/badge/Hibernate-59666C?style=for-the-badge&logo=Hibernate&logoColor=white" alt="Hibernate"/>
<img src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker"/>

### 🧠 Inteligência Artificial & Inferência

<img src="https://img.shields.io/badge/Ollama-000000?style=for-the-badge&logo=ollama&logoColor=white" alt="Ollama"/>
<img src="https://img.shields.io/badge/OpenAI-412991?style=for-the-badge&logo=openai&logoColor=white" alt="OpenAI"/>
<img src="https://img.shields.io/badge/Llama_3-0466C8?style=for-the-badge&logo=meta&logoColor=white" alt="Llama 3"/>
<img src="https://img.shields.io/badge/GPT4All-212121?style=for-the-badge&logo=gnometerminal&logoColor=white" alt="GPT4All"/>
<img src="https://img.shields.io/badge/Groq-F55036?style=for-the-badge&logoColor=white" alt="Groq"/>

</div>

<br>

> ⚡ **Extras:** Operações Assíncronas, Sistemas Distribuídos, Serviços RESTful e Integração de Áudio (TTS).

---

## 🚀 Projetos em Destaque

### 🎙️ Narrador IA (Minecraft Mod)

Um sistema narrativo reativo e em tempo real para Minecraft, onde uma Inteligência Artificial julga, narra e reage às ações do jogador dinamicamente.

- **Arquitetura:** O cliente (Java/Fabric) atua como um sensor de telemetria inteligente, possuindo um compressor de eventos e sistema de buffer anti-perda. Ele se comunica via HTTP com o servidor local (Python/FastAPI).
- **Cérebro (IA):** O backend processa a matemática de engajamento do jogador (risco, tédio e progresso) para ditar o tom da cena e faz a inferência usando LLMs locais (Ollama) ou em nuvem.
- **Output:** Geração de áudio TTS assíncrono em tempo real (Stream) direto para o cliente do jogo.
- **Stack:** `Java` `Fabric API` `Python` `FastAPI` `Prompt Engineering` `LLMs`

### ⚙️ Smartflow

Solução híbrida baseada em microsserviços desenvolvida para resolver o gap de sincronização entre leads de revisão e agendamentos reais. O ecossistema aproveita a robustez do Java para regras de negócio e a alta performance do Go para processamento em lote e concorrência.

- **API Core (Gestão):** Desenvolvida com Spring Boot e PostgreSQL, atua como o sistema de registro principal para gestão de clientes e criação de agendamentos (`POST /agendamentos`).
- **Motor Worker (Go):** Serviço focado em alta performance que realiza varreduras periódicas com uma engine de elegibilidade avançada (filtros de 90 dias, proteção antiduplicidade e validação de agendamentos ativos).
- **Mensageria & Rate Limiting:** Integração direta com a Telegram Bot API contendo controle de vazão rigoroso (`time.Ticker` de 100ms), limitando o processamento assíncrono a 10 leads por segundo e garantindo resiliência sob alto estresse (validado com mais de 300 clientes simultâneos).
- **Stack:** `Java` `Spring Boot` `Go` `PostgreSQL` `Docker` `Telegram API`

---

## 📊 Minhas Estatísticas

<div align="center">
  <img src="https://streak-stats.demolab.com?user=Gabigoubi&theme=tokyonight&hide_border=true&locale=pt-br" alt="GitHub Streak" />
</div>

<br>

<div align="center">
  <img src="https://media.tenor.com/RyF8iVDEf3cAAAAj/16bit-80s.gif" width="250" alt="Meu GIF bonitinho" />
</div>
