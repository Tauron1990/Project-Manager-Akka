﻿using System;
using System.Threading.Tasks;
using Akka.Actor;
using Ionic.Zip;
using Tauron.Application.Master.Commands;

namespace ServiceHost.Installer.Impl.Source
{
    public sealed class LocalSource : IInstallationSource
    {
        public Status ValidateInput(InstallerContext context)
        {
            try
            {
                if (ZipFile.CheckZip((string)context.SourceLocation))
                    return new Status.Success(null);

                return new Status.Failure(new ZipException("Inconsistent Zip Dictionary"));
            }
            catch (Exception e)
            {
                return new Status.Failure(e);
            }
        }

        public Task<Status> PrepareforCopy(InstallerContext context)
            => Task.FromResult<Status>(new Status.Success(null));

        public Task<Status> CopyTo(InstallerContext context, string target)
        {
            return Task.Run<Status>(
                () =>
                {
                    try
                    {
                        using var zip = ZipFile.Read((string)context.SourceLocation);
                        zip.ExtractAll(target, ExtractExistingFileAction.OverwriteSilently);

                        return new Status.Success(null);
                    }
                    catch (Exception e)
                    {
                        return new Status.Failure(e);
                    }
                });
        }

        public void CleanUp(InstallerContext context) { }

        public SimpleVersion Version => SimpleVersion.NoVersion;
        public string ToZipFile(InstallerContext context) => (string)context.SourceLocation;
    }
}