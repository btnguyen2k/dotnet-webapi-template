
# syntax=docker/dockerfile:1

ARG DOTNETVERSION=8.0
ARG BASEIMAGE=alpine

# Use --platform=$BUILDPLATFORM in order to correctly pull the base image for the build platform.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:$DOTNETVERSION-$BASEIMAGE AS build

COPY . /source
WORKDIR /source

ARG TARGETARCH
ARG BUILDPLATFORM
ARG TARGETPLATFORM

# Build the application.
RUN echo "Running on $BUILDPLATFORM, building for $TARGETPLATFORM ($TARGETARCH)"
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish -a $TARGETARCH --no-restore --property:PublishDir=/app

################################################################################

# If you need to enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md

FROM mcr.microsoft.com/dotnet/aspnet:$DOTNETVERSION-$BASEIMAGE AS final
WORKDIR /app

# Change this to match the name of your application.
ARG BASENAME="Dwt"

COPY --from=build /app ./
COPY ${BASENAME}.Api/config ./config
COPY ${BASENAME}.Api/data ./data

# Create a non-privileged user that the app will run under.
# See https://docs.docker.com/go/dockerfile-user-best-practices/
ARG UID=10001
RUN adduser \
    --disabled-password \
    --gecos "" \
    --home "/nonexistent" \
    --shell "/sbin/nologin" \
    --no-create-home \
    --uid "$UID" \
    appuser
RUN chown -R appuser:appuser /app
USER appuser

# Enable Swagger UI
ENV ENABLE_SWAGGER_UI=true

# Initialize the database
ENV INIT_DB=true

# Default port for dotnet application
EXPOSE 8080

# Change "Dwt.Api.dll" to match the name of your application.
ENTRYPOINT ["dotnet", "Dwt.Api.dll"]
