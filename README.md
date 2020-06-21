# A Visual Studio Project: Blazor WASM, IdentityServer4 with Multiple Roles &amp; gRPC Roles Authorization

This **.NET 5 Preview** repo combines two repo's by **@javiercn**:

1. https://github.com/javiercn/BlazorAuthRoles
2. https://github.com/javiercn/BlazorGrpcAuth

And adds Role Authorization to the Greeter gRPC Service.

    endpoints
      .MapGrpcService<GreeterService>()
      .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrator"})
      .EnableGrpcWeb();
        
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

The `admin@example.com` will be assigned to the roles: 'Administrators' &amp; 'Users'

The `user@example.com` will be assigned to the Users role only.

