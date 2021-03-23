namespace Single.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
// open Bolero.Remoting.Client
open Microsoft.Extensions.DependencyInjection
open System.Net.Http
open System

module Program =
    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<Main.MyApp>("#main")
        let apiBase = Uri("https://api.github.com")
        builder.Services.AddSingleton<HttpClient>(new HttpClient (BaseAddress=apiBase)) |> ignore
        // builder.Services.AddRemoting(builder.HostEnvironment) |> ignore
        builder.Build().RunAsync() |> ignore
        0
