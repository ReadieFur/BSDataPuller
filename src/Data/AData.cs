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

        public virtual void Initialize()
        {
            Plugin.Logger.Debug($"Initialize {GetType().Name}.");
            Reset();
        }

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

        internal AData() => Initialize();

        internal virtual void Send() => OnUpdate?.Invoke(ToJson());

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
            //Using attributes for this process comes with its own set of flaws, some of which are annoying to work around.
            //I would've used reflection only and taken the default values upon construction but I was also getting an odd error there that.
            //I couldn't be bothered to figure out how to fix right now, in the future I may come back to that to fix it.
            if (memberInfo.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultValueAttribute)
                defaultValue = defaultValueAttribute.Value;
            else
                //Use the default value for the type.
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
    }
}
