﻿using System;
using System.Collections.Generic;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectRepository.Core
{
    #pragma warning disable MT1000, MA0018, MA0064
    public abstract class SharedObject<TObject, TConfiguration> : IDisposable
        where TObject : SharedObject<TObject, TConfiguration>, new()
        where TConfiguration : class, IReporterProvider, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly object Lock = new();

        private static readonly Dictionary<TConfiguration, ObjectEntry> SharedObjects = new();
        private TConfiguration? _configuration;

        private bool _disposed;

        protected TConfiguration Configuration
        {
            get => _configuration ?? new TConfiguration();
            private set => _configuration = value;
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (Lock)
            {
                var target = SharedObjects[Configuration];
                target.Count--;

                if (target.Count != 0) return;

                _disposed = true;
                SharedObjects.Remove(Configuration);
                InternalDispose();
                GC.SuppressFinalize(this);
            }
        }

        public static TObject GetOrNew(TConfiguration configuration)
        {
            lock (Lock)
            {
                if (SharedObjects.TryGetValue(configuration, out var obj))
                {
                    obj.Count++;

                    return obj.SharedObject;
                }

                var sharedObject = new TObject();
                sharedObject.Init(configuration);

                SharedObjects[configuration] = new ObjectEntry(sharedObject);

                return sharedObject;
            }
        }

        protected void SendMessage(in RepositoryMessage msg) => Configuration.SendMessage(msg);

        private void Init(TConfiguration configuration) => Configuration = configuration;

        protected virtual void InternalDispose()
        {
            GC.SuppressFinalize(this);
        }

#pragma warning disable MA0055
        ~SharedObject()
#pragma warning restore MA0055
        {
            Dispose();
        }

        private sealed class ObjectEntry
        {
            internal ObjectEntry(TObject sharedObject)
            {
                SharedObject = sharedObject;
                Count = 1;
            }

            internal int Count { get; set; }

            internal TObject SharedObject { get; }
        }
    }
}
#pragma warning restore MT1000