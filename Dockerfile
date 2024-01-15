# Build using `docker build -t myapplicationname .`
# then host the container e.g. `docker run --name container_name --rm -p 8080:8080 myapplicationname`
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

# Install build dependencies
RUN apt-get update
RUN apt-get install -y ca-certificates curl gnupg
RUN mkdir -p /etc/apt/keyrings
RUN curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
ENV NODE_MAJOR=20
RUN echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list
RUN apt-get update
RUN apt-get install nodejs -y

WORKDIR /workspace
COPY . .
RUN dotnet tool restore

# Compile the application for production
RUN npm install
RUN dotnet fable src/Client
RUN cd src/Server && dotnet publish -c Release -o ../../deploy
RUN npx vite build

# Create production application container
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
COPY --from=build /workspace/deploy /app
COPY --from=build /workspace/deploy-client /app/public

WORKDIR /app
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Server.dll" ]
