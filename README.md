# 🚀 PixSystemCore

Este projeto é uma implementação robusta de um sistema de processamento de Pix, focado em alta disponibilidade, consistência e boas práticas de arquitetura. O sistema utiliza **DDD (Domain-Driven Design)**, **CQRS (via MediatR)** e conta com uma suíte de testes de integração automatizados.

## 🛠 Tecnologias Principais

* **.NET 10**
* **Entity Framework Core** (SQL Server)
* **MediatR** (Padrão CQRS)
* **Docker & Testcontainers** (Para testes de integração isolados)
* **FluentAssertions** (Para validação de testes)

## 🏗 Arquitetura do Projeto

O projeto é organizado seguindo os princípios de Clean Architecture:

* **Domain:** Entidades, Value Objects e interfaces de repositório.
* **Application:** Comandos (Commands), Queries e Handlers (MediatR).
* **Infrastructure:** Implementação dos repositórios, persistência (EF Core) e configurações de banco de dados.
* **Api:** Controllers e Middlewares de tratamento de erros.

## 📋 Funcionalidades

- [x] Abertura de contas.
- [x] Execução de transferências Pix (com controle de idempotência).
- [x] Consulta de saldo e extrato.
- [x] Consulta de contas por chave Pix.
- [x] Estorno de transações Pix.

## 🧪 Executando os Testes

Este projeto utiliza **Testcontainers** para garantir que os testes de integração rodem em um ambiente real (SQL Server via Docker), garantindo que seu código funcione com o banco de dados sem depender de instalações locais complexas.

### Pré-requisitos
- Docker Desktop instalado e rodando.
- .NET 8.0 SDK.

## 🏗 Diagrama de Arquitetura

O fluxo de processamento do sistema segue uma estrutura desacoplada, garantindo que a lógica de negócio seja independente de frameworks externos:

```mermaid
graph TD
    API[API Controllers] --> MediatR[MediatR CQRS]
    MediatR --> App[Application Handlers]
    App --> Domain[Domain Entities]
    App --> Infra[Infrastructure EF Core]
    Infra --> DB[(SQL Server)]
    Infra --> Kafka[Kafka/Redis]
 ```

## 🏁 Getting Started

Para obter uma cópia local funcionando, siga estes passos simples.

### Pré-requisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e rodando

### Passo a Passo

1. Clone o repositório:
   ```bash
   git clone [https://github.com/joaodev2005/PixSystemCore.git](https://github.com/joaodev2005/PixSystemCore.git)
   cd PixSystemCore
   ```
2. Suba a infraestrutura necessária (SQL Server, Kafka e Redis) via Docker Compose:
   ```bash
   docker compose up -d
   ```
3. Rode a aplicação principal:
   ```bash
   dotnet run --project src/PixSystemCore.API
   ```
4. Para rodar a suíte de testes localmente:
   ```bash
   dotnet test
   ```

