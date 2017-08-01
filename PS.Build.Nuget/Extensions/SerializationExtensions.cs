using System.IO;
using System.Xml.Serialization;

namespace PS.Build.Nuget.Extensions
{
    public static class SerializationExtensions
    {
        #region Static members

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