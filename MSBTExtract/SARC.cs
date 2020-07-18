using System;
using System.Collections.Generic;

namespace MSBTExtract
{
    public struct SARC
    {
        private struct SARCNode
        {
            public uint Start;
            public uint End;
        }
        
        public static Dictionary<string, byte[]> Extract(byte[] inFile)
        {
            var pos = 12;
            var dataOffset = BitConverter.ToUInt32(inFile, pos);
            pos += 8;

            if (inFile[pos] != 'S' || inFile[pos + 1] != 'F' || inFile[pos + 2] != 'A' || inFile[pos + 3] != 'T')
            {
                return null;
            }

            pos += 6;
            var nodeCount = BitConverter.ToUInt16(inFile, pos);
            pos += 6;

            var nodes = new SARCNode[nodeCount];
            var tempNode = new SARCNode();

            for (var i = 0; i < nodeCount; i++)
            {
                tempNode.Start = BitConverter.ToUInt32(inFile, pos + 8);
                tempNode.End = BitConverter.ToUInt32(inFile, pos + 12);
                pos += 16;
                nodes[i] = tempNode;
            }

            pos += 8;

            var fileNames = new string[nodeCount];
            for (var i = 0; i < nodeCount; i++)
            {
                var tempName = "";
                while (inFile[pos] != 0)
                {
                    tempName += ((char) inFile[pos]).ToString(); 
                    pos += 1;
                }

                while (inFile[pos] == 0) 
                {
                    pos += 1;
                }

                fileNames[i] = tempName; 
            }

            var dicOut = new Dictionary<string, byte[]>();
            for (var i = 0; i < nodeCount; i++)
            {
                var dataArray = new byte[(int) (nodes[i].End - nodes[i].Start)];
                Array.Copy(inFile, (int) (nodes[i].Start + dataOffset), dataArray, 0, (int) (nodes[i].End - nodes[i].Start));
                dicOut.Add(fileNames[i], dataArray);
            }

            return dicOut;
        }
    }
}
