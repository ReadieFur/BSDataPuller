namespace DataPuller.Attributes
{
    public class DefaultValueTAttribute<T> : DefaultValueAttribute where T : new()
    {
        public DefaultValueTAttribute() : base(new T()) {}
    }
}
