# Tests (examples)

This folder contains example/unit test sketches you can enable locally.

How to use
1. In Visual Studio, create a new Test project (xUnit or MSTest) targeting .NET 10.
2. Add the necessary NuGet packages (Microsoft.EntityFrameworkCore.InMemory, xUnit or MSTest, Moq if needed).
3. Copy the example `.example.cs` files into the test project and rename to `.cs`.
4. Adjust namespaces and references (ProjectReference to the API project) and run the tests.

Files
- AuthServiceTests.example.cs - Example tests for AuthService (register/login) using InMemory EF and a simple ISession.
- EventsControllerTests.example.cs - Sketch for controller-level tests using TestServer or controller unit tests.

These files are intentionally examples and are not included in the solution to avoid build issues. Enable them locally when ready.
