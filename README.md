### To Initiate Database
1. Create a migration with `dotnet ef migrations add InitialCreate -v`
2. If database exists, drop it with `dotnet ef database drop -v`
3. To create the database, run `dotnet ef database update -v`
    1. Before running the command above, comment out <strong>lines 41-54</strong> in file `Program.cs` (There are comments in code)
    2. After running the command above, remove the comments of the same code.

### Before Starting Program
1. Make sure all the connection strings are correct in `EventsController.cs`, `VolunteerController.cs`, `ActiveChecker.cs`, and `appsetting.json`
    1. Search the variable _connectionString_ for easier editing.