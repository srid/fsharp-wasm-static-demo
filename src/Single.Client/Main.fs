module Single.Client.Main

open Elmish
open Bolero
open Bolero.Html
open System.Net.Http
open Microsoft.AspNetCore.Components

type Model =
    {
        x: string
        client: HttpClient
    }
type Message =
    | SetMessage of string
    | Loading 

let update message model =
    match message with
    | SetMessage s -> { model with x = s }
    | Loading -> { model with x = "Doing something long ..." }

let handleClick model dispatch _ = 
    dispatch Loading
    async {
        let! resp = model.client.GetAsync "/users/srid" |> Async.AwaitTask
        let! s = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
        dispatch (SetMessage s)
    } |> Async.Start

let view model dispatch =
    div [] [
        div [attr.``class`` "bg-green-200"] [
            text model.x
        ]
        button [on.click (handleClick model dispatch)] [ text "Click me" ]
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val GitHubClient = Unchecked.defaultof<HttpClient> with get, set

    override this.Program =
        Program.mkSimple (fun _ -> { x = "Start"; client = this.GitHubClient }) update view
