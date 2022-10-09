using System;
using System.Reflection;
using DataPuller.Attributes;
using Newtonsoft.Json;
using Zenject;

#nullable enable
namespace DataPuller.Data
{
    //TODO: Impliment zenject (check AppInstallers.cs for more information).
    public abstract class AData //: IInitializable
    {
#if false
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [JsonIgnore] [Inject] public static AData Instance { get; protected set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#endif

        public event Action<string>? OnUpdate;

        [JsonProperty] public long UnixTimestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        internal AData() => Initialize();

        public virtual void Send() => OnUpdate?.Invoke(ToJson());
        
        public virtual string ToJson() =>
            JsonConvert.SerializeObject(this, Formatting.None);

        public virtual void Reset()
        {
            Type type = GetType();
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            foreach (FieldInfo field in type.GetFields(bindingFlags))
                ProcessMemberInfo(field);
            foreach (PropertyInfo property in type.GetProperties(bindingFlags))
                ProcessMemberInfo(property);
        }

        protected virtual void ProcessMemberInfo(MemberInfo memberInfo)
        {
            //Get the fields type.
            Type type = memberInfo switch
            {
                FieldInfo field => field.FieldType,
                PropertyInfo property => property.PropertyType,
                _ => throw new ArgumentException("MemberInfo must be of type FieldInfo or PropertyInfo", nameof(memberInfo))
            };

            //Make sure the member is settable
            if ((memberInfo is PropertyInfo _property && !_property.CanWrite) || (memberInfo is FieldInfo _field && _field.IsInitOnly))
                return;

            object? defaultValue;
            //Check if the field has a DefaultValue attribute.
            if (memberInfo.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute)
                defaultValue = defaultValueAttribute.Value;
            else //If a custom default value is not set, use the default value for the type.
                defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;

            //Set the value of the member.
            switch (memberInfo)
            {
                case FieldInfo field:
                    field.SetValue(this, defaultValue);
                    break;
                case PropertyInfo property:
                    property.SetValue(this, defaultValue);
                    break;
            }
        }

        public virtual void Initialize()
        {
            Plugin.Logger.Debug($"Initialize {GetType().Name}.");
            Reset();
        }
    }
}
