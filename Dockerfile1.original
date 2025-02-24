FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LibraryManagement.API.csproj", "."]
RUN dotnet restore "./LibraryManagement.API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./LibraryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LibraryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

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

# Expose ports
EXPOSE 443