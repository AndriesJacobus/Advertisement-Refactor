# Advertisement Service Refactoring Project

## Project Overview
This project demonstrates the refactoring of a C# Advertisement Service that integrates with third-party advertisement providers. The original implementation, while functional, contains several architectural and design issues that need improvement.

## Project Structure
```
├── BadProject/
│   ├── AdvertisementService.cs    # Main service implementation
│   └── App.config                 # Application configuration
├── ThirdParty/                    # Third-party components (cannot be modified)
│   ├── Advertisement.cs           # Advertisement model
│   ├── NoSqlAdvProvider.cs       # Primary provider
│   └── SQLAdvProvider.cs         # Backup provider
```

## Current Functionality
The service provides advertisements through a failover system:
1. Attempts to retrieve from cache
2. If not in cache, uses NoSQL provider with configurable retries
3. Falls back to SQL provider if NoSQL fails or error threshold is exceeded

## Constraints
- ThirdParty project code cannot be modified
- Must maintain the public interface: `public Advertisement GetAdvertisement(string id)`
- Must support both providers (NoSQL and SQL)

## Improvement Goals
1. Apply SOLID principles
2. Implement proper dependency injection
3. Improve error handling and resilience
4. Enhance testability
5. Implement proper configuration management
6. Add logging and telemetry

## Design Patterns Used
- **Adapter Pattern**: Wrap third-party providers
- **Factory Pattern**: Provider instantiation
- **Circuit Breaker**: Failure handling
- **Decorator**: Caching implementation
- **Options Pattern**: Configuration management

## Getting Started
1. Open the solution in Visual Studio
2. Restore NuGet packages
3. Build the solution
4. Run tests (once implemented)

## Configuration
The following settings can be configured in App.config:
- RetryCount: Number of retry attempts for NoSQL provider
- CacheDuration: How long to cache advertisements
- CircuitBreakerWindow: Time window for error tracking
- ErrorThreshold: Number of errors before failover

## Testing
The refactored solution will include:
- Unit tests for all components
- Integration tests for provider interactions
- Circuit breaker tests
- Cache behavior tests

## Future Improvements
- Add metrics collection
- Implement distributed caching
- Add health checks
- Support for async operations

---

Made with 💚 by Andries Jacobus