# Base image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LibraryManagement.API.csproj", "."]
RUN dotnet restore "./LibraryManagement.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./LibraryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LibraryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install and configure SSH
RUN apt-get update && \
    apt-get install -y --no-install-recommends openssh-server && \
    echo "root:Docker!" | chpasswd && \
    mkdir /var/run/sshd && \
    sed -i 's/^PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config && \
    sed -i 's/^PasswordAuthentication no/PasswordAuthentication yes/' /etc/ssh/sshd_config

# Copy init script
COPY init_container.sh /app/init_container.sh
RUN chmod +x /app/init_container.sh

ENTRYPOINT ["/app/init_container.sh"]
