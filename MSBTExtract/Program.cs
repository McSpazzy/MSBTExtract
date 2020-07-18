using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSBTTools;

namespace MSBTExtract
{
    public class Program
    {
        static async Task Main(string[] args)
        {

            var pathTest = @"E:\Messageold";

            var sw = new Stopwatch();
            sw.Start();

            var filesIn = Directory.GetFiles(pathTest, "*EUen*.zs");

            var dataOut = new Dictionary<string, byte[]>();


            foreach (var file in filesIn)
            {
                var d = await File.ReadAllBytesAsync(file);

                using var decompress = new ZstdNet.Decompressor();
                var unzip = decompress.Unwrap(d);

                var files = SARC.Extract(unzip);

                foreach (var filem in files)
                {
                    var dat = filem.Value.Sorted.SelectMany(s => Encoding.Unicode.GetBytes($"{s.Key},{s.Value}\r\n")).ToArray();
                    dataOut.Add(file.Replace(pathTest, "") + "\\" + filem.Key, dat);
                }
            }

            await using var zipToOpen = new MemoryStream();
            using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create, true);
            foreach (var file in dataOut)
            {
                var zipArchiveEntry = archive.CreateEntry(file.Key.TrimStart('\\'), CompressionLevel.NoCompression);
                await using var zipStream = zipArchiveEntry.Open();
                await zipStream.WriteAsync(file.Value, 0, file.Value.Length);
            }

            var time = sw.ElapsedMilliseconds;

            await File.WriteAllBytesAsync(@"D:\MessageTest\zip.zip", zipToOpen.ToArray());

        }
    }
}
