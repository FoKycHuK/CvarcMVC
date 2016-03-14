using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace CVARC.Infrastructure
{
    //этот класс -- полная копия JSonSerializer из Cvarc.Core
    //я боюсь переносить его сюда и юзать в коре.
    //к тому же, это сейчас невозможно из-за циклической зависимости :(
    public static class Serializer
    {
        public static void Serialize(object obj, Stream stream)
        {
            new DataContractJsonSerializer(obj.GetType()).WriteObject(stream, obj);
        }

        public static object Deserialize(Type type, Stream stream)
        {
                return new DataContractJsonSerializer(type).ReadObject(stream);
        }
    }
}
