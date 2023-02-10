using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.GridFS.Old
{
    [PublicAPI]
    public class GridFsDic : GridFSSystemNode, IDirectory
    {
        protected const string DicId = "GridFsDic-A7515C4C-2901-4186-A77B-DB4A983DB464";

        private readonly object _createLock = new();

        public GridFsDic(GridFSBucket bucket, GridFSFileInfo? fileInfo, IDirectory? parentDirectory, string name, string path, Action? existsNow)
            : base(bucket, fileInfo, parentDirectory, name, path, existsNow) { }

        public override FileSystemFeature Features { get; } =
            FileSystemFeature.Create | FileSystemFeature.Delete | FileSystemFeature.Read | FileSystemFeature.Write;
        
        public override NodeType Type => NodeType.Directory;
        public override bool IsDirectory => true;

        public IEnumerable<IDirectory> Directories
            => from entry in FindRelevantEntries()
               let subPath = entry.Filename.Remove(0, OriginalPath.Length + 1)
               where subPath.EndsWith(DicId, StringComparison.Ordinal) && subPath.IsSingle(c => c == PathHelper.Seperator)
               select new GridFsDic(
                   Bucket,
                   entry,
                   this,
                   subPath.Split(PathHelper.Seperator, 2)[0],
                   entry.Filename.Replace(PathHelper.Seperator + DicId, string.Empty, StringComparison.Ordinal),
                   NotifyExist);

        public IEnumerable<IFile> Files
            => from entry in FindRelevantEntries()
               let fileName = entry.Filename.Remove(0, OriginalPath.Length + 1)
               where !fileName.Contains(PathHelper.Seperator, StringComparison.Ordinal) && !fileName.EndsWith(DicId, StringComparison.Ordinal)
               select new GridFSFile(Bucket, entry, this, fileName, entry.Filename, NotifyExist);

        public IFile GetFile(PathInfo name)
            => GetFile((string)name);
        
        public IDirectory GetDirectory(PathInfo name) 
            => GetDirectory((string)name);

        public IDirectory MoveTo(PathInfo location) => throw new InvalidOperationException("Moving is not Supported");


        public IFile GetFile(string name)
        {
            if (name.Contains(PathHelper.Seperator, StringComparison.Ordinal))
            {
                string[] elements = name.Split(PathHelper.Seperator, 2);
                string fileName = elements[1];
                string dicName = elements[0];
                string fullDicPath = PathHelper.Combine(OriginalPath, dicName);

                var tempDic = new GridFsDic(Bucket, FindEntry(PathHelper.Combine(fullDicPath, DicId)), this, dicName, fullDicPath, NotifyExist);

                return tempDic.GetFile(fileName);
            }

            string fullPath = PathHelper.Combine(OriginalPath, name);

            return new GridFSFile(Bucket, FindEntry(fullPath), this, name, fullPath, NotifyExist);
        }

        public IDirectory GetDirectory(string name)
        {
            if (name.Contains(PathHelper.Seperator, StringComparison.Ordinal))
            {
                string[] elements = name.Split(PathHelper.Seperator, 2);
                string dicPath = elements[1];
                string dicName = elements[0];
                string fullDicPath = PathHelper.Combine(OriginalPath, dicName);

                var tempDic = new GridFsDic(Bucket, FindEntry(PathHelper.Combine(fullDicPath, DicId)), this, dicName, fullDicPath, NotifyExist);

                return tempDic.GetDirectory(dicPath);
            }

            string fullPath = PathHelper.Combine(OriginalPath, name);

            return new GridFsDic(Bucket, FindEntry(PathHelper.Combine(fullPath, DicId)), this, name, fullPath, NotifyExist);
        }

        public IDirectory MoveTo(string location) => throw new NotSupportedException("Directory Movement Not Supported For MongoDb");

        public override void Delete()
        {
            foreach (var entry in FindRelevantEntries().ToArray()) Bucket.Delete(entry.Id);

            base.Delete();
        }

        private IEnumerable<GridFSFileInfo> FindRelevantEntries()
        {
            if (FileInfo is null)
                throw new FileNotFoundException(OriginalPath + " Not Found");

            string start = OriginalPath;

            return Bucket.Find(Builders<GridFSFileInfo>.Filter.Where(f => f.Filename.StartsWith(start))).ToEnumerable();
        }

        private void NotifyExist()
        {
            lock (_createLock)
            {
                if (Exist) return;

                ObjectId id = Bucket.UploadFromBytes(PathHelper.Combine(OriginalPath, DicId), Array.Empty<byte>());

                FindEntry(id);
            }
        }
    }
}