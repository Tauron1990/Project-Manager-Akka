using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.Shared.Api
{
    public sealed class ApiMessageTranslator : IApiMessageTranslator
    { 
        public string Translate<T>(T message)
        {
            return message switch
            {
                RepositoryMessage repositoryMessage => RepositoryMessages(repositoryMessage),
                RepositoryErrorCode repositoryErrorCode => RepositoryErrorCodes(repositoryErrorCode),
                DeploymentMessage deploymentMessage => DeploymentMessages(deploymentMessage),
                DeploymentErrorCode deploymentErrorCode => DeplymentErrorCodes(deploymentErrorCode),
                _ => message?.ToString() ?? string.Empty,
            };
        }

        private static string DeplymentErrorCodes(in DeploymentErrorCode deploymentErrorCode)
        {

            if(deploymentErrorCode == DeploymentErrorCode.CommandDuplicateApp)
                return "Die Anwendung Existiert schon";

            if(deploymentErrorCode == DeploymentErrorCode.GeneralQueryFailed)
                return "Abrufen der Daten Fehlgeschlagen";

            if(deploymentErrorCode == DeploymentErrorCode.GernalBuildError)
                return "Compilierung Fehlgeschlagen";

            if(deploymentErrorCode == DeploymentErrorCode.GerneralCommandError)
                return "Kommando Fehlgeschlagen";

            if(deploymentErrorCode == DeploymentErrorCode.BuildDotNetFailed)
                return "Compiler Anwendung nicht erfolgreich ausgeführt";

            if(deploymentErrorCode == DeploymentErrorCode.BuildDotnetNotFound)
                return "Compiler nicht gefunden";

            if(deploymentErrorCode == DeploymentErrorCode.BuildProjectNotFound)
                return "Project wurde nicht gefunden";

            if(deploymentErrorCode == DeploymentErrorCode.CommandAppNotFound)
                return "Anwendung nicht gefunden";

            if(deploymentErrorCode == DeploymentErrorCode.CommandErrorRegisterRepository)
                return "Fehler beim´registrieren des Repositorys";

            return deploymentErrorCode.Value;
        }

        private static string DeploymentMessages(in DeploymentMessage deploymentMessage)
        {

            if(deploymentMessage == DeploymentMessage.BuildCompled)
                return "Compilieren der Anwedung Abgeschlossen";

            if(deploymentMessage == DeploymentMessage.BuildKilling)
                return "Compiller wird Beended";

            if(deploymentMessage == DeploymentMessage.BuildStart)
                return "Compilierung wird gestartet";

            if(deploymentMessage == DeploymentMessage.RegisterRepository)
                return "Registriere Repository";

            if(deploymentMessage == DeploymentMessage.BuildExtractingRepository)
                return "Enpacke Repository für Compilireung";

            if(deploymentMessage == DeploymentMessage.BuildRunBuilding)
                return "Compilierung wird ausgeführt";

            if(deploymentMessage == DeploymentMessage.BuildTryFindProject)
                return "Projekt wird gesucht";

            return deploymentMessage.Value;
        }

        private static string RepositoryErrorCodes(in RepositoryErrorCode repositoryErrorCode)
        {
            if(repositoryErrorCode == RepositoryErrorCode.DuplicateRepository)
                return "Repository ist Doppelt";

            if(repositoryErrorCode == RepositoryErrorCode.InvalidRepoName)
                return "Falscher Repository Name";

            if(repositoryErrorCode == RepositoryErrorCode.DatabaseNoRepoFound)
                return "Repository nicht in Datenbank gefunden";

            if(repositoryErrorCode == RepositoryErrorCode.GithubNoRepoFound)
                return "Repository nicht auf github gefunden";

            return repositoryErrorCode.Value;
        }

        private static string RepositoryMessages(in RepositoryMessage repositoryMessage)
        {
            if(repositoryMessage == RepositoryMessage.CompressRepository)
                return "Kompremiere Repository";

            if(repositoryMessage == RepositoryMessage.DownloadRepository)
                return "Repository Herunterladen";

            if(repositoryMessage == RepositoryMessage.ExtractRepository)
                return "Repository Extrahieren";

            if(repositoryMessage == RepositoryMessage.GetRepo)
                return "Repository Anfordern";

            if(repositoryMessage == RepositoryMessage.UpdateRepository)
                return "Repository Aktualisieren";

            if(repositoryMessage == RepositoryMessage.GetRepositoryFromDatabase)
                return "Repository von der Datenbank abrufen";

            if(repositoryMessage == RepositoryMessage.UploadRepositoryToDatabase)
                return "Repository zur Datenbank hochladen";

            return repositoryMessage.Value;
        }
    }
}