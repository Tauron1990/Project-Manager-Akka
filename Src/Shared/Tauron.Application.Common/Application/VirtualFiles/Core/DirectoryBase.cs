using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core
{
    [PublicAPI]
    public abstract class DirectoryBase<TContext> : SystemNodeBase<TContext>, IDirectory
    {
        protected DirectoryBase(TContext context, FileSystemFeature feature, NodeType nodeType) 
            : base(context, feature, nodeType) { }
        
        protected DirectoryBase(TContext context, FileSystemFeature feature) 
            : base(context, feature, NodeType.Directory) { }

        public abstract IEnumerable<IDirectory> Directories { get; }
        
        public abstract IEnumerable<IFile> Files { get; }

        protected abstract IDirectory GetDirectory(TContext context, string name);
        
        protected abstract IFile GetFile(TContext context, string name);

        protected virtual IDirectory SplitDirectoryPath(string name)
        {
            name = GenericPathHelper.NormalizePath(name);

            if (!name.Contains(GenericPathHelper.GenericSeperator)) return GetDirectory(Context, name);

            var elements = name.Split(GenericPathHelper.GenericSeperator, 2);

            return GetDirectory(Context, elements[0]).GetDirectory(elements[1]);

        }
        
        protected virtual IFile SplitFilePath(string name)
        {
            name = GenericPathHelper.NormalizePath(name);

            if (!name.Contains(GenericPathHelper.GenericSeperator)) return GetFile(Context, name);

            var elements = name.Split(GenericPathHelper.GenericSeperator, 2);

            return GetDirectory(Context, elements[0]).GetFile(elements[1]);
        }
        
        public IFile GetFile(string name)
            => SplitFilePath(name);

        public IDirectory GetDirectory(string name)
            => SplitDirectoryPath(name);

        public IDirectory MoveTo(string location)
        {
            ValidateFeature(FileSystemFeature.Moveable);
            
            return MovetTo(Context, GenericPathHelper.NormalizePath(location));
        }

        protected virtual IDirectory MovetTo(TContext context, string location)
            => throw new IOException("Move is not Implemented");
    }
}