module Single.Client.Main

open Elmish
open Bolero
open Bolero.Html
open System.Net.Http
open System.Net
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open System.Text.Json
open System.Text.Json.Serialization

// Json bootstrap
// cf. https://github.com/fsbolero/website/pull/19

let options = JsonSerializerOptions(IgnoreNullValues = true)
options.Converters.Add(JsonFSharpConverter())

// GitHub API User type

[<JsonFSharpConverter>]
type User =
    { login: string
      id: int
      name: string option
      company: string option
      blog: string option
      location: string option
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
    inherit ElmishComponent<User, User option>()

    override this.View user dispatch = 
        let userName = user.name |> Option.defaultValue user.login
        let userWebUrl = match user.blog with 
                            | None -> user.html_url
                            | Some "" -> user.html_url  // In case the user has nullfied this field
                            | Some x -> x
        let location = user.location |> Option.defaultValue "Earth"
        let mapUrl = sprintf "https://www.bing.com/maps/?q=%s" location
        elClass figure "md:flex bg-gray-100 rounded-xl p-8 md:p-0" [
            img [
                attr.``class`` "w-32 h-32 md:w-48 md:h-auto md:rounded-none rounded-full mx-auto"
                attr.width 284
                attr.height 512
                attr.src user.avatar_url
            ]
            divClass "pt-6 md:p-8 text-center md:text-left space-y-4" [
                blockquote [] [
                    b [] [aLink userWebUrl [text userName]]
                    text " lives in " 
                    aLink mapUrl [text location]
                    text " and has "
                    aLink user.html_url [ text (sprintf "%d followers" user.followers) ]
                    br []
                    br []
                    divClass "font-mono text-xs text-gray-400" [
                        text (sprintf "%A" user)
                    ]
                ]
            ]
            ]

// App MVU
type Model =
    {
        user: string
        userInfo: Choice<User,string> option
        loading: bool
        client: HttpClient
    }
type Message =
    | SetUserInfo of User
    | SetUserError of string
    | SetUser of string
    | Loading 

let update message model =
    match message with
    | SetUserInfo i -> { model with userInfo = Some (Choice1Of2 i); loading = false }
    | SetUserError s -> { model with userInfo = Some (Choice2Of2 s); loading = false }
    | SetUser s -> { model with user = s.Trim() }
    | Loading -> { model with loading = true }

let loadUser (client: HttpClient) dispatch userName = 
    Loading |> dispatch
    async {
        let reqPath = "/users/" + userName
        let! resp = client.GetAsync reqPath |> Async.AwaitTask
        match resp.StatusCode with 
        | HttpStatusCode.OK -> 
            let! s = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
            try 
                let user = JsonSerializer.Deserialize(s, options)
                SetUserInfo user |> dispatch
            with 
                | ex ->
                SetUserError (ex.ToString()) |> dispatch
        | _ -> 
            let! s = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
            SetUserError (sprintf "Error(%A): %s" resp.StatusCode s) |> dispatch
    } |> Async.Start

let view model dispatch =
    divClass "container mx-auto px-4" [
        elClass h1 "p-2 m-4 text-center text-4xl font-bold" [ 
            aLink "https://github.com/srid/GitHubUser" [text "GitHubUser"]
        ]
        elClass p "" [
            text "Enter the Github username (eg: srid) you wish to query for and hit ENTER."
        ]
        divClass "border-1 my-1 py-1" [
            ecomp<Input,_,_> [] {
                label = "Username"
                value = model.user
            } (fun s -> loadUser model.client dispatch s)
        ]
        match (model.loading, model.userInfo) with
        | true, _ -> divClass "" [ text "Loading..." ]
        | _, None -> divClass "" [text "No info yet"]
        | _, Some (Choice2Of2 err) -> divClass "bg-red-200" [pre [] [text err]]
        | _, Some (Choice1Of2 user) -> 
            ecomp<UserCard,_,_> [] user (fun _ -> ())
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    [<Inject>]
    member val GitHubClient = Unchecked.defaultof<HttpClient> with get, set

    override this.Program =
        let initialModel = { 
            loading = false
            user = ""
            userInfo = None
            client = this.GitHubClient 
        }
        Program.mkSimple (fun _ -> initialModel) update view
        |> Program.withTrace (fun msg model ->
            this.JSRuntime.InvokeVoidAsync("console.log", msg, model) |> ignore)
