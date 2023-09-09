using Newtonsoft.Json;
using System.Dynamic;

namespace StarLaiPortal.WebApi.Converters
{
    public class ExpandoObjectJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) => objectType == typeof(ExpandoObject) || typeof(ICollection<ExpandoObject>).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(ExpandoObject))
            {
                ExpandoObject obj = new ExpandoObject();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        string propertyName = (string)reader.Value;
                        reader.Read();

                        object value = GetReaderValue(reader, serializer);
                        obj.TryAdd(propertyName, value);
                    }
                    else if (reader.TokenType == JsonToken.EndObject)
                    {
                        break;
                    }
                }

                return obj;
            }

            ICollection<ExpandoObject> list = Activator.CreateInstance(objectType) as ICollection<ExpandoObject>;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var value = serializer.Deserialize<ExpandoObject>(reader);
                    list.Add(value);
                }
                else if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Should not get here.");
        }

        private static object GetReaderValue(JsonReader reader, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return serializer.Deserialize<ExpandoObject>(reader);
                case JsonToken.StartArray:
                    List<object> obj_list = new List<object>();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.EndArray) break;

                        obj_list.Add(GetReaderValue(reader, serializer));
                    }

                    return obj_list;
                default:
                    return serializer.Deserialize(reader);
            }
        }
    }
}
