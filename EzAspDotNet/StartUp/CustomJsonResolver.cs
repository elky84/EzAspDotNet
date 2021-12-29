using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace EzAspDotNet.StartUp
{
    public class CustomJsonResolver : CamelCasePropertyNamesContractResolver
    {
        public override JsonContract ResolveContract(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return new DefaultContractResolver().ResolveContract(type);
            }
            else
            {
                return base.ResolveContract(type);
            }
        }

    }
}
