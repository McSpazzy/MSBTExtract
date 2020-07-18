using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MSBTTools;

namespace MSBTExtract
{
    public class Program
    {
        static async Task Main(string[] args)
        {

           var msbt = await MSBT.Open(@"E:\E:\Messageold\TalkNNpc_EUen\B1_Bo\Quest\BO_Quest_TreasureHunt_Begin.msbt");



        }

        private static byte[] DeZip(byte[] data)
        {
            using var decompress = new ZstdNet.Decompressor();
            return decompress.Unwrap(data);
        }
    }
}
