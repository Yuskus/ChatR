ARG registry=mcr.microsoft.com
ARG nuget=https://api.nuget.org/v3/index.json

FROM ${registry}/dotnet/sdk:10.0 AS build

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY *.slnx .
COPY ChatR/*.csproj ./ChatR/

RUN dotnet restore --runtime linux-musl-x64
COPY . .

ARG BUILD_CONFIGURATION=Debug

RUN dotnet publish ChatR --runtime linux-musl-x64 -c ${BUILD_CONFIGURATION} -o out

FROM ${registry}/dotnet/aspnet:10.0-alpine

RUN apk add --no-cache icu-libs tzdata krb5-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV TZ=Europe/Moscow

WORKDIR /app
COPY --from=build /app/out .

ARG DB_CONNECTION
ENV DB_CONNECTION=${DB_CONNECTION}
ARG DOCKER_USER_APP
ARG DOCKER_UID
ARG DOCKER_GID

RUN addgroup -g ${DOCKER_GID} -S ${DOCKER_USER_APP} \
    && adduser -u ${DOCKER_UID} -S -G ${DOCKER_USER_APP} -s /bin/bash ${DOCKER_USER_APP}

USER ${DOCKER_USER_APP}

ENTRYPOINT ["./ChatR"]