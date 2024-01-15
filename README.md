# minimal F# dotnet/js full stack app

#### dependencies:

- node.js 18.0+
- dotnet sdk 8.0+
- docker (optional)

#### initial setup

```bash
npm install
dotnet fable src/Client --noCache
```

#### development

```bash
npm run start:dev
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

