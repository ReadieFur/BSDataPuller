using System;
using System.Reflection;
using DataPuller.Attributes;
using Newtonsoft.Json;

/*XML comment structure:        
/// <summary></summary>
/// <remarks></remarks>
/// <value>Default is <see href=""/>.</value>
*/

#nullable enable
namespace DataPuller.Data
{
    public abstract class AData
    {
        #region Properties
        /// <summary>The event that is fired when data is updated.</summary>
        /// <remarks>This event gets fired manually.</remarks>
        public event Action<string>? OnUpdate;

        /// <summary>The time that the data was serialized.</summary>
        /// <remarks></remarks>
        /// <value><see cref="DateTimeOffset.UtcNow"/> in milliseconds since unix.</value>
        [JsonProperty] public long UnixTimestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        #endregion

        #region Methods
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
        #endregion
    }
}
