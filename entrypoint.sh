#!/bin/sh
set -e
service ssh start
exec dotnet LibraryManagement.API.dll
