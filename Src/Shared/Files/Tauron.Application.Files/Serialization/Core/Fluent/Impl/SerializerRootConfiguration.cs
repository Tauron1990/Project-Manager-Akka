namespace Tauron.Application.Files.Serialization.Core.Fluent.Impl
{
    internal abstract class SerializerRootConfigurationBase : ISerializerRootConfiguration
    {
        public TypedSerializer<TType> Apply<TType>() where TType : class => new(ApplyInternal());

        public abstract ISerializer ApplyInternal();

        protected void VerifyErrors(ISerializer serializer)
        {
            var aggregateException = serializer.Errors;
            if (aggregateException != null) throw aggregateException;
        }
    }
}