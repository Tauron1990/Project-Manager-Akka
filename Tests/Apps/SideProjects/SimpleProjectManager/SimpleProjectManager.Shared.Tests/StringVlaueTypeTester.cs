namespace SimpleProjectManager.Shared.Tests;

public abstract class StringVlaueTypeTester<TType>
    where TType : IStringValueType<TType>
{
    public virtual void Not_Null_Valid_Value(string value)
    {
        TType msg = TType.From(value);
        
        Assert.True(msg == value, "msg == value; operator");
        Assert.Equal(value, msg.Value);
    }

    public virtual void Null_InValid_Value()
        => Assert.Throws<Vogen.ValueObjectValidationException>(() => TType.From(null));

    public virtual void Empty_InValid_Value()
        => Assert.Throws<Vogen.ValueObjectValidationException>(() => TType.From(string.Empty));
    
    public virtual void Default_InValid_Value()
        #pragma warning disable VOG009
        => Assert.Throws<Vogen.ValueObjectValidationException>(() => default(TType).Value);
    #pragma warning restore VOG009

    public virtual void Empty_Equal_Value()
    {
        Assert.Equal(string.Empty, TType.GetEmpty.Value);
        Assert.True(TType.GetEmpty == string.Empty, "SimpleMessage.Empty == string.Empty; operator");
    }
}