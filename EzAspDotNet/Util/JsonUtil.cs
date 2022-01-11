using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EzAspDotNet.Util
{
    public static class JsonUtil
    {
        static private JsonSerializer JsonSerializer = new JsonSerializer();

        static public T Populate<T>(this JObject extensionData) where T : new()
        {
            var value = new T();
            if (extensionData != null)
            {
                JsonSerializer.Populate(extensionData.CreateReader(), value);
            }
            return value;
        }

        public static Target ConvertTo<Target, Source>(this Source source) where Source : class
        {
            var deserialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<Target>(deserialized);
        }
    }
}
