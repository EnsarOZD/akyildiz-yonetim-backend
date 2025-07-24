# Akyildiz Yonetim Backend - Deployment Guide

## Overview
This guide provides instructions for deploying the Akyildiz Yonetim Backend API to various platforms and troubleshooting common deployment issues.

## Prerequisites
- .NET 8.0 SDK
- Docker (optional, for containerized deployment)
- SQL Server or compatible database

## Local Development

### Quick Start
```bash
# Clone the repository
git clone <repository-url>
cd akyildiz-yonetim-backend

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the API
dotnet run --project AkyildizYonetim.API
```

### Using the Build Script
```powershell
# Build with tests
.\build.ps1 -Test

# Build with Docker image
.\build.ps1 -Docker

# Clean build
.\build.ps1 -Clean -Test
```

## Docker Deployment

### Local Docker Build
```bash
# Build the image
docker build -t akyildiz-yonetim-backend .

# Run the container
docker run -p 8080:8080 akyildiz-yonetim-backend
```

### Troubleshooting BuildKit Issues

The BuildKit daemon connection error (`could not connect to unix:///run/user/1000/buildkit/buildkitd.sock`) is a common issue in CI/CD environments. Here are several solutions:

#### Solution 1: Disable BuildKit (Recommended for CI/CD)
```bash
# Set environment variable to disable BuildKit
export DOCKER_BUILDKIT=0

# Or use the --no-buildkit flag
docker build --no-buildkit -t akyildiz-yonetim-backend .
```

#### Solution 2: Use Legacy Builder
```bash
# Use the legacy builder instead of BuildKit
docker buildx use default
docker build -t akyildiz-yonetim-backend .
```

#### Solution 3: Configure BuildKit Properly
```bash
# Create buildkit daemon configuration
mkdir -p /etc/buildkit
cat > /etc/buildkit/buildkitd.toml << EOF
[registry."docker.io"]
  mirrors = ["mirror.gcr.io"]
EOF

# Start buildkit daemon
buildkitd --config /etc/buildkit/buildkitd.toml &
```

#### Solution 4: Use Multi-Platform Build (if needed)
```bash
# Create and use a new builder instance
docker buildx create --name mybuilder --use

# Build for multiple platforms
docker buildx build --platform linux/amd64,linux/arm64 -t akyildiz-yonetim-backend .
```

## Cloud Deployment

### Render.com Deployment

1. **Connect your repository** to Render
2. **Create a new Web Service**
3. **Configure the service:**
   - **Build Command:** `dotnet publish AkyildizYonetim.API/AkyildizYonetim.API.csproj -c Release -o out`
   - **Start Command:** `dotnet out/AkyildizYonetim.API.dll`
   - **Environment:** .NET 8.0

4. **Environment Variables:**
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ConnectionStrings__DefaultConnection=<your-database-connection-string>
   JwtSettings__SecretKey=<your-jwt-secret>
   JwtSettings__Issuer=<your-jwt-issuer>
   JwtSettings__Audience=<your-jwt-audience>
   ```

### Railway.app Deployment

1. **Connect your repository** to Railway
2. **Add a new service** from GitHub
3. **Configure environment variables** as above
4. **Railway will automatically detect** the .NET project and build it

### Heroku Deployment

1. **Install Heroku CLI**
2. **Create a `heroku.yml` file:**
   ```yaml
   build:
     docker:
       web: Dockerfile
   ```

3. **Deploy:**
   ```bash
   heroku create your-app-name
   heroku stack:set container
   git push heroku main
   ```

## Database Setup

### SQL Server (Recommended)
```sql
-- Create database
CREATE DATABASE AkyildizYonetimDb;
GO

-- Use the database
USE AkyildizYonetimDb;
GO
```

### Connection String Format
```
Server=<server>;Database=AkyildizYonetimDb;User Id=<username>;Password=<password>;TrustServerCertificate=true;Encrypt=false;
```

## Environment Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<your-connection-string>"
  },
  "JwtSettings": {
    "SecretKey": "<your-secret-key>",
    "Issuer": "<your-issuer>",
    "Audience": "<your-audience>"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Health Checks

The API includes health check endpoints:

- **Health Check:** `GET /health`
- **Database Health:** `GET /health/db`
- **Overall Health:** `GET /health/ready`

## Monitoring and Logging

### Application Insights (Optional)
Add to `appsettings.json`:
```json
{
  "ApplicationInsights": {
    "ConnectionString": "<your-connection-string>"
  }
}
```

### Structured Logging
The application uses structured logging with Serilog. Logs include:
- Request/response information
- Database operations
- Authentication events
- Business logic operations

## Troubleshooting

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
   - Check issuer and audience values

4. **CORS Issues**
   - Verify CORS configuration in `Program.cs`
   - Check allowed origins in production

### Performance Optimization

1. **Database Indexing**
   - Ensure proper indexes on frequently queried columns
   - Monitor query performance

2. **Caching**
   - Consider adding Redis for caching
   - Implement response caching where appropriate

3. **Connection Pooling**
   - Configure appropriate connection pool size
   - Monitor connection usage

## Security Considerations

1. **Environment Variables**
   - Never commit secrets to source control
   - Use secure environment variable management

2. **HTTPS**
   - Always use HTTPS in production
   - Configure proper SSL certificates

3. **Authentication**
   - Use strong JWT secrets
   - Implement proper token expiration
   - Consider refresh token rotation

4. **Input Validation**
   - All inputs are validated using FluentValidation
   - SQL injection protection via Entity Framework

## Support

For deployment issues:
1. Check the logs in your deployment platform
2. Verify all environment variables are set correctly
3. Test locally before deploying
4. Review the troubleshooting section above

For code-related issues:
1. Check the build output for compilation errors
2. Review the application logs
3. Test individual components locally 