# Hangfire Scheduled Comarques Import Job

## Overview
This project uses Hangfire for background job scheduling and execution. The import of comarques (counties) from the DIBA API is automated and runs on a weekly schedule. All Hangfire tables use the prefix `hangfireaj_` for easy identification in the MySQL database.

## How It Works
- **Persistent Storage:** Hangfire is configured to use MySQL with a custom table prefix (`hangfireaj_`).
- **Job Registration:** The `ComarquesImportJob` class implements the logic to fetch, map, and upsert comarques data from the API into the database.
- **Recurring Job:** The job is registered in `Program.cs` to run every week using Hangfire's `RecurringJob.AddOrUpdate`.
- **Dashboard:** The Hangfire dashboard is available at `/hangfire` for monitoring and manual job execution.

## Key Files
- `Program.cs`: Configures Hangfire, registers the job, and sets up the dashboard.
- `Jobs/ComarquesImportJob.cs`: Contains the import logic and API mapping.

## Configuration
- **MySQL Connection:**
  - The connection string is read from `appsettings.Development.json` (key: `HangfireConnection` or `DefaultConnection`).
  - All Hangfire tables will be created with the prefix `hangfireaj_` (e.g., `hangfireaj_job`, `hangfireaj_state`).
- **API Endpoint:**
  - The job fetches data from `https://do.diba.cat/api/tipus/comarca`.

## How to Use
1. **Build the project:**
   ```sh
   dotnet restore
   dotnet build
   ```
2. **Run the application:**
   ```sh
   dotnet run
   ```
3. **Access Hangfire dashboard:**
   - Go to `http://localhost:<port>/hangfire` in your browser.
   - You can trigger the import job manually or monitor scheduled executions.

## Customization
- **Change schedule:**
  - Edit the `Cron.Weekly` parameter in `Program.cs` to use a different schedule (see Hangfire Cron expressions).
- **Change table prefix:**
  - Update the `TablePrefix` property in the Hangfire configuration in `Program.cs`.

## Troubleshooting
- If Hangfire tables are not created, check the MySQL connection string and user permissions.
- All job logs and errors are written to the application logs via `ILogger`.

## References
- [Hangfire Documentation](https://docs.hangfire.io/)
- [DIBA Comarques API](https://do.diba.cat/api/tipus/comarca)

---
**Last updated:** 2026-03-04
