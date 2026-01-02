# ⚽ Futebol API - Backend do Sistema de Sorteio

API RESTful desenvolvida em .NET 9 para gerenciamento de torneios de futebol com sorteio automático de times, controle de partidas e classificações.

## 🚀 Tecnologias

- **.NET 9** - Framework web moderno
- **ASP.NET Core Minimal APIs** - Arquitetura de endpoints
- **Entity Framework Core** - ORM para acesso a dados
- **SQLite** - Banco de dados leve e portátil
- **JWT (JSON Web Tokens)** - Autenticação stateless
- **BCrypt** - Hash seguro de senhas
- **Google OAuth 2.0** - Autenticação via Google

## 📋 Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQLite (incluído no EF Core)

## 🔧 Instalação e Configuração

### 1. Restaurar Dependências

```bash
dotnet restore
```

### 2. Configurar Variáveis de Ambiente

Edite `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "sua-chave-secreta-muito-segura-aqui",
    "Issuer": "FutebolApi",
    "Audience": "FutebolClient",
    "ExpiryInHours": 24
  },
  "GoogleAuth": {
    "ClientId": "seu-google-client-id.apps.googleusercontent.com"
  }
}
```

### 3. Aplicar Migrations

```bash
dotnet ef database update
```

### 4. Executar a API

