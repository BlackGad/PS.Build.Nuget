using System.IO;
using System.Xml.Serialization;

namespace PS.Build.Nuget.Extensions
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

        public static void SaveXml<T>(this T file, string path)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, file);
            }
        }

        #endregion
    }
}