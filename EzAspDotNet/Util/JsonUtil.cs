using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebShared.Util
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
    }
}
