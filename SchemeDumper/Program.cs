using GameRes;
using GameRes.Compression;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SchemeDumper
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using (Stream stream = File.OpenRead(".\\GameData\\Formats.dat"))
            {
                int version = FormatCatalog.Instance.GetSerializedSchemeVersion(stream);
                using (ZLibStream zs = new ZLibStream(stream, CompressionMode.Decompress, true))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    SchemeDataBase db = (SchemeDataBase)bin.Deserialize(zs);
                    string json = JsonConvert.SerializeObject(db, Formatting.Indented);
                    File.WriteAllText(".\\GameData\\Formats.json", json);
                }
            }
            Console.WriteLine("Scheme dumped.");
            Console.ReadLine();
        }
    }
}
