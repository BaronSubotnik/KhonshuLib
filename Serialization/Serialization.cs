using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Khonshu.Serialization
{
    public sealed class Serialization<T>
    {
        private readonly T ItemSerialized;
        private readonly String Path;

        private JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,

        };

        public Serialization(T itemSerialized)
        {
            this.ItemSerialized = itemSerialized;
        }
        public Serialization(T itemSerialized, String path)
        {
            this.ItemSerialized = itemSerialized;
            this.Path = path;
        }

        public void SerializInFile()
        {
            if (!File.Exists(Path))
            {
                File.Create(Path).Dispose();
            }
            String json = JsonSerializer.Serialize<T>(ItemSerialized, options);
            File.WriteAllText(Path, json);
        }
        public async Task SerializInFileAsync()
        {
            if (!File.Exists(Path))
            {
                File.Create(Path).Dispose();
            }

            


            

        }
    }
}
