# --- Step 1: Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy rest of the code and publish
COPY . ./
RUN dotnet publish -c Release -o out

# --- Step 2: Runtime Stage ---
FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "ConsoleApp2.dll"]
