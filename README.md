# Smart .NET Web API

A comprehensive, production-ready .NET 8 Web API structure with authentication, caching, logging, and database operations using modern best practices including Repository Pattern and Unit of Work.

## Features

- **JWT Authentication** with access and refresh tokens
- **Request/Response Logging** with Serilog
- **Redis Caching** integration for high performance
- **Entity Framework Core** with SQL Server
- **Repository Pattern** with Unit of Work for data access abstraction
- **Dependency Injection** architecture
- **Global Exception Handling** middleware
- **Swagger Documentation** with authentication
- **Health Checks** for monitoring
- **FluentValidation** for request validation
- **AutoMapper** for object mapping
- **Background Services** for maintenance tasks

## Architecture

### Clean Architecture Layers
- **Controllers**: API endpoints and request handling
- **Services**: Business logic and orchestration
- **Data**: Repository pattern, Unit of Work, and Entity Framework DbContext
- **Models**: DTOs and domain entities

### Key Components

#### Data Access Layer
- Repository pattern for data abstraction
- Unit of Work for transaction management
- Generic repository with specialized implementations
- No direct DbContext exposure in services

#### Authentication & Security
- JWT token generation and validation
- Refresh token rotation
- Identity management with ASP.NET Core Identity
- Secure password hashing

#### Caching Strategy
- Redis integration for distributed caching
- User data caching with configurable expiration
- Cache invalidation on data updates

#### Logging & Monitoring
- Structured logging with Serilog
- Request/response logging middleware
- Database logging sink
- Health checks for dependencies

#### Error Handling
- Global exception handling middleware
- Structured error responses
- Proper HTTP status codes

#### Background Services
- Automatic cleanup of expired tokens
- Configurable maintenance intervals
- Proper error handling and logging

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Redis server

### Setup

1. **Update Connection Strings**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=SmartWebApiDb;Trusted_Connection=true;",
       "Redis": "your_redis_connection_string"
     }
   }
   ```

2. **Run Database Migration**
   ```bash
   dotnet ef database update
   ```

3. **Start the Application**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI**
   Navigate to `https://localhost:5001` (or the configured port)

### API Endpoints
password = "Admin123!" 
passwordhash: AQAAAAIAAYagAAAAEODh5LzN5LP5udWD+gGrjbR+rVgJIcilvMhUIZ5/dgL31Xorgjx2kPlO4Cz+2PhFmw==
#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/revoke` - Revoke refresh token
- `POST /api/auth/revoke-all` - Revoke all user tokens
- `GET /api/auth/me` - Get current user info

#### Users
- `GET /api/users/profile` - Get current user profile
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users/deactivate` - Deactivate account
- `POST /api/users/{id}/activate` - Activate account

#### Health Checks
- `GET /health` - Application health status

### Configuration

#### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "SmartWebApi",
    "Audience": "SmartWebApiUsers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

#### Cache Settings
```json
{
  "CacheSettings": {
    "DefaultExpirationMinutes": 30,
    "UserCacheExpirationMinutes": 60
  }
}
```

## Best Practices Implemented

### Security
- Strong password requirements
- JWT token expiration
- Refresh token rotation
- SQL injection prevention with EF Core
- Input validation with FluentValidation

### Performance
- Redis caching for frequently accessed data
- Async/await patterns throughout
- Connection pooling with EF Core
- Response compression

### Maintainability
- Clean Architecture separation
- Dependency injection
- Repository pattern abstraction
- Comprehensive logging
- Unit test friendly design

### Monitoring
- Health checks for dependencies
- Structured logging with correlation IDs
- Request/response tracking
- Performance metrics

## Development

### Adding New Features
1. Create DTOs in `Models/DTOs`
2. Add repository interfaces in `Data/Repositories/Interfaces`
3. Implement repositories in `Data/Repositories`
4. Update Unit of Work if needed
5. Add service interfaces in `Services/Interfaces`
6. Implement services in `Services`
7. Create controllers in `Controllers`
8. Add validation in `Validators`

### Testing
The architecture supports easy unit testing with:
- Dependency injection for mockable services
- Clear separation of concerns
- Repository pattern for data access
- Unit of Work for transaction testing

## Production Considerations

### Security Checklist
- [ ] Use HTTPS in production
- [ ] Implement rate limiting
- [ ] Add API versioning
- [ ] Set up CORS properly
- [ ] Use secure JWT secret keys
- [ ] Implement proper user roles/permissions

### Performance Optimizations
- [ ] Configure connection pooling
- [ ] Set up Redis clustering
- [ ] Implement database indexing
- [ ] Add response compression
- [ ] Configure caching policies
- [ ] Optimize repository queries
- [ ] Implement query result caching

### Monitoring & Logging
- [ ] Set up centralized logging (ELK stack)
- [ ] Configure application insights
- [ ] Set up alerts for health checks
- [ ] Monitor performance metrics
- [ ] Track repository performance
- [ ] Monitor background service health

This structure provides a solid foundation for building scalable, maintainable .NET Web APIs with enterprise-level features, proper data access abstraction, and modern architectural patterns.