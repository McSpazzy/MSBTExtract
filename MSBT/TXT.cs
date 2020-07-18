using System;
using System.IO;
using System.Text;

namespace MSBTTools
{
    public struct TXT
    {
        public uint Length;
        public Text[] Texts;

        public struct Text
        {
            public byte[] Data;
            public string Value;

            public Text(byte[] data)
            {
                Data = data;
                Value = Encoding.Unicode.GetString(MSBTFunctions.ReplaceFunctions(data)).TrimEnd('\0').Replace("\n", "\\n").Replace("\r", "\\r");
            }

            public override string ToString()
            {
                return $"{Value}";
            }
        }

        public TXT(byte[] data, int offset)
        {
            if (Encoding.UTF8.GetString(data, offset, 4) != "TXT2")
            {
                throw new InvalidDataException("Missing TXT");
            }

            Length = BitConverter.ToUInt32(data, offset + 4);

            var count = BitConverter.ToUInt32(data, offset + 16);
            Texts = new Text[count];

            for (var i = 0; i < count; i++)
            {
                var textOffset = BitConverter.ToUInt32(data, offset + 4 + 16 + i * 4);
                var nextOffset = BitConverter.ToUInt32(data, offset + 4 + 16 + 4 + i * 4);
                if (i == count - 1)
                {
                    nextOffset = Length;
                }

                var length = nextOffset - textOffset;

                var start = offset + 16 + (int)textOffset;

                var ssd = new byte[length];

                Array.Copy(data, start, ssd, 0, length);

                Texts[i] = new Text(ssd);
            }
        }
    }
}
