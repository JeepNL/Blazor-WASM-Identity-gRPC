# A Visual Studio Project: Blazor WASM, IdentityServer4 with Multiple Roles &amp; gRPC Roles Authorization

This **.NET 5 Preview** repo combines two repo's by **@javiercn**:

1. https://github.com/javiercn/BlazorAuthRoles
2. https://github.com/javiercn/BlazorGrpcAuth

And adds Role Authorization to the Greeter gRPC Service:

*Server/Startup.cs*

    endpoints
        .MapGrpcService<GreeterService>()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrator"})
        .EnableGrpcWeb();
        
I've also added a 'Claims' page with a list of the current user's claims.

It uses the Kestrel webserver as default, a SQLite database and is "*CTRL-F5'able*" without any further configuration.

You can delete de SQLite database and migrations folder if you want and use the following commands in Visual Studio's Package Manager Console to re-create the db.

1. Add-Migration InitialCreate
2. Update-Database

At first run the app will create 2 users:

1. `admin@example.com` / `Qwerty1234#`
2. `user@example.com` / `Qwerty1234#`

And 2 roles: 

1. Users
2. Administrators

The 'Administrators' &amp; 'Users' roles will be assigned to: `admin@example.com`

The 'Users' role will be assigned to: `user@example.com`

