# UrlShortener Project Repository

**UrlShortener** is a lightweight web application that allows users to shorten long URLs into more manageable and shareable links. It uses a custom algorithm to generate unique short links and stores them in a database for easy retrieval.

---

## 🚀 Technologies Used

This project utilizes the following technologies and libraries:

- **.NET 9** – Core framework for building the application.
- **ASP.NET Core Identity** – Handles user authentication and authorization.
- **Entity Framework Core** – ORM for database management and migrations.
- **HybridCache** – Improves performance with a combination of in-memory and distributed caching.
- **Polly** – Provides resilience and fault-handling strategies (e.g., retries, fallbacks).
- **Redis** – Used as a distributed caching solution for faster response times.
- **SQL Server** – Primary database management system for persistent storage.

---

## 🛠 Setup Instructions

### 1. Prerequisites

Before running the project locally, ensure you have the following installed:

- **[.NET SDK v9](https://dotnet.microsoft.com/download/dotnet/9.0)**  
- **[Git](https://git-scm.com/downloads)**  
- **SQL Server** (if using a database)  
- **Redis** (if using caching)  

---

### 2. Clone the Repository

Run the following commands:

```bash
# Clone the repository and navigate into the project folder
git clone https://github.com/NazariySh/UrlShortener.git
cd UrlShortener
```

### 3. Restore the Packages

Restore the necessary NuGet packages by running:

```bash
# From the solution folder (UrlShortener)
dotnet restore
```

### 4. Set Up the Database and Redis (If Needed)

Ensure your connection string is correctly set in the `appsettings.json` file. For example, for SQL Server and Redis, you can configure it like this:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=ShortenedUrlDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "UrlShortener_"
  }
}
```

### 5. Run the Application

Start the application using:

```bash
# From the solution folder (UrlShortener)
dotnet run
```

The application will be available at:
➡️ http://localhost:5000 (HTTP)
➡️ https://localhost:5001 (HTTPS, if configured)