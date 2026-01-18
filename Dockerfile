FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS builder

WORKDIR /app

# Native build requirements for Miningcore
RUN apt-get update && \
    apt-get install -y \
        cmake clang ninja-build build-essential libssl-dev pkg-config \
        libboost-all-dev libsodium-dev libzmq5 libzmq3-dev golang-go \
        libgmp-dev libc++-dev zlib1g-dev

# Copy the full source tree
COPY . .
WORKDIR /app/src/Miningcore
RUN dotnet publish -c Release --framework net8.0 -o ../../build

# --------------------------
#   RUNTIME STAGE (.NET 8)
# --------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy

WORKDIR /app

# Runtime dependencies only
RUN apt-get update && \
    apt-get install -y libzmq5 libsodium-dev curl && \
    apt-get clean

EXPOSE 4000-4090

COPY --from=builder /app/build ./


CMD ["./Miningcore", "-c", "config.json"]
