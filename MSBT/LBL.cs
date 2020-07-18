using System;
using System.IO;
using System.Text;

namespace MSBTTools
{
    public struct LBL
    {
        public Label[] Labels;
        public Group[] Groups;

        public uint Length;

        public struct Group
        {
            public int Labels;
            public uint Offset;

            public Group(int labels, uint offset)
            {
                Labels = labels;
                Offset = offset;
            }
        }

        public struct Label
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

        public LBL(byte[] data)
        {
            if (Encoding.UTF8.GetString(data, 32, 4) != "LBL1")
            {
                throw new InvalidDataException("Missing LBL1");
            }

            Length = BitConverter.ToUInt32(data, 32 + 4);

            Groups = new Group[BitConverter.ToUInt32(data, 32 + 16)];
            var labelCount = 0;

            for (var i = 0; i < Groups.Length; i++)
            {
                Groups[i] = new Group(BitConverter.ToInt32(data, 32 + 20 + i * 8), BitConverter.ToUInt32(data, 32 + 24 + i * 8));
                labelCount += Groups[i].Labels;
            }

            Labels = new Label[labelCount];

            for (var i = 0; i < Groups.Length; i++)
            {
                var nameOffset = 0;
                for (var j = 0; j < Groups[i].Labels; j++)
                {
                    var labelOffset = 32 + 16 + Groups[i].Offset + nameOffset + 1 + j * 5;
                    var length = data[labelOffset - 1];
                    var name = Encoding.UTF8.GetString(data, (int)labelOffset, length);
                    var index = BitConverter.ToUInt32(data, (int)(labelOffset + length));
                    nameOffset += length;
                    Labels[index] = new Label(name, index, i);
                }
            }
        }
    }
}
