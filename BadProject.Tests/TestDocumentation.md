# Advertisement Service Test Documentation

## Test Coverage Overview

### 1. Provider Tests
#### NoSQL Provider (`NoSqlAdvertisementAdapterTests.cs`)
- ✓ Basic functionality with valid IDs
- ✓ Input validation for invalid IDs
- ✓ Retry behavior with configurable attempts
- ✓ Exponential backoff implementation
- ✓ Error handling and exception propagation

#### SQL Provider (`SqlAdvertisementAdapterTests.cs`)
- ✓ Basic functionality with valid IDs
- ✓ Input validation for invalid IDs
- ✓ Retry behavior with configurable attempts
- ✓ Logging verification
- ✓ Static provider wrapper behavior

### 2. Infrastructure Tests
#### Cache Implementation (`MemoryAdvertisementCacheTests.cs`)
- ✓ Cache hit/miss scenarios
- ✓ Cache expiration behavior
- ✓ Input validation
- ✓ Duration validation
- ✓ Null handling

#### Circuit Breaker (`RollingWindowCircuitBreakerTests.cs`)
- ✓ Error threshold handling
- ✓ Window-based error tracking
- ✓ State transitions (open/closed)
- ✓ Success/failure recording
- ✓ Thread safety validation

### 3. Configuration Tests (`ConfigurationLoaderTests.cs`)
- ✓ Default configuration loading
- ✓ Custom configuration values
- ✓ Invalid configuration handling
- ✓ Configuration validation rules
- ✓ Time-based settings validation

### 4. Integration Tests (`AdvertisementServiceTests.cs`)
- ✓ End-to-end advertisement retrieval
- ✓ Caching behavior
- ✓ Failover scenarios
- ✓ Circuit breaker integration
- ✓ Error propagation
- ✓ Logging verification

## Running the Tests

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.8
- xUnit Test Runner

### Running Tests in Visual Studio
1. Open the solution in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Open Test Explorer (Test > Test Explorer)
4. Click "Run All" to execute all tests

### Running Tests from Command Line
```bash
dotnet test BadProject.Tests/BadProject.Tests.csproj
```

## Test Dependencies and Setup

### Required NuGet Packages
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.1.0" />
<PackageReference Include="Moq" Version="4.18.1" />
<PackageReference Include="xUnit" Version="2.4.1" />
<PackageReference Include="xUnit.runner.visualstudio" Version="2.4.3" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="6.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="6.0.0" />
```

### Environment Setup
1. Install Visual Studio 2019 or later
2. Install .NET Framework 4.8 SDK
3. Install xUnit VS Test Runner extension
4. Restore NuGet packages:
   ```
   nuget restore BadProject.sln
   ```

### Test Project Configuration
- Target Framework: .NET Framework 4.8
- Test Framework: xUnit
- Mocking Framework: Moq
- Logging: Microsoft.Extensions.Logging
- Configuration: Microsoft.Extensions.Options

### CI/CD Considerations
- Set up proper package restore in build pipeline
- Configure test result output format
- Set minimum code coverage requirements
- Configure test parallelization settings

## Test Categories and Organization

### Unit Tests
- Located in respective component folders
- Focus on isolated component behavior
- Use mocking for dependencies
- Quick execution time

### Integration Tests
- Located in root test folder
- Test component interactions
- Verify system behavior
- May have longer execution time

## Mocking Strategy

### Primary Tools
- Moq framework for mocking
- TestFixture base class for common setup

### Mock Usage
- IAdvertisementProvider for both providers
- IAdvertisementCache for caching layer
- ICircuitBreaker for failure detection
- ILogger for logging verification

## Test Data Strategy

### Test Fixtures
- Common test data creation in TestFixture
- Consistent ID formatting
- Representative advertisement data

### Configuration
- Default values in App.config
- Test-specific overrides when needed
- Validation of configuration combinations

## Coverage Goals

### Code Coverage Targets
- ✓ 100% Provider Adapters
- ✓ 100% Infrastructure Components
- ✓ 100% Configuration Loading
- ✓ 90%+ Service Logic
- ✓ 90%+ Exception Handling

### Quality Metrics
- All public methods tested
- Edge cases covered
- Error conditions verified
- Thread safety validated
- Configuration variations tested

## Maintenance and Updates

### Adding New Tests
1. Follow existing pattern in relevant test class
2. Inherit from TestFixture for common functionality
3. Use descriptive test names (MethodName_Scenario_ExpectedResult)
4. Include both positive and negative test cases

### Test Patterns
- Arrange-Act-Assert pattern
- Descriptive test names
- Proper test isolation
- Efficient setup/teardown

## Common Test Scenarios

### 1. Basic Functionality
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var component = CreateComponent();
    
    // Act
    var result = await component.Method();
    
    // Assert
    Assert.NotNull(result);
}
```

### 2. Error Handling
```csharp
[Fact]
public async Task MethodName_ErrorCondition_ThrowsExpectedException()
{
    // Arrange
    var component = CreateComponent();
    
    // Act & Assert
    await Assert.ThrowsAsync<SpecificException>(
        () => component.Method());
}
```

## Troubleshooting

### Common Issues
1. Configuration not loading
   - Verify App.config is copied to output
   - Check configuration section names

2. Mock verification failures
   - Check exact parameter matching
   - Verify Times.Once vs Times.AtLeastOnce

3. Async test issues
   - Use async/await consistently
   - Avoid mixing sync and async calls

### Best Practices
1. Keep tests focused and simple
2. Use clear naming conventions
3. Maintain test independence
4. Clean up resources properly
5. Document unusual test scenarios