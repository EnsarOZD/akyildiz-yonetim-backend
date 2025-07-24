# Akyildiz Yonetim Backend

A .NET 8.0 Web API for property management system with JWT authentication, Entity Framework Core, and Clean Architecture.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (or compatible database)
- Docker (optional)

### Local Development
```bash
# Clone the repository
git clone <repository-url>
cd akyildiz-yonetim-backend

# Restore and build
dotnet restore
dotnet build

# Run the API
dotnet run --project AkyildizYonetim.API
```

### Using Build Scripts
```bash
# Windows (Batch)
build.bat --test

# PowerShell (if execution policy allows)
.\build.ps1 -Test
```

## 🐳 Docker Deployment

### Local Docker Build
```bash
# Build the image
docker build -t akyildiz-yonetim-backend .

# Run the container
docker run -p 8080:8080 akyildiz-yonetim-backend
```

## 🔧 BuildKit Issue Resolution

If you encounter the BuildKit daemon connection error:
```
could not connect to unix:///run/user/1000/buildkit/buildkitd.sock
```

### Solution 1: Disable BuildKit (Recommended)
```bash
export DOCKER_BUILDKIT=0
docker build -t akyildiz-yonetim-backend .
```

### Solution 2: Use Legacy Builder
```bash
docker buildx use default
docker build -t akyildiz-yonetim-backend .
```

### Solution 3: Use --no-buildkit Flag
```bash
docker build --no-buildkit -t akyildiz-yonetim-backend .
```

## 📁 Project Structure

```
AkyildizYonetim/
├── AkyildizYonetim.API/          # Web API Controllers
├── AkyildizYonetim.Application/  # Business Logic & Commands/Queries
├── AkyildizYonetim.Domain/       # Entities & Domain Models
├── AkyildizYonetim.Infrastructure/ # Data Access & External Services
├── AkyildizYonetim.Persistence/  # Database Context & Migrations
└── AkyildizYonetim.Tests/        # Unit & Integration Tests
```

## 🔐 Authentication

The API uses JWT authentication with the following endpoints:
- `POST /auth/login` - User login
- `POST /auth/register` - User registration
- `POST /auth/refresh` - Refresh token

## 📊 API Endpoints

### Tenants
- `GET /tenants` - Get all tenants
- `POST /tenants` - Create new tenant
- `PUT /tenants/{id}` - Update tenant
- `DELETE /tenants/{id}` - Delete tenant
- `GET /tenants/available-flats` - Get available flats

### Owners
- `GET /owners` - Get all owners
- `POST /owners` - Create new owner
- `PUT /owners/{id}` - Update owner
- `DELETE /owners/{id}` - Delete owner

### Payments
- `GET /payments` - Get all payments
- `POST /payments` - Create new payment
- `PUT /payments/{id}` - Update payment
- `DELETE /payments/{id}` - Delete payment

### Reports
- `GET /reports/financial` - Get financial reports

## 🗄️ Database

The application uses Entity Framework Core with SQL Server. Database migrations are automatically applied on startup in production.

### Connection String Format
```
Server=<server>;Database=AkyildizYonetimDb;User Id=<username>;Password=<password>;TrustServerCertificate=true;Encrypt=false;
```

## 🔧 Configuration

### Environment Variables
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<your-connection-string>"
  },
  "JwtSettings": {
    "SecretKey": "<your-secret-key>",
    "Issuer": "<your-issuer>",
    "Audience": "<your-audience>"
  }
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📚 Documentation

- [Deployment Guide](DEPLOYMENT.md) - Detailed deployment instructions
- [API Documentation](https://localhost:7001/swagger) - Swagger UI (when running)

## 🚨 Troubleshooting

### Common Issues

1. **BuildKit Connection Error**
   - Use `DOCKER_BUILDKIT=0` environment variable
   - Or use `--no-buildkit` flag with docker build

2. **Database Connection Issues**
   - Verify connection string format
   - Check firewall settings
   - Ensure database server is accessible

3. **JWT Configuration Errors**
   - Verify all JWT settings are provided
   - Ensure secret key is at least 16 characters

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For issues and questions:
1. Check the [troubleshooting section](#-troubleshooting)
2. Review the [deployment guide](DEPLOYMENT.md)
3. Create an issue in the repository 