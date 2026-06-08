# Safar-webApp
🛠️ Tech Stack & DependenciesLayerTechnologyDescriptionBackend Framework.NET 8.0 (ASP.NET Core)High-performance cross-platform server framework.Frontend EngineRazor Pages & HTML5Server-side rendered views optimized with custom CSS workflows.Database ORMEntity Framework CoreStrongly-typed SQL mappings with exact decimal/currency scales.ContainerizationDockerStandardized multi-stage container deployment.🔧 Installation & Local SetupPrerequisitesMake sure you have the following installed on your machine:.NET 8.0 SDKDocker Desktop (Optional, for containerized run)IDE: Visual Studio 2022 or VS CodeSteps to RunClone the RepositoryBashgit clone [https://github.com/talalash123/Safar.git](https://github.com/talalash123/Safar.git)
cd Safar
Configure Database ConnectionOpen appsettings.json and adjust the connection string to point to your local database server:JSON"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=SafarDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
Apply Database MigrationsRun the following commands in your terminal or Package Manager Console to set up your schemas:Bashdotnet ef database update
Build and Run the ProjectBashdotnet build
dotnet run --launch-profile https
Open your browser and navigate to https://localhost:7051 (or the port specified in your console).🐳 Docker DeploymentTo build and run the application using Docker containers:Build the Docker ImageBashdocker build -t safar-app .
Run the ContainerBashdocker run -d -p 8080:80 --name safar-booking-system safar-app
Access the system via http://localhost:8080.📌 Recent Updates & Commit HistoryFix: Solved ticket confirmation fare amount and database decimal precision mapping.Enhancement: Upgraded Admin sidebar aesthetics and resolved CSS compilation during building.Design: Added premium layout enhancements for customer routing workflows.Initial Release: Completed Safar Premium UI design core implementation."""with open("README.md", "w", encoding="utf-8") as f:f.write(readme_content)print("README.md successfully created.")
