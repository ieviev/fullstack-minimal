# minimal F# dotnet/js full stack app

#### dependencies:

- node.js 18.0+
- dotnet sdk 8.0+
- docker (optional)

#### initial setup

```bash
npm install
dotnet tool restore
dotnet fable src/Client --noCache
```

#### development

```bash
npm run start:dev
# start client and server separately
# npm run start:client
# npm run start:server
```
then open http://localhost:8080/ in the browser

inspect package.json for other scripts

#### publishing the application for production using Docker

```bash
docker build -t myapplicationname .
```

after this you can run the image locally on port 8080 using 
```bash
docker run --name container_name --rm -p 8080:8080 myapplicationname
```

or host the created image somewhere else


#### hints about the project

the application consists of 3 F# files, `src/Server/Server.fs`, `src/Client/Client.fs`, and `src/Shared/Shared.fs`

the client-side static assets such as favicon.ico or images are kept in `src/Client/assets`

the client application is built using [Fable](https://fable.io/) bindings for React and [Elmish](https://elmish.github.io/elmish/)

the server configuration can be edited in `src/Server/Properties/launchSettings.json`

the entire API layer is completely abstracted away by the `Fable.Remoting` library, which
guarantees that the front-end and server share the same specification in Shared.fs

more reading:
- https://zaid-ajaj.github.io/the-elmish-book/
- Bulma.io styling documentation (https://bulma.io/documentation/)


### develop in docker container instead:

note: this has some caveats, because docker does not have access to filesystem updates for hot reloading

```bash
# build the dev image
docker build -t dev -f Dockerfile-dev .

# run 3 instances of the dev image:
# - F# to js compiler in watch mode
# - client on port 8080
# - server on port 5000

# start compiling F# to JS in watch mode
docker run --rm -ti -v ".:/app" dev -c "npm run start:client-docker"

# make changes to src/Client/Client.fs and see changes in src/Client/Client.fs.js

# start the client on port 8080
docker run --rm -ti -p 8080:8080 -v ".:/app" dev -c "npm run start:vite"

# open http://localhost:8080/ in the browser to see the client

# start the server on port 5000
docker run --rm -ti -p 5000:5000 -v ".:/app" dev -c "npm run start:server"

```

in case of cache issues, run the following command inside the container:
`dotnet fable clean`

on linux, you may want to add rw permissions to the files created by docker by running
```bash

sudo chmod +rw -R src/Client
```

you may want to reclaim the file permissions created by the container, run the following command on the host machine:
```bash
sudo chown -R $USER:$USER .
```

