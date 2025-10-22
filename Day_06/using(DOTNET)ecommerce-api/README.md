# 🛒 E-Commerce API (.NET 9 Web API)

This project provides a fully working **ASP.NET Core 9 Web API** backend for an e-commerce platform. It features JWT authentication, role-based authorization, product and order management, coupon support, user profiles, admin functionalities, Google OAuth login, and built-in ASP.NET Core rate limiting.

👉 A working Docker setup using this API is available in the companion repository: [m-akgul/ecommerce-docker](https://github.com/m-akgul/ecommerce-docker)

---

## 🚀 Technologies

- **.NET 9** (ASP.NET Core Web API)
- **Entity Framework Core** (SQL Server)
- **JWT Authentication & Role Authorization**
- **Google OAuth Login** (using Google ID Token)
- **Rate Limiting** (`app.UseRateLimiter`)
- **Swagger**

---

## 📂 Project Structure

- ECommerce.API            --> Main Web API project
- ECommerce.Application    --> Application Layer (Services, DTOs, Interfaces)
- ECommerce.Domain         --> Domain Entities, Identity
- ECommerce.Infrastructure --> Background service and pdf invoice helper
- ECommerce.Persistence    --> EF Core DbContext
- ECommerce.Shared         --> Shared classes (ApiResponse, Exceptions)
- ECommerce.Tests          --> Unit Tests for services and controllers

---

## 🔐 Authentication

- JWT Token generation during login/register
- Login via:
  - Email + Password
  - Google OAuth2 (`/api/Auth/signin-google`)
- Role-based restrictions using `[Authorize(Roles = "...")]`

---

## 🚦 Rate Limiting

Enabled using ASP.NET Core built-in middleware.

- Default: 100 requests per 1 minute
- `/Auth/*`: 10 requests per 1 minute (more strict)
- Custom `429 Too Many Requests` handled in middleware

---

## ⚙ Configuration

Stored in `appsettings.json` or environment variables:

```json
"JwtSettings": {
  "Key": "...",
  "Issuer": "ECommerceApi",
  "Audience": "ECommerceClient",
  "ExpiresInMinutes": 60
},
"Google": {
  "ClientId": "GOOGLE_CLIENT_ID"
},
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Database=ECommerceDb;..."
}
```

---

## 🧪 Testing

You can use Swagger, Postman or any REST client to test the endpoints. All endpoints return consistent response format using `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Product fetched successfully",
  "data": { "id": "...", "name": "..." }
}
```

---

## 🔐 Sample Users

| Role      | Email          | Password  | Banned |
|-----------|----------------|-----------|--------|
| Admin     | `mert@gmail.com` | 123456    |   -    |
| Customer  | `ali@gmail.com`  | 123456    |   No   |
| Customer  | `veli@gmail.com` | 123456    |   Yes  |

---

## Notes

- The solution is modular and follows Clean Architecture principles.
- All projects target **.NET 9**.
- For PDF generation, `libwkhtmltox.dll` is included and configured for use with DinkToPdf.
