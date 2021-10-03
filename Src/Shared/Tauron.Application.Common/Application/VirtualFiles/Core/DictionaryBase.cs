using System;
using System.Collections.Generic;

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
        
        public IFile GetFile(string name)
            => throw new NotImplementedException();

        public IDirectory GetDirectory(string name)
            => throw new NotImplementedException();

        public IDirectory MoveTo(string location)
            => throw new NotImplementedException();
    }
}