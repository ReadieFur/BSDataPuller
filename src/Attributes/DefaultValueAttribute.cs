using System;

#nullable enable
namespace DataPuller.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DefaultValueAttribute : Attribute
    {
        public virtual object? Value { get; protected set; }

        public DefaultValueAttribute(object? value) => Value = value;
    }
}
