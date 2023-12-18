# Database Setup
# =================
*TODO: Move to docker compose*

## SqlServer
```bash
# Pull sql server image
$ docker pull mcr.microsoft.com/mssql/server:2022-latest

# Run sql server container
$ sudo docker run -e "ACCEPT_EULA=Y" -e 'MSSQL_SA_PASSWORD=<password>' -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2022-latest

# Start sql server container if stopped
$ docker start sql1

# Add initial migration if missing
$ dotnet ef migrations add InitialCreate --project FisherTournament.Infrastructure/ --startup-project FisherTournament.API  -v -o Persistence/Migrations

# Update database
$ dotnet ef database update -p FisherTournament.Infrastructure/ -s FisherTournament.API --connection "<connection-string>"
```

## SQLite
```bash
# Ensure UseSqlite in Infrastructure dependency injection
# Add initial migration if missing
$ dotnet ef migrations add InitialMigration --project FisherTournament.Infrastructure/ --startup-project FisherTournament.API  -v -o Persistence/Migrations/Tournament --context "TournamentFisherDbContext"
$ dotnet ef migrations add InitialMigration --project FisherTournament.Infrastructure/ --startup-project FisherTournament.API  -v -o Persistence/Migrations/ReadModels --context "ReadModelsDbContext"  


# Update database
$ dotnet ef database update -p FisherTournament.Infrastructure -s FisherTournament.API --connection 'Data Source=Tournament.db' --context "TournamentFisherDbContext"
$ dotnet ef database update -p FisherTournament.Infrastructure -s FisherTournament.API --connection 'Data Source=ReadModels.db' --context "ReadModelsDbContext"  
```