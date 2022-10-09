using System;
using System.Collections.Generic;

#nullable enable
namespace DataPuller.Attributes
{
    public class DefaultValueDictionaryAttribute<TKey, TValue> : DefaultValueAttribute
    {
        private static Dictionary<TKey, TValue> DefaultValue => new();

        public DefaultValueDictionaryAttribute() : base(DefaultValue) {}

        public DefaultValueDictionaryAttribute(TKey[] keys) : base(DefaultValue)
        {
            foreach (TKey key in keys) Add(key, default!);
        }

        public DefaultValueDictionaryAttribute(TKey[] keys, TValue defaultValue) : base(DefaultValue)
        {
            foreach (TKey key in keys) Add(key, defaultValue);
        }

        public DefaultValueDictionaryAttribute(TKey[] keys, TValue[] values) : base(DefaultValue)
        {
            if (keys.Length != values.Length) throw new ArgumentException("Keys and values must be the same length.");
            for (int i = 0; i < keys.Length; i++) Add(keys[i], values[i]);
        }

        private void Add(TKey key, TValue value) => ((Dictionary<TKey, TValue>)Value!).Add(key, value);
    }
}
