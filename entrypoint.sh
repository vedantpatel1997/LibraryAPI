#!/bin/sh
set -e

# Ensure environment variables are available
eval $(printenv | sed -n "s/^\([^=]\+\)=\(.*\)$/export \1=\2/p" | sed 's/"/\\\"/g' | sed '/=/s//="/' | sed 's/$/"/' >> /etc/profile)

echo "Starting SSH ..."
service ssh start

# Log the environment variables
env

echo "Starting application ..."
# Start your .NET application
exec dotnet /app/publish/LibraryManagement.API.dll
