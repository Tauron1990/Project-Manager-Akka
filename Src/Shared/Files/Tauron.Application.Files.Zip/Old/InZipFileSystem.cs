﻿using System;
using System.IO;
using Ionic.Zip;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.Zip
{
    [PublicAPI]
    public class InZipFileSystem : InZipDirectory, IVirtualFileSystem
    {
        private ZipFile _file;

        public override NodeType Type => NodeType.Root;

        public InZipFileSystem(ZipFile file)
            : base(null, "zip::", InternalZipDirectory.ReadZipDirectory(file), file, string.Empty)
        {
            _file = file;
            SaveAfterDispose = true;
        }

        public InZipFileSystem()
            : this(new ZipFile()) { }

        public void Dispose()
        {
            if (SaveAfterDispose && !string.IsNullOrWhiteSpace(_file.Name))
                _file.Save();

            _file.Dispose();
        }

        public bool IsRealTime => false;
        public bool SaveAfterDispose { get; set; }

        public override DateTime LastModified
            => _file.Name.ExisFile() ? File.GetLastWriteTime(_file.Name) : base.LastModified;

        public string Source => _file.Name ?? string.Empty;

        public void Reload(string source)
        {
            if (SaveAfterDispose && !string.IsNullOrWhiteSpace(_file.Name))
                _file.Save();
            _file.Dispose();

            if (!ZipFile.IsZipFile(source)) return;

            _file = ZipFile.Read(source);
            ResetDirectory(_file, InternalZipDirectory.ReadZipDirectory(_file));
            Reset(OriginalPath, null);
        }

        public override bool Exist => true;

        protected override void DeleteImpl()
        {
            if (string.IsNullOrWhiteSpace(_file.Name)) return;

            _file.Dispose();
            File.Delete(_file.Name);
        }
    }
}