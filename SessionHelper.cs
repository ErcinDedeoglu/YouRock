using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace YouRock
{
    public static class SessionHelper
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            if ((typeof(T) == typeof(int?) || typeof(T) == typeof(int)) && value != null)
            {
                session.SetInt32(key, (int)(object)value);
            }
            else
            {
                session.SetString(key, JsonConvert.SerializeObject(value));
            }
        }

        public static T Get<T>(this ISession session, string key)
        {
            object result;

            if (typeof(T) == typeof(int?) || typeof(T) == typeof(int))
            {
                result = session.GetInt32(key);
            }
            else
            {
                result = JsonConvert.DeserializeObject<T>(session.GetString(key));
            }

            return (T) result;
        }
    }
}