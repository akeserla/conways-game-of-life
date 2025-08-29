# Game of Life API

A production-ready implementation of Conway's Game of Life API with persistence, comprehensive validation, and enterprise-grade architecture.

## 🏗️ Architecture Overview

The application follows clean architecture principles with clear separation of concerns:

```
GameOfLife.API/
├── Controllers/          # HTTP API endpoints
├── Services/            # Business logic and validation
├── Repositories/        # Data access layer
├── Data/               # Entity Framework context and models
└── Models/             # Data transfer objects and entities
```

## ✨ Key Features

### Production-Ready Features
- **Data Persistence**: Entity Framework Core with SQL Server support
- **Repository Pattern**: Clean data access abstraction
- **Comprehensive Validation**: Business logic validation with detailed error messages
- **Error Handling**: Structured error responses with request tracking
- **Logging**: Structured logging with correlation IDs
- **Configuration**: Environment-specific configuration (Development vs Production)
- **Database Indexing**: Optimized database schema with proper indexes
- **History Tracking**: Complete audit trail of board state changes

### Game of Life Features
- **Board Management**: Upload, retrieve, and manage board states
- **Generation Advancement**: Single or multiple generation progression
- **Final State Calculation**: Automatic detection of stable states
- **Pattern Validation**: Classic Conway patterns tested and validated

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (for production) or SQLite (for development)
- Visual Studio 2022 or JetBrains Rider

### Development Setup
1. Clone the repository
2. Navigate to `GameOfLife.API/`
3. Run the application:
   ```bash
   dotnet run
   ```
4. Access Swagger UI at `https://localhost:5001/swagger`

### Production Setup
1. Configure connection string in `appsettings.Production.json`
2. Set environment to Production
3. Run database migrations:
   ```bash
   dotnet ef database update
   ```

## 🗄️ Database Configuration

### Development
- Uses Entity Framework In-Memory database
- Automatically created on startup
- No external dependencies

### Production
- SQL Server with optimized schema
- Connection string configuration
- Database migrations support

### Database Schema
```sql
-- Board States
BoardStates
├── Id (PK, Guid)
├── GridData (nvarchar(max)) - Serialized grid
├── Rows (int)
├── Columns (int)
├── CreatedAt (datetime2)
├── Generation (int)
└── LastModifiedAt (datetime2)

-- Board History
BoardStateHistory
├── Id (PK, Guid)
├── BoardStateId (FK, Guid)
├── GridData (nvarchar(max))
├── Generation (int)
└── CreatedAt (datetime2)
```

## 📡 API Endpoints

### POST /api/GameOfLife/upload
Upload a new board state.

**Request:**
```json
{
  "rows": 3,
  "columns": 3,
  "grid": [[true, false, true], [false, true, false], [true, false, true]]
}
```

**Response:**
```json
{
  "boardId": "guid",
  "message": "Board uploaded successfully. Size: 3x3",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### GET /api/GameOfLife/{boardId}/next
Get the next generation state.

### GET /api/GameOfLife/{boardId}/advance/{generations}
Advance the board by N generations.

### GET /api/GameOfLife/{boardId}/final
Calculate the final stable state.

## 🔧 Configuration

### Development Environment
- In-memory database
- Detailed logging
- Swagger documentation enabled

### Production Environment
- SQL Server database
- Optimized logging levels
- Swagger disabled
- Connection string validation

### Environment Variables
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=your-connection-string
```

## 🧪 Testing

### Running Tests
```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific test class
dotnet test --filter "ClassName=GameOfLifeServiceTests"
```

### Test Coverage
- **Service Layer**: 100% method coverage
- **Controller Layer**: 100% method coverage
- **Validation**: Comprehensive input validation
- **Error Handling**: Exception scenarios covered
- **Business Logic**: Game of Life rules validated

## 🏭 Production Deployment

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GameOfLife.API/GameOfLife.API.csproj", "GameOfLife.API/"]
RUN dotnet restore "GameOfLife.API/GameOfLife.API.csproj"
COPY . .
WORKDIR "/src/GameOfLife.API"
RUN dotnet build "GameOfLife.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameOfLife.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameOfLife.API.dll"]
```

### Health Checks
The API includes built-in health checks for monitoring:
- Database connectivity
- Service availability
- Memory usage

### Monitoring
- Structured logging with correlation IDs
- Performance metrics
- Error tracking
- Request/response logging

## 🔒 Security Considerations

- Input validation and sanitization
- SQL injection prevention (Entity Framework)
- Request size limits
- Rate limiting support
- CORS configuration

## 📊 Performance

### Optimizations
- Database indexing on frequently queried fields
- Efficient grid serialization/deserialization
- Async/await throughout the stack
- Connection pooling
- Lazy loading for related entities

### Scalability
- Stateless service design
- Repository pattern for data access
- Dependency injection for loose coupling
- Configurable limits and timeouts

## 🚨 Error Handling

### Error Response Format
```json
{
  "error": "Error Type",
  "message": "Detailed error message",
  "timestamp": "2024-01-01T00:00:00Z",
  "requestId": "correlation-id"
}
```

### Error Types
- **400 Bad Request**: Validation errors
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Unexpected errors

## 📝 Logging

### Log Levels
- **Information**: Business operations
- **Warning**: Validation failures
- **Error**: Exceptions and failures
- **Debug**: Development details

### Structured Logging
```csharp
_logger.LogInformation("Board uploaded with ID: {BoardId}, Size: {Rows}x{Columns}", 
    boardId, rows, columns);
```

## 🔄 Data Migration

### Entity Framework Migrations
```bash
# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Remove migration
dotnet ef migrations remove
```

## 📚 Dependencies

### Core Packages
- **Microsoft.EntityFrameworkCore**: ORM framework
- **Microsoft.EntityFrameworkCore.SqlServer**: SQL Server provider
- **Microsoft.AspNetCore.OpenApi**: OpenAPI support
- **Swashbuckle.AspNetCore**: Swagger documentation

### Development Packages
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database
- **Microsoft.EntityFrameworkCore.Tools**: Migration tools

## 🤝 Contributing

1. Follow C# coding conventions
2. Add unit tests for new features
3. Update documentation
4. Ensure all tests pass
5. Follow the established architecture patterns

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For issues and questions:
1. Check the test suite for usage examples
2. Review the API documentation
3. Check the logs for detailed error information
4. Verify configuration settings

---

**Built with ❤️ using .NET 9.0 and Entity Framework Core**
