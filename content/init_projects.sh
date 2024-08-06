#!/bin/sh

BASENAME=Dwt

# Create a new solution
dotnet new sln -o ${BASENAME}

# Create projects
cd ${BASENAME}
dotnet new classlib -o ${BASENAME}.Shared
dotnet new webapi -o ${BASENAME}.Api

# Add projects to solution
dotnet sln add ${BASENAME}.Shared/${BASENAME}.Shared.csproj
dotnet sln add ${BASENAME}.Api/${BASENAME}.Api.csproj

# Add references
dotnet add ${BASENAME}.Api/${BASENAME}.Api.csproj reference ${BASENAME}.Shared/${BASENAME}.Shared.csproj
