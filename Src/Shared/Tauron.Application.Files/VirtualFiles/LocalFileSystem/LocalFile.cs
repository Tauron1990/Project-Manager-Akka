﻿using System;
using System.IO;
using Tauron.Application.Files.VirtualFiles.Core;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.VirtualFiles.LocalFileSystem
{
    public class LocalFile : FileBase<FileInfo>
    {
        public LocalFile(string fullPath, IDirectory path)
            : base(() => path, fullPath, fullPath.GetFileName())
        {
        }

        private LocalFile(string fullPath)
            : base(() => new LocalDirectory(fullPath.GetDirectoryName() ?? throw new InvalidOperationException("Directory Name not Found")), fullPath, fullPath.GetFileName())
        {
        }

        public override DateTime LastModified => InfoObject?.LastWriteTime ?? DateTime.MinValue;

        public override bool Exist => InfoObject?.Exists ?? false;

        public override string Extension
        {
            get => InfoObject?.Extension ?? string.Empty;
            set
            {
                if (InfoObject?.Extension == value) return;

                string newPath = Path.ChangeExtension(OriginalPath, value);
                MoveFile(InfoObject, newPath);
                Name = Path.ChangeExtension(OriginalPath, Name);

                Reset(newPath, ParentDirectory);
            }
        }

        public override long Size => InfoObject?.Length ?? 0;

        public override IFile MoveTo(string location)
        {
            if (InfoObject == null) return this;

            if (InfoObject.FullName == location) return this;

            MoveFile(InfoObject, location);

            return new LocalFile(location);
        }

        protected override void DeleteImpl() 
            => InfoObject?.Delete();

        protected override FileInfo GetInfo(string path) => new(path);

        protected override Stream CreateStream(FileAccess access, InternalFileMode mode)
        {
            if (mode is InternalFileMode.Create or InternalFileMode.CreateNew)
                ParentDirectory?.OriginalPath.CreateDirectoryIfNotExis();

            return new FileStream(OriginalPath, (FileMode) mode, access, access == FileAccess.Read ? FileShare.Read : FileShare.None);
        }

        private static void MoveFile(FileInfo? old, string newLoc)
            => old?.MoveTo(newLoc);
    }
}