FROM mcr.microsoft.com/dotnet/sdk:5.0.402-alpine3.13 as build-env

WORKDIR /app
COPY SqlHydraDemo.fsproj .
COPY ./src/ ./src/

RUN dotnet publish -c Release -o out -warnaserror SqlHydraDemo.fsproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:5.0.11-alpine3.13

RUN wget -O rambler https://github.com/elwinar/rambler/releases/download/v5.4.0/rambler-alpine-amd64
RUN chmod +x rambler

WORKDIR /app

COPY --from=build-env /app/out/ .
COPY migrations migrations
COPY rambler.json ./

# Entrypoint
COPY docker-entrypoint.sh .
RUN ["chmod", "+x", "./docker-entrypoint.sh"]
ENTRYPOINT ["./docker-entrypoint.sh"]
