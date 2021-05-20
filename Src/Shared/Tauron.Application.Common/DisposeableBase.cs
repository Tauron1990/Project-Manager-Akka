﻿using System;
using Akka.Util;
using JetBrains.Annotations;
using NLog;

namespace Tauron
{
    [PublicAPI]
    public abstract class DisposeableBase : IDisposable
    {
        private readonly AtomicBoolean _isDisposed = new();
        private Action? _tracker;

        public bool IsDisposed => _isDisposed.Value;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void ThrowDispose()
        {
            if (_isDisposed.Value)
                throw new ObjectDisposedException(nameof(GetType));
        }

        public void TrackDispose(Action action) => _tracker = _tracker.Combine(action);

        public void RemoveTrackDispose(Action action) => _tracker = _tracker.Remove(action);

        protected void Dispose(bool disposing)
        {
            if (_isDisposed.GetAndSet(true))
                return;

            DisposeCore(disposing);

            try
            {
                _tracker?.Invoke();
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger(GetType()).Warn(e, "Error on Execute Dispose Tracker");
            }
            finally
            {
                _tracker = null;
            }
        }

        protected abstract void DisposeCore(bool disposing);
    }
}