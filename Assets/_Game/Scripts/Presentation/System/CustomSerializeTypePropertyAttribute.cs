using System;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomSerializeTypePropertyAttribute : SerializeTypePropertyAttribute
    {
        public Type Description { get; }

        public CustomSerializeTypePropertyAttribute(Type description)
        {
            Description = description;
        }
    }
}