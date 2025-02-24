# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LibraryManagement.API.csproj", "."]
RUN dotnet restore "./LibraryManagement.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./LibraryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LibraryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Start and enable SSH
RUN apt-get update \
    && apt-get install -y --no-install-recommends dialog \
    && apt-get install -y --no-install-recommends openssh-server \
    && echo "root:Docker!" | chpasswd \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir /var/run/sshd \
    && chmod 0755 /var/run/sshd

# Copy the SSH configuration
COPY sshd_config /etc/ssh/


# Start SSH and the application
CMD service ssh start && exec dotnet LibraryManagement.API.dll

EXPOSE 8080 2222


#az login
#az acr login --name libraryacr
#docker build -t libraryacr.azurecr.io/library:frontend .
#docker build -t libraryacr.azurecr.io/library:backend .
#docker push libraryacr.azurecr.io/library:frontend
#docker push libraryacr.azurecr.io/library:backend