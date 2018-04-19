using System.IO;
using System.Xml.Serialization;

namespace PS.Build.Nuget.Shared.Extensions
{
    public static class SerializationExtensions
    {
        #region Static members

        public static T LoadXml<T>(this string path)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StreamReader(path))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static void SaveXml<T>(this T instance, string path)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, instance);
            }
        }

        public static string SerializeXml<T>(this T instance)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, instance);
                return textWriter.ToString();
            }
        }

        #endregion
    }
}