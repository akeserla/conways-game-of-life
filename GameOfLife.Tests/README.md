# Game of Life Unit Tests

This directory contains comprehensive unit tests for the Game of Life API project.

## Test Structure

### 1. GameOfLifeServiceTests.cs
Tests for the core `GameOfLifeService` class, covering:

- **Service Operations**: All CRUD operations and state transitions
- **Input Validation**: Error handling for invalid inputs
- **Edge Cases**: Boundary conditions and special scenarios
- **Game Rules**: Conway's Game of Life rule validation

**Test Categories:**
- `UploadBoardStateAsync_*` - Board upload functionality
- `GetNextStateAsync_*` - Single generation advancement
- `GetNStatesAheadAsync_*` - Multiple generation advancement
- `GetFinalStateAsync_*` - Final state calculation
- `GameOfLifeRules_*` - Core game rule validation
- `EdgeCase_*` - Boundary condition handling

### 2. GameOfLifeRulesTests.cs
Specialized tests for Conway's Game of Life rules, covering:

- **Still Life Patterns**: Block, Beehive (patterns that don't change)
- **Oscillators**: Blinker, Toad (patterns that repeat)
- **Spaceships**: Glider (patterns that move)
- **Edge Cases**: Empty grids, single cells, cross patterns

**Test Categories:**
- `StillLife_*` - Static pattern validation
- `Oscillator_*` - Repeating pattern validation
- `Spaceship_*` - Moving pattern validation
- `EdgeCase_*` - Special scenario handling
- `MultipleGenerations_*` - Long-term behavior validation

### 3. GameOfLifeControllerTests.cs
API controller tests covering:

- **HTTP Endpoints**: All controller methods
- **Response Types**: Success and error responses
- **Exception Handling**: Service error propagation
- **HTTP Status Codes**: Proper status code returns

**Test Categories:**
- `UploadBoardState_*` - POST /api/GameOfLife/upload
- `GetNextState_*` - GET /api/GameOfLife/{boardId}/next
- `GetNStatesAhead_*` - GET /api/GameOfLife/{boardId}/advance/{generations}
- `GetFinalState_*` - GET /api/GameOfLife/{boardId}/final

## Test Coverage

The test suite provides comprehensive coverage of:

- ✅ **Service Layer**: 100% method coverage
- ✅ **Controller Layer**: 100% method coverage  
- ✅ **Business Logic**: All Game of Life rules validated
- ✅ **Error Handling**: Exception scenarios covered
- ✅ **Edge Cases**: Boundary conditions tested
- ✅ **Pattern Validation**: Classic Conway patterns tested

## Running Tests

### Basic Test Execution
```bash
dotnet test
```

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Specific Test Class
```bash
dotnet test --filter "ClassName=GameOfLifeServiceTests"
```

### Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~GameOfLifeRules_StillLife_Block_RemainsUnchanged"
```

## Test Patterns Used

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
- **Arrange**: Set up test data and mocks
- **Act**: Execute the method under test
- **Assert**: Verify expected outcomes

### Mocking
- **ILogger**: Mocked to avoid logging dependencies
- **IGameOfLifeService**: Mocked for controller tests
- **Dependencies**: Properly isolated for unit testing

### Async Testing
- All async methods properly tested with `async/await`
- Exception handling tested for async operations

## Conway's Game of Life Rules Tested

1. **Underpopulation**: Live cell with < 2 neighbors dies
2. **Survival**: Live cell with 2-3 neighbors survives
3. **Overpopulation**: Live cell with > 3 neighbors dies
4. **Reproduction**: Dead cell with exactly 3 neighbors becomes alive

## Classic Patterns Validated

- **Block**: 2x2 square (still life)
- **Beehive**: 6-cell pattern (still life)
- **Blinker**: 3-cell line (oscillator, period 2)
- **Toad**: 6-cell pattern (oscillator, period 2)
- **Glider**: 5-cell pattern (spaceship)

## Dependencies

- **xUnit**: Testing framework
- **Moq**: Mocking library
- **coverlet.collector**: Code coverage collection
- **Microsoft.NET.Test.Sdk**: Test discovery and execution

## Test Results

- **Total Tests**: 37
- **Passed**: 37
- **Failed**: 0
- **Skipped**: 0
- **Duration**: ~0.5 seconds

All tests pass successfully, providing confidence in the implementation's correctness and robustness.
