# 🏦 PagnanBank

Aplicativo **desktop** desenvolvido com **.NET 8** e **WPF** que simula o funcionamento de um **banco digital completo**, com núcleo bancário, produtos financeiros, administração, relatórios e simuladores.

O projeto foi construído utilizando arquitetura em camadas, seguindo os princípios de separação de responsabilidades, SOLID, MVVM e persistência local de dados.

---

## 💻 Funcionalidades

### Núcleo Bancário
* Login seguro com hashing BCrypt e bloqueio por tentativas
* Auto-cadastro de clientes
* Gestão de contas correntes
* Depósitos e saques
* Transferências e PIX
* Extrato de movimentações

### Produtos Financeiros
* Investimentos com aplicação e resgate (rendimento composto)
* Empréstimos parcelados (Tabela Price)
* Cartões virtual e físico (limite, bloqueio e fatura)
* Loja virtual com carrinho, cashback e pagamento no débito ou crédito parcelado

### Administração
* Criação, bloqueio, desbloqueio e exclusão de usuários
* Ajuste de saldo de teste
* Auditoria de operações com filtros
* Histórico de login
* Autorização por perfil validada na camada de serviço

### Evolução e Indicadores
* Dashboard com saldo, investimentos e empréstimos
* Gráfico de entradas x saídas
* Cashback acumulado
* Fluxo financeiro mensal

### Relatórios
* Exportação em PDF (QuestPDF)
* Exportação em Excel .xlsx (ClosedXML)
* Exportação em CSV e HTML

### Simuladores
* Financiamento (SAC)
* Empréstimo (Price)
* Investimento
* Quitação antecipada
* Renegociação de dívida

---

## 🏗️ Arquitetura

O projeto segue uma estrutura em camadas:

```text
PagnanBank
│
├── src
│   ├── BankingSystem.Domain          (regra de negócio pura)
│   ├── BankingSystem.Application      (serviços e casos de uso)
│   ├── BankingSystem.Infrastructure   (implementações transversais)
│   ├── BankingSystem.Persistence      (acesso a dados / EF Core)
│   └── BankingSystem.Desktop          (apresentação / WPF)
│
└── tests
    └── BankingSystem.Tests            (testes de unidade)
```

### Responsabilidades

#### Domain
Contém o coração da aplicação, sem dependências externas:
* Entidades de domínio
* Enums
* Padrão Result
* Cálculos financeiros reutilizáveis

#### Application
Orquestra a regra de negócio:
* Serviços e casos de uso
* DTOs
* Interfaces de repositório
* Validações (FluentValidation)

#### Infrastructure
Implementações transversais:
* Hashing de senha (BCrypt)
* Provedor de data/hora

#### Persistence
Responsável pelo acesso aos dados:
* Entity Framework Core
* SQLite
* Repositórios e Unit of Work
* Seed inicial de usuários e catálogo de produtos

#### Desktop
Camada de apresentação:
* Janelas e páginas (WPF / XAML)
* ViewModels (MVVM)
* Composition root (Injeção de Dependência, Serilog, bootstrap)

---

## 🛠️ Tecnologias Utilizadas

* .NET 8
* WPF
* C#
* XAML
* Entity Framework Core
* SQLite
* Microsoft.Extensions.DependencyInjection / Hosting
* FluentValidation
* Serilog
* BCrypt.Net
* QuestPDF
* ClosedXML
* xUnit / FluentAssertions

---

## 📂 Principais Recursos

### Contas e Movimentações
Gestão de contas correntes com depósitos, saques, transferências e PIX.

### Produtos Financeiros
Investimentos, empréstimos, cartões e loja virtual com cashback.

### Auditoria
Registro de todas as operações relevantes, com filtros por módulo e resultado.

### Relatórios
Exportação do extrato e do resumo financeiro em PDF, Excel, CSV e HTML.

### Simuladores
Ferramentas reutilizáveis de financiamento, quitação antecipada e renegociação.

---

## 🚀 Como Executar

### Pré-requisitos
* Visual Studio 2022 17.8+ ou Visual Studio 2026
* .NET 8 SDK
* Windows (aplicação WPF)

### Clonar o Projeto
```bash
git clone https://github.com/SEU-USUARIO/PagnanBank.git
```

### Restaurar Dependências
```bash
dotnet restore
```

### Executar
```bash
dotnet run --project src/BankingSystem.Desktop
```

Ou abra a solução `PagnanBank.sln` no Visual Studio, defina **BankingSystem.Desktop** como projeto de inicialização e execute (F5).

> A licença **Community** do QuestPDF (gratuita para uso individual) já está configurada no arranque da aplicação.

### Credenciais de demonstração

| Perfil | E-mail | Senha |
|---|---|---|
| Administrador | `admin@bank.local` | `Admin@123` |
| Cliente | `cliente@bank.local` | `Cliente@123` |

Novos clientes também podem se cadastrar pela própria tela de login.

---

## 🗄️ Banco de Dados

O aplicativo utiliza **SQLite** para armazenamento local.

O banco é inicializado automaticamente através do processo de Seed, carregando:

* Usuários de demonstração (administrador e cliente)
* Conta bancária inicial
* Catálogo de produtos da loja virtual
* Estruturas iniciais da aplicação

As senhas nunca são armazenadas em texto puro — apenas o hash BCrypt.

---

## 📈 Roadmap

* [ ] Notificações
* [ ] Agendamentos de transações
* [ ] Tela de detalhe do cliente para o administrador
* [ ] Backup automático
* [ ] Migrations do EF Core
* [ ] Migração para SQL Server
* [ ] Dashboard de evolução com gráficos avançados

---

## 👨‍💻 Autor

**Felipe Pagnan**

Software Engineer especializado em desenvolvimento .NET, arquitetura de software e aplicações multiplataforma.

LinkedIn:
https://www.linkedin.com/in/felipe-pagnan/

---

## 📄 Licença

Este projeto está sob a licença Pagnan.
Sinta-se à vontade para estudar, utilizar e contribuir com melhorias.
"# PagnanBank" 
