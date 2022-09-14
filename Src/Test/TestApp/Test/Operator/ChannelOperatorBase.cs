using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestApp.Test.Operator;

public abstract class ChannelOperatorBase<TIn> : ChannelOperatorBase<TIn, TIn>
{
    protected ChannelOperatorBase(ChannelReader<TIn> reader) : base(reader) { }
}

public abstract class ChannelOperatorBase<TIn, TOut> : ChannelReader<TOut>
{
    protected ChannelReader<TIn> Reader { get; }

    protected ChannelOperatorBase(ChannelReader<TIn> reader)
        => Reader = reader;

    public override Task Completion => Reader.Completion;

    public override int Count => Reader.Count;

    public override bool CanCount => CanCountCore && Reader.CanCount;
    
    protected abstract bool CanCountCore { get; } 
}