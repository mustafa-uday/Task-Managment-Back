# Team Tasks – Backend

A production-style **Task Management Backend API** built with **.NET 9** following **Clean Architecture** principles.  
This backend is designed to be scalable, maintainable, and suitable for real-world deployment.

---

## 🧠 Overview

The backend provides a secure and well-structured REST API for managing tasks and users.  
It demonstrates best practices in API design, validation, authentication, and separation of concerns.

---

## 🏗️ Architecture

The project follows **Clean Architecture**, ensuring that business logic remains independent of frameworks and external concerns.


---

## ⚙️ Tech Stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- FluentValidation
- Swagger / OpenAPI

---

## 🔐 Authentication

- JWT-based authentication
- Secure API endpoints
- Role-ready authorization structure

---

## 🧪 Validation & Quality

- Validation enforced at API boundaries using **FluentValidation**
- No fat controllers
- Business logic isolated in the Application layer
- Clean commit history

---

## 🚀 Running the Project

### Prerequisites
- .NET 9 SDK
- PostgreSQL

### Steps
```bash
dotnet restore
dotnet ef database update
dotnet run
