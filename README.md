## StayHealthy API
This API allows you to check available slots from Slots API client for a specific week, and book a slot for a specific date and time. You won't be able to book a slot which is blocked.
- - -

### Installation
- Clone the repository

### Usage
To run the API, you need to run commands from the terminal:
```powershell
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the project
dotnet run --project StayHealthy.Api/StayHealthy.Api.csproj
~~~~
# Run the tests
dotnet test
```
The API will be available at [http://localhost:5103/swagger/index.html](http://localhost:5103/swagger/index.html)
You can use swagger to send requests. There are 2 endpoints present:
- GET /api/availability?date=2024-08-01

- POST /api/appointment
```powershell
  '{
  "facilityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "start": "2024-08-01T10:31:28.380Z",
  "end": "2024-08-01T10:31:28.380Z",
  "comments": "test",
  "patient": {
  "name": "test",
  "secondName": "test",
  "email": "test@gmail.com",
  "phone": "+48567345132"
  }
  }'
```