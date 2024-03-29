﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Tauron.Application.Files.VirtualFiles.Core;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.Zip
{
    public class InZipDirectory : DirectoryBase<InternalZipDirectory>
    {
        private InternalZipDirectory _dic;
        private ZipFile _file;

        protected InZipDirectory(
            IDirectory? parentDirectory, string originalPath, InternalZipDirectory dic, ZipFile file,
            string name)
            : base(() => parentDirectory, originalPath, name)
        {
            _dic = dic;
            _file = file;
        }

        public override NodeType Type => NodeType.Directory;
        
        public override DateTime LastModified => _dic.ZipEntry?.ModifiedTime ?? DateTime.MinValue;

        public override bool Exist => _dic.ZipEntry != null || _dic.Files.Count + _dic.Directorys.Count > 0;

        public override IEnumerable<IDirectory> Directories => _dic.Directorys.Select(
            internalZipDirectory
                => new InZipDirectory(
                    this,
                    OriginalPath.CombinePath(internalZipDirectory.Name),
                    internalZipDirectory,
                    _file,
                    internalZipDirectory.Name));

        public override IEnumerable<IFile> Files => _dic.Files.Select(
            ent
                => new InZipFile(this, OriginalPath.CombinePath(InternalZipDirectory.GetFileName(ent)), _file, _dic, ent));

        public override IDirectory GetDirectory(string name)
            => throw new NotSupportedException("Zip Directory Resolution not SUpported");

        protected override void DeleteImpl()
        {
            DeleteDic(_dic, _file);
        }

        private static void DeleteDic(InternalZipDirectory dic, ZipFile file)
        {
            if (dic.ZipEntry != null)
                file.RemoveEntry(dic.ZipEntry);

            foreach (var zipEntry in dic.Files)
                file.RemoveEntry(zipEntry);

            foreach (var internalZipDirectory in dic.Directorys)
                DeleteDic(internalZipDirectory, file);
        }

        protected override InternalZipDirectory GetInfo(string path) => _dic;

        public override IFile GetFile(string name)
        {
            var parts = name.Split(InternalZipDirectory.PathSplit, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length <= 1)
            {
                var compledetPath = OriginalPath.CombinePath(name);

                return new InZipFile(this, compledetPath, _file, _dic, _file[compledetPath]);
            }

            var dic = _dic;
            var inZipParent = this;

            var originalPath = new StringBuilder(OriginalPath);

            for (var i = 0; i < parts.Length; i++)
            {
                if (i == parts.Length - 1)
                    return new InZipFile(
                        inZipParent,
                        originalPath.Append('\\').Append(parts[i]).ToString(),
                        _file,
                        dic,
                        _file[originalPath.ToString()]);

                dic = dic.GetOrAdd(parts[i]);
                originalPath.Append('\\').Append(parts[i]);
                inZipParent = new InZipDirectory(inZipParent, originalPath.ToString(), dic, _file, name);
            }

            throw new FileNotFoundException("File in zip Directory not Found");
        }

        public override IDirectory MoveTo(string location)
            => throw new NotSupportedException("Zip Directory Moving not Supported");

        protected void ResetDirectory(ZipFile file, InternalZipDirectory directory)
        {
            _file = file;
            _dic = directory;
        }
    }
}