﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Tauron;

/// <summary>
///     A class which calculates progress statistics like average bytes per second or estimated finishing time.
///     To use it, call the ProgressChange method in regular intervals with the actual progress.
/// </summary>
[PublicAPI]
public class ProgressStatistic
{
    public ProgressStatistic()
    {
        StartingTime = DateTime.MinValue;
        FinishingTime = DateTime.MinValue;

        _progressChangedArgs = new ProgressEventArgs(this); //Event args can be cached
    }

    /// <summary>
    ///     Gets whether the operation has started
    /// </summary>
    public bool HasStarted { get; private set; }

    /// <summary>
    ///     Gets whether the operation has finished
    /// </summary>
    public bool HasFinished => FinishingTime != DateTime.MinValue;

    /// <summary>
    ///     Gets whether the operation is still running
    /// </summary>
    public bool IsRunning => HasStarted && !HasFinished;

    /// <summary>
    ///     Gets the amount of bytes already read.
    /// </summary>
    public long BytesRead { get; private set; }

    /// <summary>
    ///     Gets the amount of total bytes to read. Can be -1 if unknown.
    /// </summary>
    public long TotalBytesToRead { get; private set; }

    /// <summary>
    ///     Gets the progress in percent between 0 and 1.
    ///     If the amount of total bytes to read is unknown, -1 is returned.
    /// </summary>
    public double Progress => TotalBytesToRead == -1 ? -1 : BytesRead / (double)TotalBytesToRead;

    /// <summary>
    ///     Gets the average bytes per second.
    /// </summary>
    public double AverageBytesPerSecond => BytesRead / Duration.TotalSeconds;

    /// <summary>
    ///     This method can be called to report progress changes.
    ///     The signature of this method is compliant with the ProgressChange-delegate
    /// </summary>
    /// <param name="bytesRead">The amount of bytes already read</param>
    /// <param name="totalBytesToRead">The amount of total bytes to read. Can be -1 if unknown.</param>
    /// <exception cref="ArgumentException">Thrown if bytesRead has not changed or even shrunk.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the operation has finished already.</exception>
    public virtual void ProgressChange(long bytesRead, long totalBytesToRead)
    {
        if(!HasStarted)
        {
            StartingTime = DateTime.Now;
            HasStarted = true;
            OnStarted();
        }

        BytesRead = bytesRead;
        TotalBytesToRead = totalBytesToRead;

        ProcessSample(bytesRead);

        OnProgressChanged();

        if(bytesRead != TotalBytesToRead) return;

        FinishingTime = DateTime.Now;
        OnFinished();
    }

    /// <summary>
    ///     This method can be called to finish an aborted operation.
    ///     If the operation does not reach 100%, "Finished" will be never raised, so this method should be called.
    /// </summary>
    public virtual void Finish()
    {
        if(HasFinished) return;

        FinishingTime = DateTime.Now;
        OnFinished();
    }

    #region Time

    /// <summary>
    ///     Gets the date time when the operation has started
    /// </summary>
    public DateTime StartingTime { get; private set; }

    /// <summary>
    ///     Gets the date time when the operation has finished
    /// </summary>
    public DateTime FinishingTime { get; private set; }

    /// <summary>
    ///     Gets the duration of the operation.
    ///     If the operation is still running, the time since starting is returned.
    ///     If the operation has not started, TimeSpan.Zero is returned.
    ///     If the operation has finished, the time between starting and finishing is returned.
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            if(!HasStarted)
                return TimeSpan.Zero;
            if(!HasFinished)
                return DateTime.Now - StartingTime;

