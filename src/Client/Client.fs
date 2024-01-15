// all client-side browser code is defined here
module Client

open Fable.Core
open Browser
open Feliz
open Fable.Remoting.Client
open Shared
open Elmish

// the state of the application
type Model = { Todos: Todo list; Input: string }

// type ErasedPersonProvider = 
//     Fable.JsonProvider.Generator<"""
//     {
//         "name" : "john",
//         "age" : 100,
//         "favColors" : [ "red","green","blue" ]
//     }
//     """>

// let readPerson (json:string) = 
//     let person = ErasedPersonProvider(json)
//     let agein5years = person.age + 5.
//     console.log(person.name)

type Msg =
    // | UpdateModel of newModel:Model // if you don't need granularity
    | SetInput of string
    | RemoveTodo of System.Guid
    | AddTodo // <- starts async request to the server
    | GotAddTodoResponse of Todo // <- response from the server
    | GotTodos of Todo seq
    | GotRemoveTodoResponse of System.Guid
    | GotError of exn

// create the entire API based on specification in Shared.fs
let todosApi =
    Remoting.createApi ()
#if DEBUG
    |> Remoting.withBaseUrl "http://127.0.0.1:5000/api"
#else
    |> Remoting.withBaseUrl "/api"
#endif
    // |> Remoting.withBinarySerialization // replaces json with msgpack for production
    |> Remoting.buildProxy<ITodosApi>

// the application itself consists of a model, a view for the model state and a way to update the model

// returns the initial model and a list of remaining commands to execute
let init() : Model * Cmd<Msg> =
    let model = { Todos = []; Input = "" }

    console.log ("calling init") // example of logging to browser console

    let loadTodosFromServer: Cmd<Msg> =
        Cmd.OfAsync.either // handles an async function as a command
            todosApi.getTodos // (unit -> Async<list<Todo>>)
            () // parameters for getTodos, getTodos() has no parameters
            (fun okResponse -> Msg.GotTodos okResponse) // ('response -> Msg), convert ok response back to message
            (fun errorResponse -> Msg.GotError errorResponse) // (exn -> Msg), convert error response back to message
    // ^ the lambda functions here are redundant

    // these commands will be executed after init
    let remainingCommands = Cmd.batch [ Cmd.ofMsg (SetInput ""); loadTodosFromServer ]

    model, remainingCommands

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    // log every update to console
    console.log ($"update: {msg}")

    // helper to call api and convert any exception to a GotError message
    let callApi apiMethod parameters mapSuccess =
        Cmd.OfAsync.either
            apiMethod
            parameters
            (function
            | Error exn -> GotError(exn) // thrown on api error
            | Ok(data) -> mapSuccess (data))
            GotError // thrown on connection error

    match msg with
    | GotTodos todos -> { model with Todos = List.ofSeq todos }, []
    | SetInput value -> { model with Input = value }, []
    | AddTodo ->
        let newTodo = Todo.create model.Input
        // this is the command that will be executed after the update
        let cmd = callApi todosApi.addTodo newTodo GotAddTodoResponse
        { model with Input = "" }, cmd
    | GotAddTodoResponse newTodo ->
        // update the model state with copy-and-update expression
        { model with Todos = model.Todos @ [ newTodo ] }, [] // <- no more commands to execute
    | GotError(exn) ->
        window.alert (exn.Message)
        model, []
    | RemoveTodo(guid) ->
        let cmd = callApi todosApi.removeTodo guid (fun _ -> GotRemoveTodoResponse(guid))
        model, cmd
    | GotRemoveTodoResponse(guid) ->
        { model with Todos = model.Todos |> List.where (fun todo -> todo.Id <> guid) }, []

open Feliz.Bulma
// open type Feliz.Html // if you don't want to write Html.<name>
// open type Feliz.prop // if you don't want to write prop.<name>
// but beware of name collisions

let containerBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.content [
            Html.ul [
                for todo in model.Todos do
                    Html.li [
                        prop.style [ style.display.flex ]
                        prop.children [
                            Html.p [ prop.text todo.Description; prop.style [ style.fontWeight.bold ] ]
                            Bulma.button.a [
                                prop.style [
                                    // most length units are strongly typed in length.<name>
                                    style.marginLeft (length.auto)
                                // style.custom("margin-left", "auto") // escape hatch in case you can not find the property
                                ]
                                button.isSmall
                                color.isDanger
                                prop.onClick (fun _ -> dispatch (RemoveTodo todo.Id))
                                prop.text "Remove"
                            ]
                        ]
                    ]
            ]
        ]
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Input
                            prop.placeholder "What needs to be done?"
                            prop.onChange (fun text ->
                                SetInput text // creates message with new input
                                |> dispatch // passes message to the update loop
                            )
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        prop.disabled (not (Todo.isValid model.Input))
                        prop.onClick (fun _ -> dispatch AddTodo)
                        prop.text "Add"
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.custom (
                "background",
                "linear-gradient(90deg, rgba(2,0,36,1) 0%, rgba(9,9,121,1) 35%, rgba(0,212,255,1) 100%)"
            )
            style.borderStyle borderStyle.solid
            style.borderColor color.black
        ]
        prop.children [
            Bulma.heroHead [ Bulma.navbar [ Bulma.container [] ] ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [ text.hasTextCentered; prop.text "TO DO" ]
                            containerBox model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]

// create the program and run it
open Elmish.React

Program.mkProgram init update view // pass the init, update and view functions to the program
// optionally track every model change in console for debugging
// |> Program.withConsoleTrace
// reload state on every update
|> Program.withReactBatched "root" // render to the element with id "root"
|> Program.run
// with hot-module-reload
// |> Elmish.HMR.Program.withReactBatched "root" // render to the element with id "root"
// |> Elmish.HMR.Program.run
