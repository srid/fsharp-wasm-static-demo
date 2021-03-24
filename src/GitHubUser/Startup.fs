namespace GitHubUser

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Microsoft.Extensions.DependencyInjection
open System.Net.Http
open System

module Program =
    [<EntryPoint>]
    let Main args =
        let builder =
            WebAssemblyHostBuilder.CreateDefault(args)

        let githubClient =
            new HttpClient(BaseAddress = Uri("https://api.github.com"))

        builder.Services.AddSingleton<HttpClient>(githubClient)
        |> ignore

        builder.RootComponents.Add<Main.MyApp>("#main")
        builder.Build().RunAsync() |> ignore
        0
