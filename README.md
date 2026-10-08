# PracticalTestDotNet - Backend de Gestão de Pedidos (.NET 10)

Sistema de Gestão de Pedidos para E-Commerce construído com .NET 10.

---

## Arquitetura do Sistema

A solução foi estruturada aplicando rigorosamente os princípios de **Clean Architecture** e **CQRS (Command Query Responsibility Segregation)**:

```text
PracticalTestDotNet/
├── src/
│   ├── PracticalTestDotNet.Domain/          # Entidades (Order, OrderItem), Enums, Exceções e Interfaces (IOrderRepository)
│   ├── PracticalTestDotNet.Application/     # CQRS (Commands, Queries, DTOs), Pipeline Behaviors (Validation, Logging)
│   ├── PracticalTestDotNet.Infrastructure/  # EF Core DbContext, SQLite, Mapeamentos, Repositórios e Gerador JWT
│   └── PracticalTestDotNet.Api/             # Endpoints (Minimal API), Middlewares, Serilog, OpenTelemetry e Scalar API UI
│
├── tests/
│   ├── PracticalTestDotNet.UnitTests/       # Testes unitários para o Domínio e Handlers do MediatR (xUnit + NSubstitute + FluentAssertions)
│   └── PracticalTestDotNet.IntegrationTests/# Testes de integração E2E com WebApplicationFactory
│
├── docker-compose.yml                       # Docker Compose com a API e serviço de análise SonarQube
├── Dockerfile                               # Build multi-stage otimizado para .NET 10
└── README.md                                # Documentação técnica da solução
```

---

## Justificativas Arquiteturais

### 1. Minimal APIs vs Controllers
- **Escolha**: **Minimal APIs com Endpoint Groups (`MapGroup`)**.
- **Justificativa**: No .NET 10, as Minimal APIs são a abordagem moderna e idiomática recomendada pela Microsoft. Elas oferecem performance superior (menor alocação de memória e execução sem overhead de descoberta de controllers via reflexão por requisição), integração nativa com o ecossistema OpenAPI/Scalar e excelente organização quando modularizadas em classes de extensão (`AuthEndpoints`, `OrderEndpoints`).

### 2. Evitando Repositórios Genéricos (`IRepository<T>`)
- **Escolha**: **Repositório Específico (`IOrderRepository`)**.
- **Justificativa**: Repositórios genéricos frequentemente viram abstrações vazias que apenas repassam chamadas do EF Core ou vazam detalhes de infraestrutura para a camada de aplicação. O `IOrderRepository` expõe unicamente os métodos necessários para a regra de negócio do domínio (`AddAsync`, `GetByIdAsync`, `GetPaginatedAsync`, `UpdateAsync`), mantendo o contrato explícito e testável.

### 3. Encapsulamento de Regras no Domínio (Rich Domain Model)
- **Cálculo de `TotalAmount`**: Calculado **estritamente dentro da entidade `Order`** (`TotalAmount => _items.Sum(item => item.UnitPrice * item.Quantity)`). A camada de aplicação não realiza esse cálculo.
- **Validação de Cancelamento**: A regra de que *"Apenas pedidos em status Pending podem ser cancelados"* reside no método `Order.Cancel()`, garantindo que o domínio proteja a consistência de seus estados.
- **Validação de Itens e Quantidades**: `Order` e `OrderItem` garantem em suas fábricas/construtores que pedidos possuem no mínimo 1 item e que `UnitPrice` e `Quantity` são estritamente maiores que zero.

---

## Como Executar o Projeto

### Pré-requisitos
- .NET 10 SDK
- Docker & Docker Compose (opcional para execução em container)

---

### Opção 1: Execução Local (.NET CLI)

1. **Restaurar dependências e compilar a solução:**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Executar os testes unitários e de integração:**
   ```bash
   dotnet test
   ```

3. **Executar a API:**
   ```bash
   dotnet run --project src/PracticalTestDotNet.Api/PracticalTestDotNet.Api.csproj
   ```

A API iniciará localmente e aplicará as **migrações do banco SQLite (`orders.db`) automaticamente** ao subir.

- **Scalar API Documentation (Swagger Moderno):** `http://localhost:5252/scalar/v1` (ou na porta HTTP/HTTPS exibida no console).

---

### Opção 2: Execução via Docker Compose

1. **Subir os containers da API e do SonarQube:**
   ```bash
   docker-compose up --build
   ```

2. **Acessar a aplicação:**
   - **API Backend:** `http://localhost:8080`
   - **SonarQube Dashboard:** `http://localhost:9000`

---

## Endpoints da API & Autenticação

### Autenticação JWT

#### `POST /auth/login`
Autentica o usuário fixo em memória e retorna o token JWT Bearer.

- **Request Body:**
  ```json
  {
    "email": "dev@martech.com",
    "password": "Senha@123"
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "tokenType": "Bearer",
    "expiresInSeconds": 3600
  }
  ```

---

### Pedidos (`/api/orders`) — Requer Header `Authorization: Bearer <TOKEN>`

#### 1. `POST /api/orders`
Cria um novo pedido com seus itens.

- **Request Body:**
  ```json
  {
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "items": [
      {
        "productName": "Teclado Mecânico RGB",
        "quantity": 1,
        "unitPrice": 350.00
      },
      {
        "productName": "Mouse Gamer",
        "quantity": 2,
        "unitPrice": 120.00
      }
    ]
  }
  ```
- **Response (201 Created):**
  ```json
  {
    "id": "e8d4a9c1-1234-5678-90ab-cdef12345678",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Pending",
    "createdAt": "2026-10-07T16:14:00Z",
    "totalAmount": 590.00,
    "items": [
      {
        "id": "...",
        "orderId": "...",
        "productName": "Teclado Mecânico RGB",
        "quantity": 1,
        "unitPrice": 350.00
      },
      {
        "id": "...",
        "orderId": "...",
        "productName": "Mouse Gamer",
        "quantity": 2,
        "unitPrice": 120.00
      }
    ]
  }
  ```

#### 2. `GET /api/orders?page=1&pageSize=10`
Lista os pedidos cadastrados de forma paginada.

- **Response (200 OK):**
  ```json
  {
    "items": [ ... ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
  ```

#### 3. `GET /api/orders/{id}`
Busca um pedido específico pelo seu ID (Guid).

- **Response (200 OK / 404 Not Found)**

#### 4. `PATCH /api/orders/{id}/cancel`
Cancela um pedido pendente. Lança erro se o pedido já estiver cancelado ou em outro status.

- **Response (200 OK com status "Cancelled" / 400 Bad Request)**

---

## Recursos Desejáveis Implementados

- [x] **Serilog Pipeline Logging**: Logs estruturados em console gravando nome da command/query, payload sanitizado e tempo exato de execução em milissegundos.
- [x] **OpenTelemetry**: Rastreamento (Tracing) e Métricas configurados exportando para console.
- [x] **Testes de Integração E2E**: Testes integrados com `WebApplicationFactory<Program>` testando fluxo de autenticação e criação de pedidos.
- [x] **SonarQube no Docker Compose**: Serviço pronto no `docker-compose.yml` para execução do scanner de qualidade de código.
