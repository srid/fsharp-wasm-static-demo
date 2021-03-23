A serverless [Bolero](https://fsbolero.io/) F# app demonstrating the use of [`HttpClient`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0) to interact with external APIs. Specifically it talks to GitHub to display the top repos in each programming language.

WIP

## Running locally

```
dotnet watch -p .\src\GitHubTop\GitHubTop.fsproj run --urls http://localhost:3000
```

## Deploying to Linux

Do a full build,

```
dotnet publish -c Release -r linux-x64 --self-contained
```

Serve the `.\src\GitHubTop\bin\Release\net5.0\publish\wwwroot\` directory statically. It would be a SPA app with no backend.