            return FinishingTime - StartingTime;
        }
    }

    /// <summary>
    ///     The method which will be used for estimating duration and finishing time
    /// </summary>
    public enum EstimatingMethod
    {
        /// <summary>
        ///     Current bytes per second will be used for estimating.
        /// </summary>
        CurrentBytesPerSecond,

        /// <summary>
        ///     Average bytes per second will be used for estimating
        /// </summary>
        AverageBytesPerSecond,
    }

    /// <summary>
    ///     Gets or sets which method will be used for estimating.
    ///     Can only be set before the operation has started, otherwise an OperationAlreadyStartedException will be thrown.
    /// </summary>
    public EstimatingMethod UsedEstimatingMethod { get; set; } = EstimatingMethod.CurrentBytesPerSecond;

    /// <summary>
    ///     Gets the estimated duration. Use UsedEstimatingMethod to specify which method will be used for estimating.
    ///     If the operation will take more than 200 days, TimeSpan.MaxValue is returned.
    /// </summary>
    public TimeSpan EstimatedDuration
    {
        get
        {
            if(HasFinished)
                return Duration;
            if(TotalBytesToRead == -1)
                return TimeSpan.MaxValue;

            double bytesPerSecond = UsedEstimatingMethod switch
            {
                EstimatingMethod.AverageBytesPerSecond => AverageBytesPerSecond,
                EstimatingMethod.CurrentBytesPerSecond => CurrentBytesPerSecond,
                _ => throw new InvalidOperationException("No Correct Estimating method"),
            };

            double seconds = (TotalBytesToRead - BytesRead) / bytesPerSecond;

            if(seconds > 60 * 60 * 24 * 200) //over 200 Days -> infinite
                return TimeSpan.MaxValue;

            return Duration + TimeSpan.FromSeconds(seconds);
        }
    }

    /// <summary>
    ///     Gets the estimated finishing time based on EstimatedDuration.
    ///     If the operation will take more than 200 days, DateTime.MaxValue is returned.
    ///     If the operation has finished, the actual finishing time is returned.
    /// </summary>
    public DateTime EstimatedFinishingTime => EstimatedDuration == TimeSpan.MaxValue
        ? DateTime.MaxValue
        : StartingTime + EstimatedDuration;

    #endregion

    #region CurrentBytesPerSecond

    /// <summary>
    ///     Gets the approximated current count of bytes processed per second
    /// </summary>
    public double CurrentBytesPerSecond { get; private set; }


    private TimeSpan _currentBytesCalculationInterval = TimeSpan.FromSeconds(0.5);

    /// <summary>
    ///     Gets or sets the interval used for the calculation of the current bytes per second. Default is 500 ms.
    /// </summary>
    /// <exception cref="OperationAlreadyStartedException">
    ///     Thrown when trying to set although the operation has already started.
    /// </exception>
    public TimeSpan CurrentBytesCalculationInterval
    {
        get => _currentBytesCalculationInterval;
        set
        {
            if(HasStarted)
                throw new InvalidOperationException("Task has already started!");

            _currentBytesCalculationInterval = value;
        }
    }

    private KeyValuePair<DateTime, long>[] _currentBytesSamples = new KeyValuePair<DateTime, long>[6];

    /// <summary>
    ///     Gets or sets the number of samples in CurrentBytesPerSecondInterval used for current bytes per second approximation
    /// </summary>
    /// <exception cref="OperationAlreadyStartedException">
    ///     Thrown when trying to set although the operation has already started.
    /// </exception>
    public int CurrentBytesSampleCount
    {
        get => _currentBytesSamples.Length;
        set
        {
            if(HasStarted)
                throw new InvalidOperationException("Task has already started!");

            if(value != _currentBytesSamples.Length)
                _currentBytesSamples = new KeyValuePair<DateTime, long>[value];
        }
    }


    private int _currentSample; //current sample index in currentBytesSamples

    private DateTime _lastSample;

    private void ProcessSample(long bytes)
    {
        if((DateTime.Now - _lastSample).Ticks <=
           CurrentBytesCalculationInterval.Ticks / _currentBytesSamples.Length) return;

        _lastSample = DateTime.Now;

        var current = new KeyValuePair<DateTime, long>(DateTime.Now, bytes);

        var old = _currentBytesSamples[_currentSample];
        _currentBytesSamples[_currentSample] = current;

        if(old.Key == DateTime.MinValue)
            CurrentBytesPerSecond = AverageBytesPerSecond;
        else
            CurrentBytesPerSecond = (current.Value - old.Value) / (current.Key - old.Key).TotalSeconds;

        _currentSample++;
        if(_currentSample >= _currentBytesSamples.Length)
            _currentSample = 0;
    }

    #endregion

    #region Events

    private readonly ProgressEventArgs _progressChangedArgs;

    protected virtual void OnStarted() => _started.OnNext(_progressChangedArgs);

    protected virtual void OnProgressChanged() => _progressChanged.OnNext(_progressChangedArgs);

    protected virtual void OnFinished()
    {
        _finisht.OnNext(_progressChangedArgs);

        _started.OnCompleted();
        _progressChanged.OnCompleted();
        _finisht.OnCompleted();

        _started.Dispose();
        _progressChanged.Dispose();
        _finisht.Dispose();
    }

    /// <summary>
    ///     Will be raised when the operation has started
    /// </summary>
    public IObservable<ProgressEventArgs> WhenStarted => _started.AsObservable();

    private readonly Subject<ProgressEventArgs> _started = new();

    /// <summary>
    ///     Will be raised when the progress has changed
    /// </summary>
    public IObservable<ProgressEventArgs> WhenProgressChanged => _progressChanged.AsObservable();

    private readonly Subject<ProgressEventArgs> _progressChanged = new();

    /// <summary>
    ///     Will be raised when the operation has finished
    /// </summary>
    public IObservable<ProgressEventArgs> WhenFinished => _finisht.AsObservable();

    private readonly Subject<ProgressEventArgs> _finisht = new();

    #endregion
}