# --- Step 1: Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set working directory inside container
WORKDIR /src

# Copy .csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./
RUN dotnet publish -c Release -o /app/publish


# --- Step 2: Runtime Stage ---
FROM mcr.microsoft.com/dotnet/runtime:7.0

# Set working directory
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "ConsoleApp2.dll"]
