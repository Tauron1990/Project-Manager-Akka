using System;
using System.Collections.Generic;
using System.IO;

namespace Tauron.Application.VirtualFiles.Core
{
    public abstract class DictionaryBase<TContext> : SystemNodeBase<TContext>, IDirectory
    {
        protected DictionaryBase(TContext context, FileSystemFeature feature, NodeType nodeType) 
            : base(context, feature, nodeType) { }
        
        protected DictionaryBase(TContext context, FileSystemFeature feature) 
            : base(context, feature, NodeType.Directory) { }

        public abstract IEnumerable<IDirectory> Directories { get; }
        
        public abstract IEnumerable<IFile> Files { get; }

        protected abstract IDirectory GetDirectory(TContext context, string name);
        
        protected abstract IFile GetFile(TContext context, IDirectory actualParent, string name);

        protected IDirectory SplitPath(string name)
        {
            Path.GetInvalidFileNameChars()
        }
        
        public IFile GetFile(string name)
            => throw new NotImplementedException();

        public IDirectory GetDirectory(string name)
            => throw new NotImplementedException();

        public IDirectory MoveTo(string location)
            => throw new NotImplementedException();
    }
}