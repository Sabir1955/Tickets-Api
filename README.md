# 🔐 Authentication API

A secure RESTful Authentication API built using **C#**, **ASP.NET Core .NET 8**, **Entity Framework Core**, and **SQL Server**.

## 🛠️ Technologies

* **C#**
* **ASP.NET Core**
* **.NET 8**
* **Entity Framework Core**
* **SQL Server**
* **JWT Authentication**
* **Swagger / OpenAPI**

## ✨ Features

* 👤 User Registration
* 🔑 User Login
* 🔐 JWT Authentication
* 🔒 Password Hashing
* 📧 Email Validation
* 🗄️ SQL Server Database Integration
* 🧩 Entity Framework Core
* 📖 Swagger API Documentation

## 📌 API Endpoints

### Authentication

| Method | Endpoint                | Description                  |
| ------ | ----------------------- | ---------------------------- |
| `POST` | `/api/v1/auth/register` | Register a new user          |
| `POST` | `/api/v1/auth/login`    | Login and generate JWT token |

## 🔄 Authentication Flow

```text
User
 │
 ├── Register
 │      ↓
 │   Password Hashing
 │      ↓
 │   SQL Server
 │
 └── Login
        ↓
    Validate Credentials
        ↓
    Generate JWT Token
        ↓
    Access Protected APIs
```

## 🗄️ Database

The application uses **SQL Server** with **Entity Framework Core** for database operations.

### Migration Commands

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## ▶️ How to Run

### 1. Clone the Repository

```bash
git clone https://github.com/Sabir1955/Tickets-Api.git
```

### 2. Open the Project

```bash
cd Tickets-Api
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Apply Database Migration

```bash
dotnet ef database update
```

### 5. Run the API

```bash
dotnet run
```

## 📖 Swagger

After running the application, open Swagger to test the API endpoints.

```text
https://localhost:<port>/swagger
```

Use the **Authorize 🔒** button in Swagger to provide the JWT token returned by the login endpoint.

## 🔒 Security

* Passwords are stored using secure hashing.
* JWT tokens are used for authentication.
* Protected endpoints require a valid Bearer token.
* Sensitive configuration values should be stored securely and should not be committed to GitHub.

## 👨‍💻 Author

**Gulam Sabir**

GitHub:
https://github.com/Sabir1955
