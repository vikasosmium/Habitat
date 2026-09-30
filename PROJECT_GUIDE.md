# Habitat property marketplace

This is a server-rendered ASP.NET Core MVC application. It uses C#, Razor views, ASP.NET Core Identity for accounts, Entity Framework Core for persistence, and Microsoft SQL Server. It does not need a JavaScript framework or a separate frontend server.

## Run it locally

Requirements: .NET 10 SDK and SQL Server LocalDB (available with many Visual Studio installations), or another SQL Server instance.

1. Open this folder in a terminal.
2. Set `ConnectionStrings__DefaultConnection` if your SQL Server is not LocalDB. The development value in `appsettings.json` targets `(localdb)\MSSQLLocalDB` and a database named `FlatFinder`.
3. Run `dotnet run`.
4. Open the HTTPS URL printed by ASP.NET Core. On startup, the app applies EF Core migrations and creates the database tables.
5. Choose **Create account** and enter your name, login ID and password. Your login ID is used to sign in; ASP.NET Identity stores password hashes, not plain-text passwords.

Example SQL Server connection string for a local SQL Server Express instance:

```text
Server=.\SQLEXPRESS;Database=FlatFinder;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Set it for the current PowerShell session before `dotnet run`:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=.\SQLEXPRESS;Database=FlatFinder;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True'
dotnet run
```

For a remote or shared deployment, keep credentials out of `appsettings.json`; use .NET user secrets locally or the hosting platform's secret/environment-variable store. Restrict the database login to the permissions the application needs.

## What the folders do

- `Program.cs` configures MVC, Identity, SQL Server, request routing, and applies database migrations when the app starts.
- `Controllers/AccountController.cs` handles name-based registration, sign-in and sign-out. `PropertiesController.cs` handles browsing, owner contact, and listing management.
- `Models/PropertyListing.cs` describes a property; `PropertyPhoto.cs` stores each uploaded photo path. `ApplicationUser.cs` stores the profile name.
- `Data/ApplicationDbContext.cs` connects accounts, listings and photos to EF Core. `Migrations/` records SQL schema changes.
- `Views/Properties/` contains the recent-property homepage, property details and posting forms. `Views/Account/` contains the simple login and registration forms.
- `wwwroot/css/site.css` contains the responsive styling. `wwwroot/uploads/` holds uploaded property photos.

## Main user flows

### Find a place

The homepage shows the newest properties first, with **50 listings per page**. Search by property name or area, and optionally choose rent or sale. Open a property to see its details, map link, and owner contact options.

### Publish and manage a place

A visitor signs in before opening **Post a property**. The form asks for the property type, price, address, area and contact details. A map link can be pasted, or the user can click **Use my location** and grant the browser permission. Up to five JPEG, PNG or WebP photos (5 MB each) are accepted; videos and other file types are rejected. After posting, the property is shown on the homepage. **My properties** is limited to the signed-in owner.

### Accounts

ASP.NET Core Identity powers the custom `/Account/Register` and `/Account/Login` forms. The profile menu shows the first letter of the account name and includes the owner's properties and sign-out action. For production, add account recovery, rate limiting, and a stronger account-verification flow before opening public registration.

## Database changes

The application applies checked-in EF migrations at startup. To create a future migration after changing a model, install the EF command-line tool once (`dotnet tool install --global dotnet-ef --version 10.0.11`) and run:

```powershell
dotnet ef migrations add DescribeYourChange
dotnet ef database update
```

Photos are saved under `wwwroot/uploads/` for this local starter. For a public deployment, move uploads to controlled object storage and add malware scanning and image re-encoding.
