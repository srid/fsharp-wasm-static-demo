module Single.Client.Main

open Elmish
open Bolero
open Bolero.Html
open System.IO

type Model =
    {
        x: string
    }

let initModel =
    {
        x = "First message"
    }

type Message =
    | SetMessage of string

let update message model =
    match message with
    | SetMessage s -> { model with x = s }

let view model dispatch =
    div [] [
        div [attr.``class`` "bg-green-500"] [
            text model.x
        ]
        button [on.click (fun _ -> dispatch (SetMessage "Done!"))] [ text "Click me" ]
    ]

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        Program.mkSimple (fun _ -> initModel) update view
