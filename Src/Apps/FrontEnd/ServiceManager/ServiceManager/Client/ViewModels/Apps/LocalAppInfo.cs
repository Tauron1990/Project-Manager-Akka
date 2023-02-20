using System;
using JetBrains.Annotations;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Client.ViewModels.Apps
{
    public sealed class LocalAppBinary
    {
        [UsedImplicitly]
        public LocalAppBinary()
        {
            
        }
        
        public LocalAppBinary(AppInfo app, AppBinary appBinary)
            : this(app, appBinary.FileVersion.Value, appBinary.AppName.Value, appBinary.CreationTime, appBinary.Commit.Value, appBinary.Repository.Value)
        {
            
        }

        private LocalAppBinary(AppInfo app, int fileVersion, string appName, DateTime creationTime, string commit, string repository)
        {
            App = app;
            FileVersion = fileVersion;
            AppName = appName;
            CreationTime = creationTime;
            Commit = commit;
            Repository = repository;
        }

        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public AppInfo App { get; set; } = AppInfo.Empty;
        public int FileVersion { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string AppName { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public string Commit { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;

    }
    
    public sealed class LocalAppInfo
    {
        public static readonly LocalAppInfo Empty = new(AppInfo.Empty);
        
        [UsedImplicitly]
        public LocalAppInfo()
        {
            
        }
        
        public LocalAppInfo(AppInfo self)
            : this(self, self.Name.Value, self.LastVersion.Value, self.UpdateDate, self.CreationTime, self.Repository.Value, self.ProjectName.Value)
        {
            
        }

        public LocalAppInfo(AppInfo self, string name, int lastVersion, DateTime updateDate, DateTime creationTime, string repository, string projectName)
        {
            Self = self;
            Name = name;
            LastVersion = lastVersion;
            UpdateDate = updateDate;
            CreationTime = creationTime;
            Repository = repository;
            ProjectName = projectName;
        }

        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public AppInfo Self { get; set; } = AppInfo.Empty;
        public string Name { get; set; } = string.Empty;
        public int LastVersion { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreationTime { get; set; }
        public string Repository { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global
}