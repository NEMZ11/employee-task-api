FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY EmployeeTaskApi.sln ./
COPY src/EmployeeTaskApi.Api/EmployeeTaskApi.Api.csproj src/EmployeeTaskApi.Api/
COPY tests/EmployeeTaskApi.Tests/EmployeeTaskApi.Tests.csproj tests/EmployeeTaskApi.Tests/
RUN dotnet restore

COPY . .
RUN dotnet publish src/EmployeeTaskApi.Api/EmployeeTaskApi.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EmployeeTaskApi.Api.dll"]

