# 🚀 PixSystem Core

Este projeto é uma implementação robusta de um sistema de processamento de Pix, focado em alta disponibilidade, consistência e boas práticas de arquitetura. O sistema utiliza **DDD (Domain-Driven Design)**, **CQRS (via MediatR)** e conta com uma suíte de testes de integração automatizados.

## 🛠 Tecnologias Principais

* **.NET 8.0**
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

### Comandos
Para rodar os testes, use o seguinte comando na raiz do projeto:

```bash
dotnet test
