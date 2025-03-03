using System;
using System.Text;

namespace bdmanager
{
    public static class SimpleJsonSerializer
    {
        public static string Serialize(object obj, bool indent = true)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string json = serializer.Serialize(obj);
            
            return json;
        }
        
        public static T Deserialize<T>(string json) where T : new()
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Deserialize<T>(json);
        }
    }
} 