# 🚀 Enterprise CRM Backend (.NET 8)

An enterprise-grade Customer Relationship Management (CRM) backend built using **ASP.NET Core 8**, **Clean Architecture**, **CQRS**, **Entity Framework Core**, and **JWT Authentication**.

This project demonstrates modern backend development practices including layered architecture, repository pattern, authentication, dynamic CRM modules, and RESTful APIs.

---

## 📌 Features

- 🔐 JWT Authentication & Authorization
- 👤 User Registration & Login
- 🔄 Refresh Token Support
- 🏢 Multi-Tenant Architecture
- 🏗 Clean Architecture
- ⚡ CQRS using MediatR
- 📦 Repository Pattern
- 🗄 Entity Framework Core
- 📖 Swagger/OpenAPI Documentation
- 🌐 RESTful API Design
- 🛡 Global Exception Handling
- 📋 Response Wrapper
- 📝 Logging Support

---

# 📚 API Modules

### Authentication
- User Registration
- User Login
- Refresh Token
- Logout
- Forgot Password
- Reset Password

### Contacts
- Create Contact
- Update Contact
- Delete Contact
- Restore Contact
- Search Contacts

### Dynamic CRM

The project includes a fully dynamic CRM system.

Modules include:

- Dynamic Entities
- Dynamic Fields
- Dynamic Field Options
- Dynamic Field Values
- Dynamic Records
- Dynamic Relationships
- Dynamic Permissions
- Dynamic Views

### Lead Management

- Lead Attachments
- Lead Merge

### Users

- Current User
- Assignable Users

---

# 🏗 Architecture

The project follows **Clean Architecture** principles.

```
                Presentation Layer
                    (CRM.api)
                         │
                         ▼
               Application Layer
               (CQRS + MediatR)
                         │
                         ▼
                 Domain Layer
          (Entities & Business Rules)
                         │
                         ▼
             Infrastructure Layer
      (EF Core, SQL Server, Repository)
```

---

# 📂 Project Structure

```
CRM.api
│
├── Controllers
├── Middleware
├── Extensions
├── Program.cs
│

Crm.Application
│
├── Commands
├── Queries
├── DTOs
├── Interfaces
├── Services
│

Crm.Domain
│
├── Entities
├── Common
├── Enums
├── Interfaces
│

Crm.Infrastructure
│
├── Persistence
├── Repositories
├── Identity
├── Services
└── Migrations
```

---

# 🛠 Tech Stack

| Technology | Version |
|------------|---------|
| .NET | 8 |
| ASP.NET Core | 8 |
| Entity Framework Core | 8 |
| SQL Server | Latest |
| MediatR | Latest |
| AutoMapper | Latest |
| JWT Authentication | ✔ |
| Swagger | ✔ |
| Clean Architecture | ✔ |
| Repository Pattern | ✔ |
| CQRS | ✔ |

---

# 🔑 Authentication

The API uses JWT Bearer Authentication.

Protected endpoints require:

```
Authorization: Bearer {token}
```

---

# 📖 API Documentation

Swagger UI is available when running the application.

```
https://localhost:7006/swagger
```

---

# ⚙ Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- SQL Server
- Git

---

# 🚀 Getting Started

## Clone Repository

```bash
git clone https://github.com/kousikdas-dev/dotnet8-clean-crm.git
```

---

## Navigate

```bash
cd dotnet8-clean-crm
```

---

## Restore Packages

```bash
dotnet restore
```

---

## Update Connection String

Edit

```
appsettings.json
```

Update

```json
"ConnectionStrings": {
  "DefaultConnection": "YOUR_CONNECTION_STRING"
}
```

---

## Apply Database Migration

```bash
dotnet ef database update
```

---

## Run Project

```bash
dotnet run
```

or press **F5** in Visual Studio.


---

# 🔄 Git Workflow

This project follows a Feature Branch Workflow.

```
master
   │
   ├── feature/readme
   ├── feature/auth
   ├── feature/contact
   ├── feature/dynamic-entity
   └── feature/deployment
```

---

# 🚀 Future Improvements

- Azure Deployment
- GitHub Actions (CI)
- Docker Support
- Azure SQL Database
- Azure Key Vault
- Unit Testing
- Integration Testing
- Role Based Authorization
- Email Service
- Audit Logs
- Background Jobs

---

# 📈 Project Status

🚧 Under Active Development

---

# 🤝 Contributing

Contributions are welcome.

Feel free to fork the repository, create a feature branch, and submit a Pull Request.

---

# 👨‍💻 Author

**Kousik Das**

GitHub:

https://github.com/kousikdas-dev

---

# 📄 License

This project is licensed under the MIT License.