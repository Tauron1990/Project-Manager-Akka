using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.Shared.Api
{
    public sealed class ApiMessageTranslator : IApiMessageTranslator
    {
        public string Translate(string message)
            => message switch
            {
                RepositoryMessages.CompressRepository => "Kompremiere Repository",
                RepositoryMessages.DownloadRepository => "Repository Herunterladen",
                RepositoryMessages.ExtractRepository => "Repository Extrahieren",
                RepositoryMessages.GetRepo => "Repository Anfordern",
                RepositoryMessages.UpdateRepository => "Repository Aktualisieren",
                RepositoryMessages.GetRepositoryFromDatabase => "Repository von der Datenbank abrufen",
                RepositoryMessages.UploadRepositoryToDatabase => "Repository zur Datenbank hochladen",
                
                RepositoryErrorCodes.DuplicateRepository => "Repository ist Doppelt",
                RepositoryErrorCodes.InvalidRepoName => "Falscher Repository Name",
                RepositoryErrorCodes.DatabaseNoRepoFound => "Repository nicht in Datenbank gefunden",
                RepositoryErrorCodes.GithubNoRepoFound => "Repository nicht auf github gefunden",
                
                DeploymentMessages.BuildCompled => "Compilieren der Anwedung Abgeschlossen",
                DeploymentMessages.BuildKilling => "Compiller wird Beended",
                DeploymentMessages.BuildStart => "Compilierung wird gestartet",
                DeploymentMessages.RegisterRepository => "Registriere Repository",
                DeploymentMessages.BuildExtractingRepository => "Enpacke Repository für Compilireung",
                DeploymentMessages.BuildRunBuilding => "Compilierung wird ausgeführt",
                DeploymentMessages.BuildTryFindProject => "Projekt wird gesucht",
                
                DeploymentErrorCodes.CommandDuplicateApp => "Die Anwendung Existiert schon",
                DeploymentErrorCodes.GeneralQueryFailed => "Abrufen der Daten Fehlgeschlagen",
                DeploymentErrorCodes.GernalBuildError => "Compilierung Fehlgeschlagen",
                DeploymentErrorCodes.GerneralCommandError => "Kommando Fehlgeschlagen",
                DeploymentErrorCodes.BuildDotNetFailed => "Compiler Anwendung nicht erfolgreich ausgeführt",
                DeploymentErrorCodes.BuildDotnetNotFound => "Compiler nicht gefunden",
                DeploymentErrorCodes.BuildProjectNotFound => "Project wurde nicht gefunden",
                DeploymentErrorCodes.CommandAppNotFound => "Anwendung nicht gefunden",
                DeploymentErrorCodes.CommandErrorRegisterRepository => "Fehler beim´registrieren des Repositorys",

                _ => message
            };
    }
}