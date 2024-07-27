
# syntax=docker/dockerfile:1

ARG DOTNETVERSION=8.0

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:$DOTNETVERSION-alpine AS build

COPY . /source
WORKDIR /source

# This is the architecture you’re building for, which is passed in by the builder.
# Placing it here allows the previous steps to be cached across architectures.
ARG TARGETARCH

# This is the project name, used to build the application.
# Change this to match the name of your project.
ARG PROJECT=dwt

# Build the application.
# Leverage a cache mount to /root/.nuget/packages so that subsequent builds don't have to re-download packages.
RUN echo TARGETARCH: ${TARGETARCH}
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore -a ${TARGETARCH} ${PROJECT}.csproj
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ${PROJECT}.csproj -a ${TARGETARCH} --no-restore -o /app

################################################################################

# If you need to enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md

FROM mcr.microsoft.com/dotnet/aspnet:$DOTNETVERSION-alpine AS final
WORKDIR /app

COPY --from=build /app ./
COPY ./config ./config
COPY ./data ./data

# Create a non-privileged user that the app will run under.
# See https://docs.docker.com/go/dockerfile-user-best-practices/
ARG UID=10001
RUN adduser \
    --disabled-password \
    --gecos "" \
    --home "/nonexistent" \
    --shell "/sbin/nologin" \
    --no-create-home \
    --uid "${UID}" \
    appuser
USER appuser

# Default port for dotnet application
EXPOSE 8080

# Change this to match the name of your project.
ENTRYPOINT ["dotnet", "dwt.dll"]
