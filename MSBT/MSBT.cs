using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MSBTTools
{
    public class MSBT
    {
        public Header HeaderData;
        public TXT TXTData;
        public LBL1 LBLData;

        public class Header
        {
            public Header(byte[] data)
            {
                var id = Encoding.UTF8.GetString(data, 0, 8);
                if (id != "MsgStdBn")
                {
                    throw new InvalidDataException("File is not MSBT format");
                }
            }
        }

        public class LBL1
        {
            public List<Label> Labels;
            public uint Length;

            public class Group
            {
                public int Labels;
                public uint Offset;

                public Group(int labels, uint offset)
                {
                    Labels = labels;
                    Offset = offset;
                }
            }

            public class Label
            {
                public string Name;
                public uint Index;
                public int GroupIndex;

                public Label(string name, uint index, int group)
                {
                    Name = name;
                    Index = index;
                    GroupIndex = group;
                }

                public override string ToString()
                {
                    return $"{Name} | {Index} | {GroupIndex}";
                }
            }

            public LBL1(byte[] data)
            {
                if (Encoding.UTF8.GetString(data, 32, 4) != "LBL1")
                {
                    throw new InvalidDataException("Missing LBL1");
                }

                Labels = new List<Label>();
                Length = BitConverter.ToUInt32(data, 32 + 4);

                for (var i = 0; i < BitConverter.ToUInt32(data, 32 + 16); i++)
                {
                    var g = new Group(BitConverter.ToInt32(data, 32 + 20 + i * 8), BitConverter.ToUInt32(data, 32 + 24 + i * 8));

                    var nameOffset = 0;
                    for (var j = 0; j < g.Labels; j++)
                    {
                        var labelOffset = 32 + 16 + g.Offset + nameOffset + 1 + j * 5;
                        var length = data[labelOffset - 1];
                        var name = Encoding.UTF8.GetString(data, (int)labelOffset, length);
                        var index = BitConverter.ToUInt32(data, (int)(labelOffset + length));
                        nameOffset += length;
                        Labels.Add(new Label(name, index, i));
                    }
                }
            }
        }

        public class TXT
        {
            public uint Length;
            public List<Text> Texts;

            public class Text
            {
                public byte[] Data;
                public string Value;

                public Text(byte[] data)
                {
                    Data = data;
                    Value = Encoding.Unicode.GetString(data);
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

                Texts = new List<Text>();
                Length = BitConverter.ToUInt32(data, offset + 4);

                var count = BitConverter.ToUInt32(data, offset + 16);

                for (var i = 0; i < count; i++)
                {
                    var textOffset = BitConverter.ToUInt32(data, offset + 4 + 16 + i * 4);
                    var nextOffset = BitConverter.ToUInt32(data, offset + 4 + 16 + 4 + i * 4);
                    if (i == count - 1)
                    {
                        nextOffset = Length;
                    }

                    var length = nextOffset - textOffset;

                    var start = offset + 16 + (int) textOffset;

                    var ssd = new byte[length];

                    Array.Copy(data, start, ssd, 0, length);

                    Texts.Add(new Text(ssd));
                }
            }
        }

        public static async Task<MSBT> Open(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException();
            }

            var bytes = await File.ReadAllBytesAsync(filename);

            var header = new Header(bytes);
            var labels = new LBL1(bytes);

            var lblPad = 16 - labels.Length % 16;

            var atrOffset = (int)(labels.Length + 32 + 16 + lblPad);

            var atrLength = BitConverter.ToUInt32(bytes, atrOffset + 4);
            var artPad = 16 - atrLength % 16;

            var txtOffset = (int)(atrOffset + atrLength + 16 + artPad);

            var text = new TXT(bytes, txtOffset);

            return new MSBT(){HeaderData = header, LBLData = labels, TXTData = text};
        }
    }
}
