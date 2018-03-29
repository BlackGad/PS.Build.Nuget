using System;
using System.Linq;
using System.Reflection;
using PS.Build.Nuget.Shared.Extensions;

namespace PS.Build.Nuget.Decryptor
{
    public class Unpacker : MarshalByRefObject
    {
        #region Members

        public byte[] Unpack(string filePath)
        {
            var fakeAssembly = Assembly.LoadFrom(filePath);
            var resourceName = fakeAssembly.GetManifestResourceNames().FirstOrDefault();
            if (string.IsNullOrWhiteSpace(resourceName)) throw new ArgumentException("Fake assembly does not contains any resources");
            using (var stream = fakeAssembly.GetManifestResourceStream(resourceName))
            {
                return stream.ReadStream();
            }
        }

        #endregion
    }
}