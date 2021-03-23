module Single.Client.Main

open Elmish
open Bolero
open Bolero.Html
open System.Net.Http
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open System.Text.Json
open System.Text.Json.Serialization

// Json bootstrap
// cf. https://github.com/fsbolero/website/pull/19

let options = JsonSerializerOptions()
options.Converters.Add(JsonFSharpConverter())

// GitHub API User type

[<JsonFSharpConverter>]
type User =
    { login: string
      id: int
      name: string
      company: string
      blog: string
      location: string
      followers: int
      following: int
      avatar_url: string
      html_url: string
    }

// DOM helpers

let elClass el cls children =
    el [attr.``class`` cls] children

let divClass =
    elClass div

let aLink url cs = 
        a [
            attr.href url
            attr.target "_blank"
            attr.``class`` "underline"
        ] cs

// Input element Elimish component with label

type InputModel = { label: string; value: string }

type Input() =
    inherit ElmishComponent<InputModel, string>()

    // Check for model changes by only looking at the value.
    override this.ShouldRender(oldModel, newModel) =
        oldModel.value <> newModel.value

    override this.View model dispatch =
        elClass label "py-1 pr-2 font-bold" [
            text model.label
            input [
                attr.value model.value
                on.change (fun e -> dispatch (unbox e.Value))
                attr.``class`` "border-2 p-0.5 ml-2"
            ]
        ]

// GitHub User card Elmish component
type UserCard() =
    inherit ElmishComponent<User option, User option>()

    override this.View model dispatch = 
        concat [
            match model with
            | None -> divClass "" [text "No info yet"]
            | Some user ->
                let mapUrl = sprintf "https://www.bing.com/maps/?q=%s" user.location
                elClass figure "md:flex bg-gray-100 rounded-xl p-8 md:p-0" [
                    img [
                        attr.``class`` "w-32 h-32 md:w-48 md:h-auto md:rounded-none rounded-full mx-auto"
                        attr.width 284
                        attr.height 512
                        attr.src user.avatar_url
                    ]
                    divClass "pt-6 md:p-8 text-center md:text-left space-y-4" [
                        blockquote [] [
                            b [] [aLink user.blog [text user.name]]
                            text " lives in " 
                            aLink mapUrl [text user.location]
                            text " and has "
                            aLink user.html_url [ text (sprintf "%d followers" user.followers) ]
                            br []
                            br []
                            divClass "font-mono text-xs text-gray-400" [
                                text (sprintf "%A" model)
                            ]
                        ]
                    ]
                ]
        ]

// App MVU
type Model =
    {
        userInfo: User option
        user: string
        loading: bool
        client: HttpClient
    }
type Message =
    | SetUserInfo of User
    | SetUser of string
    | Loading 

let update message model =
    match message with
    | SetUserInfo i -> { model with userInfo = Some i; loading = false }
    | SetUser s -> { model with user = s }
    | Loading -> { model with loading = true }

let handleClick model dispatch _ = 
    Loading |> dispatch
    async {
        let reqPath = "/users/" + model.user
        let! resp = model.client.GetAsync reqPath |> Async.AwaitTask
        let! s = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
        let userInfo = JsonSerializer.Deserialize s
        SetUserInfo userInfo |> dispatch
    } |> Async.Start

let view model dispatch =
    divClass "container mx-auto px-4" [
        elClass h1 "p-2 m-4 text-center text-4xl font-bold" [ 
            aLink "https://github.com/srid/GitHubUser" [text "GitHubUser"]
        ]
        elClass p "" [
            text "Enter the Github user you wish to query for, press TAB and hit ENTER (or click the button). "
        ]
        divClass "border-1 my-1 py-1" [
            ecomp<Input,_,_> [] {
                label = "Username"
                value = model.user
            } (fun s -> dispatch (SetUser s))
        ]
        button [
            on.click (handleClick model dispatch)
            attr.disabled (model.loading)
            attr.``class`` "bg-purple-500 text-white rounded p-1 hover:bg-green-500 my-1"
        ] [ 
            text (if model.loading then "..." else ("Get " + model.user))
        ]
        ecomp<UserCard,_,_> [] model.userInfo (fun _ -> ())
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val GitHubClient = Unchecked.defaultof<HttpClient> with get, set

    override this.Program =
        let initialModel = { 
            loading = false
            user = "srid"
            userInfo = None
            client = this.GitHubClient 
        }
        Program.mkSimple (fun _ -> initialModel) update view
        |> Program.withTrace (fun msg model ->
            this.JSRuntime.InvokeVoidAsync("console.log", msg, model) |> ignore)
