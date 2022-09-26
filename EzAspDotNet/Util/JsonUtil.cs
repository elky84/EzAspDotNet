using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EzAspDotNet.Util
{
    public static class JsonUtil
    {
        private static readonly JsonSerializer JsonSerializer = new();

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
