### To Initiate Database
1. Create a migration with `dotnet ef migrations add InitialCreate -v`
2. If database exists, drop it with `dotnet ef database drop -v`
3. To create the database, run `dotnet ef database update -v`
    1. Before running the command above, comment out <strong>lines 41-54</strong> in file `Program.cs` (There are comments in code)
    2. After running the command above, remove the comments of the same code.

### Before Starting Program
1. Make sure all the connection strings are correct in `EventsController.cs`, `VolunteerController.cs`, `ActiveChecker.cs`, and `appsetting.json`
    1. Search the variable _connectionString_ for easier editing.

### To Create an Admin Account
1. Go to the database.
2. Find the `dbo.ASPNetUserRoles` table.
3. Under the _RoleId_ column, change the number.
    1. 1 for Admin Role
    2. 2 for User Role
    3. You can go to the `dbo.ASPNetUsers` table to find which Id corresponds to which user.

### Limitations
1. Events cannot be created in the same time

### Recommendations
1. Creating an Event
    1. Photo be 16:9 aspect ratio
    2. Preview text/short description be less than 280 characters 
