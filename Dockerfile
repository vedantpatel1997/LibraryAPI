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

# Install SSH
RUN apt-get update \
    && apt-get install -y --no-install-recommends openssh-server \
    && echo "root:Docker!" | chpasswd \
    && mkdir /var/run/sshd

COPY sshd_config /etc/ssh/sshd_config
COPY entrypoint.sh ./
RUN chmod +x ./entrypoint.sh

EXPOSE 8000 2222 80 443

ENTRYPOINT [ "./entrypoint.sh" ]
