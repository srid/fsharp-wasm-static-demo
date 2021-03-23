module Single.Client.Main

open Elmish
open Bolero
open Bolero.Html
open System.Net.Http
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop

let elClass el cls children =
    el [attr.``class`` cls] children

let divClass =
    elClass div
type InputModel = { label: string; value: string }

type Input() =
    inherit ElmishComponent<InputModel, string>()

    // Check for model changes by only looking at the value.
    override this.ShouldRender(oldModel, newModel) =
        oldModel.value <> newModel.value

    override this.View model dispatch =
        elClass label "py-1 pr-2" [
            text model.label
            input [
                attr.value model.value
                on.change (fun e -> dispatch (unbox e.Value))
                attr.``class`` "border-2 p-1"
            ]
        ]

type Model =
    {
        x: string
        user: string
        loading: bool
        client: HttpClient
    }
type Message =
    | SetMessage of string
    | SetUser of string
    | Loading 

let update message model =
    match message with
    | SetMessage s -> { model with x = s; loading = false }
    | SetUser s -> { model with user = s }
    | Loading -> { model with x = "Doing something long ..."; loading = true }

let handleClick model dispatch _ = 
    Loading |> dispatch
    async {
        let reqPath = "/users/" + model.user
        let! resp = model.client.GetAsync reqPath |> Async.AwaitTask
        let! s = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
        SetMessage s |> dispatch
    } |> Async.Start


let view model dispatch =
    divClass "container mx-auto" [
        elClass h1 "p-2 m-4 text-center text-4xl font-bold" [ text "GitHubUser"]
        elClass p "rounded bg-gray-100 p-1 m-1" [
            text "Enter the Github user you wish to query for, press TAB and hit ENTER (or click the button). "
        ]
        divClass "border-1 m-1 p-1" [
            ecomp<Input,_,_> [] {
                label = "Username"
                value = model.user
            } (fun s -> dispatch (SetUser s))
        ]
        button [
            on.click (handleClick model dispatch)
            attr.disabled (model.loading)
            attr.``class`` "bg-red-500 text-white rounded p-1 m-1"
        ] [ 
            text (if model.loading then "..." else ("Get " + model.user))
        ]
        divClass "font-mono bg-green-50 p-1 m-1" [
            text model.x
        ]
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val GitHubClient = Unchecked.defaultof<HttpClient> with get, set

    override this.Program =
        let initialModel = { 
            x = "Nothing to show" 
            loading = false
            user = "srid"
            client = this.GitHubClient 
        }
        Program.mkSimple (fun _ -> initialModel) update view
        |> Program.withTrace (fun msg model ->
            this.JSRuntime.InvokeVoidAsync("console.log", msg, model) |> ignore)
