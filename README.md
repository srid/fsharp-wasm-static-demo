A serverless [Bolero](https://fsbolero.io/) F# app demonstrating the use of [`HttpClient`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-5.0) to interact with external APIs. Specifically it talks to GitHub to display the information for the given GitHub user.

WIP

## Running locally

```
dotnet watch -p .\src\GitHubUser\GitHubUser.fsproj run --urls http://localhost:3000
```

## Deploying to Linux

Do a full build,

```
dotnet publish -c Release -r linux-x64 --self-contained
```

Serve the `.\src\GitHubUser\bin\Release\net5.0\publish\wwwroot\` directory statically. It would be a SPA app with no backend.

