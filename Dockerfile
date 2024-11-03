FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

LABEL org.opencontainers.image.title="ASK.LiveCompose" \
      org.opencontainers.image.description="ASK.LiveCompose is a lightweight API that allows you to update your Docker Compose services via webhooks." \
      org.opencontainers.image.authors="vincent@ask.be" \
      org.opencontainers.image.url="https://github.com/ask-be/ask.livecompose" \
      org.opencontainers.image.source="https://github.com/ask-be/ask.livecompose" \
      org.opencontainers.image.licenses="GPL-3.0-or-later" \
      org.opencontainers.image.vendor="ASK"

# Install latest version of Docker & Docker compose
RUN apt-get update && \
    apt-get install -y ca-certificates curl && \
    install -m 0755 -d /etc/apt/keyrings && \
    curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc && \
    chmod a+r /etc/apt/keyrings/docker.asc && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/debian $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null && \
    apt-get update && \
    apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /App
COPY --from=build-env /App/out .

ENTRYPOINT ["dotnet", "ASK.LiveCompose.dll"]