```bash
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## 🏗️ Arquitetura

```
Futebol.Api/
├── Domain/                  # Entidades de domínio
│   ├── User.cs              # Usuário (autenticação)
│   ├── Organizador.cs       # Organizador de torneios
│   ├── Jogador.cs           # Jogador
│   ├── Sorteio.cs           # Sorteio/Torneio
│   ├── Time.cs              # Time gerado
│   ├── TimeJogador.cs       # Relação Time-Jogador
│   └── Partida.cs           # Partida/Jogo
│
├── Dtos/                    # Data Transfer Objects
│   ├── AuthDtos.cs          # DTOs de autenticação
│   ├── JogadorDto.cs        # DTOs de jogador
│   ├── OrganizadorDtos.cs   # DTOs de organizador
│   ├── SorteioDtos.cs       # DTOs de sorteio
│   ├── TimeDto.cs           # DTOs de time
│   ├── PartidaDto.cs        # DTOs de partida
│   └── ApiEnvelope.cs       # Wrapper de resposta
│
├── Endpoints/               # Endpoints da API (Minimal APIs)
│   ├── Auth.cs              # Autenticação e perfil
│   ├── Organizadores.cs     # CRUD de organizadores
│   ├── Jogadores.cs         # CRUD de jogadores
│   ├── Sorteio.cs           # Gerenciamento de sorteios
│   ├── Partidas.cs          # Registro de partidas
│   └── UsersEndpoints.cs    # Gerenciamento de usuários
│
├── Infrastructure/          # Infraestrutura e persistência
│   ├── FutebolContext.cs    # DbContext do EF Core
│   └── Mappings/            # Configurações de entidades
│       ├── UserMap.cs
│       ├── OrganizadorMap.cs
│       └── JogadorMap.cs
│
├── Migrations/              # Migrations do EF Core
│   └── ...
│
├── Utils/                   # Utilitários
│   └── Security.cs          # JWT e hash de senhas
│
└── Program.cs               # Entry point e configuração
```

## 📡 Endpoints da API

### 🔐 Autenticação (`/api/auth`)

| Método | Endpoint | Autenticação | Descrição |
|--------|----------|--------------|-----------|
| POST | `/register/organizador` | ✅ | Registrar novo organizador |
| POST | `/login` | ❌ | Login com email/senha |
| POST | `/google` | ❌ | Login via Google OAuth |
| GET | `/profile` | ✅ | Obter perfil do usuário |
| POST | `/change-password` | ✅ | Alterar senha |
| POST | `/update-name` | ✅ | Atualizar nome |
| POST | `/update-codigo` | ✅ | Atualizar código do organizador |

### 👥 Jogadores (`/api/jogadores`)

| Método | Endpoint | Autenticação | Descrição |
|--------|----------|--------------|-----------|
| GET | `/` | ✅ | Listar jogadores do organizador |
| POST | `/` | ✅ | Cadastrar novo jogador |
| PUT | `/{id}` | ✅ | Atualizar jogador |
| DELETE | `/{id}` | ✅ | Remover jogador |

### 🎲 Sorteios (`/api/sorteios`)

| Método | Endpoint | Autenticação | Descrição |
|--------|----------|--------------|-----------|
| POST | `/` | ✅ | Criar novo sorteio |
| GET | `/{id}/times` | ❌ | Listar times do sorteio (público) |
| POST | `/{id}/times` | ✅ | Adicionar times ao sorteio |
| GET | `/{sorteioId}/times/{timeId}/jogadores` | ❌ | Listar jogadores de um time (público) |

### ⚽ Partidas (`/api/partidas`)

| Método | Endpoint | Autenticação | Descrição |
|--------|----------|--------------|-----------|
| GET | `/sorteio/{sorteioId}` | ❌ | Listar partidas do sorteio (público) |
| POST | `/` | ✅ | Registrar nova partida |
| PUT | `/{id}` | ✅ | Atualizar resultado da partida |

## 🔑 Autenticação JWT

A API utiliza JWT Bearer Token para autenticação. Após o login, o token deve ser incluído no header:

```http
Authorization: Bearer <seu-token-jwt>
```

### Exemplo de Resposta de Login

```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João Silva",
    "email": "joao@example.com"
  }
}
```

## 📊 Modelos de Dados

### User
```csharp
{
  "id": "guid",
  "nome": "string",
  "email": "string",
  "passwordHash": "string",
  "ativo": "bool",
  "contaGoogle": "bool",
  "createdAt": "datetime"
}
```

### Jogador
```csharp
{
  "id": "guid",
  "nome": "string",
  "peso": "int",           // 1-10 (habilidade)
  "destaque": "bool",      // Jogador destacado
  "organizadorId": "guid"
}
```

### Sorteio
```csharp
{
  "id": "guid",
  "nome": "string",
  "dataCriacao": "datetime",
  "quantidadeTimes": "int",
  "organizadorId": "guid"
}
```

### Time
```csharp
{
  "id": "guid",
  "nome": "string",
  "cor": "string",
  "sorteioId": "guid",
  "jogadores": "TimeJogador[]"
}
```

### Partida
```csharp
{
  "id": "guid",
  "sorteioId": "guid",
  "timeCasaId": "guid",
  "timeForaId": "guid",
  "golsCasa": "int?",
  "golsFora": "int?",
  "dataPartida": "datetime"
}
```

## 🛡️ Segurança

### Hash de Senhas
- Utiliza **BCrypt** com salt automático
- Senha nunca armazenada em texto plano

### JWT
- Tokens assinados com chave secreta configurável
- Expiração configurável (padrão: 24 horas)
- Claims incluem: UserId, Email, Role

### Autorização
- Endpoints protegidos com `[Authorize]`
- Endpoints públicos marcados com `[AllowAnonymous]`
- Validação de propriedade de recursos (jogadores, sorteios)

### Google OAuth
- Verificação do token Google no backend
- Criação automática de conta na primeira autenticação
- Flag `ContaGoogle` para usuários OAuth

## 🗄️ Banco de Dados

### SQLite
- Arquivo: `futebol.db` (gerado após migrations)
- Leve e portátil
- Ideal para desenvolvimento e deploy simples

### Migrations

Criar nova migration:
```bash
dotnet ef migrations add NomeDaMigration
```

Aplicar migrations:
```bash
dotnet ef database update
```

Reverter última migration:
```bash
dotnet ef migrations remove
```

## 🧪 Testando a API

### Usando cURL

**Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "senha": "senha123"
  }'
```

**Listar Jogadores (autenticado):**
```bash
curl -X GET http://localhost:5000/api/jogadores \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

### Usando Swagger (Development)

Em ambiente de desenvolvimento, acesse:
```
http://localhost:5000/swagger
```

## ⚙️ Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `Jwt:Key` | Chave secreta para assinar tokens | - |
| `Jwt:Issuer` | Emissor do token | FutebolApi |
| `Jwt:Audience` | Audiência do token | FutebolClient |
| `Jwt:ExpiryInHours` | Tempo de expiração em horas | 24 |
| `GoogleAuth:ClientId` | Client ID do Google OAuth | - |

## 📦 Dependências Principais

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.*" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.*" />
<PackageReference Include="Google.Apis.Auth" Version="1.68.*" />
```

## 🚀 Deploy

### Publicar para Produção

```bash
dotnet publish -c Release -o ./publish
```

### Configurações de Produção

Em `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=futebol.db"
  }
}
```

## 📝 Licença

Este projeto está sob a licença especificada no arquivo [LICENSE.txt](../LICENSE.txt).

## 👨‍💻 Autor

**Jackson**
- GitHub: [@jacksontrr](https://github.com/jacksontrr)

---

Desenvolvido com ⚽ para facilitar a organização de torneios de futebol.
