module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open Shared

module Storage =
    let todos = ResizeArray()

    let addTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok(todo)
        else
            Error(System.Exception("Invalid todo"))

    let tryRemoveTodo(guid: System.Guid) =
        todos
        |> Seq.tryFind (fun v -> v.Id = guid)
        |> function
            | None -> Error(System.Exception($"No todo with such id: {guid}"))
            | Some(todo) ->
                todos.Remove(todo) |> ignore
                Ok()

    do
        [ Todo.create "Hello world from server"; Todo.create "Something important" ]
        |> Seq.iter (addTodo >> ignore)

// the entire API implementation is defined in this record type
let todosApi: Shared.ITodosApi = {
    getTodos =
        fun _ -> async {
            stdout.WriteLine $"server received getTodos"
            return Storage.todos
        }
    addTodo =
        fun todo -> async {
            stdout.WriteLine $"server received addTodo: %A{todo}"
            return (Storage.addTodo todo)
        }
    removeTodo = (fun guid -> async { return (Storage.tryRemoveTodo guid) })
}

let apiHttpHandler: Giraffe.Core.HttpHandler =
    Remoting.createApi ()
    // |> Remoting.withRouteBuilder (fun typeName methodName -> sprintf $"/customApiUrl/%s{typeName}/%s{methodName}")
    // |> Remoting.withBinarySerialization // replaces json with msgpack
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let appRouter = router {
    pipe_through (Giraffe.Core.setHttpHeader "SomeHeader" "123")
    // pipe_through (requireHeader "Accept" "asd")
    forward "/api" apiHttpHandler
}

let app: Microsoft.Extensions.Hosting.IHostBuilder = application {
    use_router appRouter
    memory_cache

    use_cors
        "anything"
        (fun builder -> builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod() |> ignore)

    use_static "public"
    use_gzip
}

run app
