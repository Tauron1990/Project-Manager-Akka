using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Tauron.Application;

[DebuggerNonUserCode]
[Serializable]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class GenericWeakReference<TType> : WeakReference, IEquatable<GenericWeakReference<TType>>, IWeakReference
    where TType : class
{
    public GenericWeakReference(TType target)
        : base(target) { }

    private GenericWeakReference(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    public TType? TypedTarget
    {
        get => Target as TType;
        set => Target = value;
    }

    public bool Equals(GenericWeakReference<TType>? other)
    {
        object? t1 = Target;
        object? t2 = other?.Target;

        return t1?.Equals(t2) ?? t2 is null;
    }

    public override bool Equals(object? obj)
    {
        while (true)
        {
            object? target = Target;
            object? temp = obj as GenericWeakReference<TType>;
            if(temp != null)
            {
                obj = temp;

                continue;
            }

            return target?.Equals(obj) ?? obj is null;
        }
    }

    public override int GetHashCode()
    {
        object? target = Target;

        return target is null ? 0 : target.GetHashCode();
    }

    public static bool operator ==(GenericWeakReference<TType> left, GenericWeakReference<TType> right)
        => Equals(left, right);

    public static bool operator !=(GenericWeakReference<TType> left, GenericWeakReference<TType> right)
        => !Equals(left, right);

    public static bool operator ==(GenericWeakReference<TType> left, object right) => Equals(left, right);

    public static bool operator !=(GenericWeakReference<TType> left, object right) => !Equals(left, right);
}