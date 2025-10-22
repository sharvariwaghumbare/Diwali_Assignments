# 🧩 Product & Category Management API (Spring Boot)

## 📖 Description
A RESTful API built with **Spring Boot**, designed to manage **Products** and their **Categories**.  
Supports full CRUD operations, relationships between tables, and robust exception handling —  
using **DTOs**, **services**, and a **global exception handler** for cleaner architecture.

Follows Java best practices and layered structure:  
**Controller → Service → Repository → Entity → Exception Handling**

---

## ⚙️ Features
- 🧱 Manage **Products** and **Categories**
- ➕ Create new records (`POST`)
- 🔍 Find by name or ID (`GET`)
- 🗑️ Delete products by ID
- 🔄 Update entities (`PUT`)
- 🔗 Relationship between **Product** and **Category** (`@ManyToOne` / `@OneToMany`)
- 💾 Uses **Spring Data JPA** with **H2 database**
- ⚠️ Custom error handling via `GlobalExceptionHandler`
- 🧩 Clean code using **Lombok** annotations

---

## 🧱 Technologies
- **Java 17+**
- **Spring Boot 3**
- **Spring Data JPA**
- **H2 Database**
- **Lombok**
- **Maven**

---

## 🧩 Entities

### 🏷️ Category
| Field | Type | Description |
|--------|-------|-------------|
| `id` | Integer | Auto-generated unique ID |
| `name` | String | Category name (unique) |
| `products` | List<Product> | List of products linked to the category |

---

### 📦 Product
| Field | Type | Description |
|--------|-------|-------------|
| `id` | Integer | Auto-generated unique ID |
| `name` | String | Product name |
| `price` | Double | Product price |
| `category` | Category | Category that the product belongs to |

---

## 🚀 Endpoints

### 🧱 Category Controller `/categoria`

| Method | Endpoint | Description | Request Body | Response |
|--------|-----------|--------------|---------------|-----------|
| **POST** | `/categoria` | Create new category | `{ "name": "gaming" }` | Created category |
| **GET** | `/categoria/search?name=Eletronicos` | Find category by name | - | Returns category |
| **GET** | `/categoria` | List all categories | - | Returns all categories |
| **PUT** | `/categoria?id=1` | Update category name | `{ "name": "Pc" }` | Updated category |

---

### 📦 Product Controller `/produto`

| Method | Endpoint | Description | Request Body | Response |
|--------|-----------|--------------|---------------|-----------|
| **POST** | `/produto` | Create new product | `{ "name": "TV", "price": 999.99, "categoryId": 1 }` | Created product |
| **GET** | `/produto?id=1` | Find product by ID | - | Returns product |
| **PUT** | `/produto?id=1` | Update product | `{ "name": "Smart TV", "price": 1299.99 }` | Updated product |
| **DELETE** | `/produto?id=1` | Delete product | - | Returns deleted product info |

---

## 🧠 Exception Handling

All exceptions are handled globally in `GlobalExceptionHandler`.

| Exception | HTTP Status | Example Message |
|------------|--------------|------------------|
| `NotFoundException` | 404 Not Found | `"Product not found: id 1"` |
| `ConflictException` | 409 Conflict | `"Category already exists: gaming"` |
| `CategoryNotFoundException` | 404 Not Found | `"Category not found with ID 5"` |
| `Generic Exception` | 500 Internal Server Error | `"Internal Server Error ----GLOBAL"` |

### Example Response
```json
{
  "error": "Category not found. ----GLOBAL",
  "message": "Category with ID 9 not found",
  "timestamp": "2025-10-19T12:21:09.371",
  "status": 404
}
```
## 📂 Project Structure
```text
src/main/java/com/pdo/product_category
│
├── controller/
│   ├── ProductController.java
│   └── CategoryController.java
│
├── DTO/
│   ├── ProductRequestDTO.java
│   ├── ProductResponseDTO.java
│   ├── CategoryRequestDTO.java
│   └── CategoryResponseDTO.java
│
├── entity/
│   ├── Product.java
│   └── Category.java
│
├── exception/
│   ├── ErrorResponse.java
│   ├── GlobalExceptionHandler.java
│   ├── CategoryNotFoundException.java
│   ├── ConflictException.java
│   ├── NotFoundException.java
│   └── GenericException.java
│
├── repository/
│   ├── ProductRepository.java
│   └── CategoryRepository.java
│
├── service/
│   ├── ProductService.java
│   └── CategoryService.java
│
└── ProductCategoryApplication.java
```
##🧪 Example Request (POST Category)
```json
Request
{
    "name": "gaming"
}
Response
{
    "id": 1,
    "name": "gaming",
    "products": null
}
```
##🧪 Example Request (POST Product)
```json
Request
{
  "name": "rtx 4070",
  "price": 899.90,
  "categoryId": 1
}
Response
{
  "id": 1,
  "name": "Monitor",
  "price": 899.90,
  "categoryName": "gaming"
}
```
## 🧰 How to Run
1. Clone the repository:
```bash
https://github.com/Pedrolso/spring-product-category-api.git
```
3. Open in your IDE (IntelliJ, Eclipse, or VS Code)
4. Run the project
5. Uses H2 in-memory database by default — no configuration needed.
6. Test endpoints using Postman or cURL.

## 🧠 Notes

- Products and categories are automatically linked via @ManyToOne and @OneToMany.
- All responses use DTOs to prevent exposing internal entities.
- Validation and exception handling are fully centralized.
- The architecture is clean and scalable for future expansion.
