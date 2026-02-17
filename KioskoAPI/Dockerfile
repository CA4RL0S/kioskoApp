# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build an optimized release
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port your app runs on
EXPOSE 8080

# Set the entry point for the container
ENTRYPOINT ["dotnet", "KioskoAPI.dll"]
