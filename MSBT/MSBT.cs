using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSBTTools
{
    public struct MSBT
    {
        public TXT TXTData;
        public LBL LBLData;

        public KeyValuePair<string, string>[] Sorted;
        
        public static async Task<MSBT> Open(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException();
            }

            var bytes = await File.ReadAllBytesAsync(filename);

            return Open(bytes);
        }

        public static MSBT Open(byte[] bytes)
        {
            var id = Encoding.UTF8.GetString(bytes, 0, 8);
            if (id != "MsgStdBn")
            {
                throw new InvalidDataException("File is not MSBT format");
            }

            var labels = new LBL(bytes);

            var lblPad = 16 - labels.Length % 16;
            if (lblPad == 16)
            {
                lblPad = 0;
            }

            var atrOffset = (int)(labels.Length + 32 + 16 + lblPad);

            var atrLength = BitConverter.ToUInt32(bytes, atrOffset + 4);
            var artPad = 16 - atrLength % 16;
            if (artPad == 16)
            {
                artPad = 0;
            }

            var txtOffset = (int)(atrOffset + atrLength + 16 + artPad);

            var text = new TXT(bytes, txtOffset);

            var msbt = new MSBT() {LBLData = labels, TXTData = text};
            var sort = new Dictionary<string, string>();

            for (var i = 0; i < labels.Labels.Length; i++)
            {
                sort.Add(labels.Labels[i].Name, text.Texts[i].Value);
            }

            msbt.Sorted = sort.OrderBy(k => k.Key).ToArray();

            return msbt;
        }
    }
}
