# Blazor WASM, IdentityServer4 with Multiple Roles &amp; gRPC Roles Authorization

This **.NET 5 Preview** repo combines two repo's by **@javiercn**:

1. https://github.com/javiercn/BlazorAuthRoles
2. https://github.com/javiercn/BlazorGrpcAuth

And adds Role Authorization to the Greeter gRPC Service:

***Server/[Startup.cs](BlazorTemplate/Server/Startup.cs)***

    endpoints
        .MapGrpcService<GreeterService>()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Administrator"})
        .EnableGrpcWeb();
        
I've also added a 'Claims' page with a list of the current user's claims.

It uses the Kestrel webserver as default, a SQLite database and is "*CTRL-F5'able*" without any further configuration.

You can delete de SQLite database and migrations folder if you want and use the following commands in Visual Studio's Package Manager Console to re-create the db.

1. Add-Migration InitialCreate
2. Update-Database

At first run the app will create 2 users (_if they don't exist, see: Server/[SeedData.cs](BlazorTemplate/Server/Data/SeedData.cs)_)

1. `admin@example.com` / `Qwerty1234#`
2. `user@example.com` / `Qwerty1234#`

and 2 roles: 

1. Users
2. Administrators

The 'Administrators' &amp; 'Users' roles will be assigned to: `admin@example.com`

The 'Users' role will be assigned to: `user@example.com`

#### TODO 1 - Additional Claim(s)

I've extended ASP.NET Identity AspNetUsers table with an extra 'CustomClaim' field (_see: Server/Models/[ApplicationUser.cs](BlazorTemplate/Server/Models/ApplicationUser.cs)_). I want to use that claim value in the client but haven't figured out how to do that.

#### TODO 2 - Claims Profile Service

To use Name and Role claims with API authorization and Identity Server you can use one of the [following approaches](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server#configure-identity-server) (_MS Docs_)

1. API authorization options
2. Profile Service

I'm using the (_first_) 'API authorization options' now but I've included Server/[ProfileService.cs](BlazorTemplate/Server/ProfileService.cs) and included (_commented_) code in Server/[Startup.cs](BlazorTemplate/Server/Startup.cs) to use that, but I haven't got it working yet.
