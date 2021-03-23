A statically hosted (serverless) [Bolero](https://fsbolero.io/) F# app demonstrating the use of .NET API ([`HttpClient`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0)) from the web browser. Specifically it talks to GitHub to display the information for the given GitHub user.

i.e., use F# (no JavaScript!) to create statically hosted sites but with dynamic behaviour and running in [WebAssembly](https://webassembly.org/): https://srid.github.io/GitHubUser/

See [Blazor WebAssembly](https://docs.microsoft.com/en-ca/aspnet/core/blazor/?view=aspnetcore-5.0#blazor-webassembly) for details on the runtime and programming model.

## Running locally

```
dotnet watch -p .\src\GitHubUser\GitHubUser.fsproj run --urls http://localhost:3000
```

## Full build

```
dotnet publish -c Release --self-contained -o out
```

Serve the `out/wwwroot` directory statically using your web server of choice, or use GitHub Pages to deploy them directly from CI (see `.github/workflows/dotnet.yml` in this repo).
