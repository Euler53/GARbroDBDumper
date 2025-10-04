using GameRes;
using GameRes.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace SchemeDumper
{
    public class ForceAllFieldsResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (!props.Any(p => p.PropertyName == field.Name))
                {
                    var prop = base.CreateProperty(field, memberSerialization);

                    prop.Readable = true; 

                    props.Add(prop);
                }
            }

            return props;
        }
    }

    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                using (Stream stream = File.OpenRead(".\\GameData\\Formats.dat"))
                {
                    FormatCatalog.Instance.GetSerializedSchemeVersion(stream);

                    using (ZLibStream zs = new ZLibStream(stream, CompressionMode.Decompress, true))
                    {
                        var bin = new BinaryFormatter();
                        var db = (SchemeDataBase)bin.Deserialize(zs);

                        var settings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            ContractResolver = new ForceAllFieldsResolver(),
                            Formatting = Formatting.Indented
                        };

                        string json = JsonConvert.SerializeObject(db, settings);
                        File.WriteAllText(".\\GameData\\Formats.json", json);
                    }
                }
                Console.WriteLine("Scheme dumped successfully (including all fields).");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("An error occurred: " + ex.Message);
            }

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
