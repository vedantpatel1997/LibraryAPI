#FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["LibraryManagement.API.csproj", "."]
#RUN dotnet restore "./LibraryManagement.API.csproj"
#COPY . .
#WORKDIR "/src/."
#RUN dotnet build "./LibraryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./LibraryManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## Install SSH
#RUN apt-get update \
    #&& apt-get install -y --no-install-recommends openssh-server \
    #&& echo "root:Docker!" | chpasswd \
    #&& mkdir /var/run/sshd
#
#COPY sshd_config /etc/ssh/sshd_config
#COPY entrypoint.sh ./
#RUN chmod +x ./entrypoint.sh
#
#EXPOSE 8000 2222 80 443
#
#ENTRYPOINT [ "./entrypoint.sh" ]
#

#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble-chiseled-composite AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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

COPY entrypoint.sh ./

## Start and enable SSH
#RUN apt-get update \
    #&& apt-get install -y --no-install-recommends dialog \
    #&& apt-get install -y --no-install-recommends openssh-server \
    #&& echo "root:Docker!" | chpasswd \
    #&& chmod u+x ./entrypoint.sh
#COPY sshd_config /etc/ssh/

EXPOSE 80 443 

ENTRYPOINT [ "./entrypoint.sh" ]