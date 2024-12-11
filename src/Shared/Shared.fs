// this file is compiled both to .NET and JS
namespace Shared

open System

type Todo = {
    Id: Guid
    Description: string
} with

    override this.ToString() = $"{this.Description}"

module Todo =
    let isValid(description: string) = not (String.IsNullOrWhiteSpace description)

    let create(description: string) = {
        Id = Guid.NewGuid()
        Description = description
    }

// this type defines the shared API
type ITodosApi = {
    getTodos: unit -> Async<Todo[]>
    addTodo: Todo -> Async<Todo>
    removeTodo: Guid -> Async<unit>
}
