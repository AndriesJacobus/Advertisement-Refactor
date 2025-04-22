# Advertisement Service Refactoring Project

## Project Overview
This project demonstrates the refactoring of a C# Advertisement Service that integrates with third-party advertisement providers. The original implementation has been improved with proper architectural patterns, error handling, testability, and thread safety.

## Project Structure
```
├── BadProject/
│   ├── AdvertisementService.cs     # Main service implementation
│   ├── Program.cs                  # Console application entry point
│   ├── App.config                  # Application configuration
│   ├── Core/                       # Core interfaces
│   ├── Infrastructure/             # Infrastructure components
│   ├── Providers/                  # Provider adapters
│   └── Configuration/              # Configuration classes
├── ThirdParty/                     # Third-party components (cannot be modified)
├── BadProject.Tests/               # Test project
```

## Current Functionality
The service provides advertisements through a failover system:
1. Attempts to retrieve from cache
2. If not in cache, uses NoSQL provider with configurable retries
3. Falls back to SQL provider if NoSQL fails or error threshold is exceeded
4. Implements circuit breaker pattern to detect and handle persistent failures

## Running the Application
The project can be run as a console application:

```bash
# Run the application
dotnet run --project BadProject/BadProject.csproj
```

This will start an interactive console where you can enter advertisement IDs to retrieve.

## Running Tests
The project includes comprehensive tests that can be run using:

```bash
# Run all tests
dotnet test

# Run a specific test class
dotnet test --filter "FullyQualifiedName~BadProject.Tests.AdvertisementServiceTests"

# Run manual test (update test file to remove [Skip] attribute first)
dotnet test --filter "FullyQualifiedName=BadProject.Tests.ManualTestAdvertisementService.RunAdvertisementService"
```

## Design Patterns Used
- **Adapter Pattern**: Wraps third-party providers
- **Factory Pattern**: Provider instantiation
- **Circuit Breaker**: Failure handling
- **Repository**: Caching implementation
- **Options Pattern**: Configuration management
- **Dependency Injection**: Component wiring

## Technical Implementation
### Thread Safety
The service is fully thread-safe through:
- Thread-safe MemoryCache with synchronized compound operations
- ConcurrentQueue for error tracking in circuit breaker
- Immutable configuration through Options pattern
- Stateless provider adapters
- Fine-grained synchronization with SemaphoreSlim
- Proper async/await patterns throughout

### Error Handling
- Structured exception handling with custom exception types
- Exponential backoff retry mechanism
- Circuit breaker for failover scenarios
- Comprehensive logging at all layers
- Proper error propagation

### Performance Considerations
- Minimal lock contention through fine-grained synchronization
- Efficient caching with configurable durations
- Async/await for scalable I/O operations
- Smart provider failover strategy
- Optimized error tracking with rolling window

## Configuration
The following settings can be configured in App.config:
- `NoSql.RetryCount`: Number of retry attempts for NoSQL provider
- `NoSql.RetryDelaySeconds`: Base delay between retries
- `NoSql.MaxRetryDelaySeconds`: Maximum retry delay
- `Sql.RetryCount`: Number of retry attempts for SQL provider
- `Sql.RetryDelaySeconds`: Base delay between retries
- `Sql.MaxRetryDelaySeconds`: Maximum retry delay
- `Cache.DurationMinutes`: How long to cache advertisements
- `CircuitBreaker.WindowHours`: Time window for error tracking
- `CircuitBreaker.ErrorThreshold`: Number of errors before failover
- `CircuitBreaker.MaxErrorCount`: Maximum number of errors to track

## Implemented Improvements
1. Applied SOLID principles with proper interface segregation
2. Implemented dependency injection for all components
3. Added robust error handling and resilience
4. Enhanced testability with proper abstractions
5. Implemented comprehensive logging
6. Added circuit breaker pattern for graceful degradation

## Future Improvements
- Add metrics collection
- Implement distributed caching
- Add health checks
- Support for async operations
- Add response compression
- Implement rate limiting
- Add cache warming strategy
- Implement A/B testing support
- Add OpenTelemetry integration
- Support for content versioning

---
Made with 💚 by Andries Jacobus