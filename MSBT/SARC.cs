using System;
using System.Collections.Generic;

namespace MSBTTools
{
    public struct SARC
    {
        public static Dictionary<string, MSBT> Extract(byte[] bytes)
        {
            var pos = 12;
            var dataOffset = BitConverter.ToUInt32(bytes, pos);
            pos += 14;
            var nodeCount = BitConverter.ToUInt16(bytes, pos);
            pos += 6;

            var nodes = new uint[nodeCount][];

            for (var i = 0; i < nodeCount; i++)
            {
                nodes[i] = new uint[2];
                nodes[i][0] = BitConverter.ToUInt32(bytes, pos + 8);
                nodes[i][1] = BitConverter.ToUInt32(bytes, pos + 12);
                
                pos += 16;
            }

            pos += 8;

            var fileNames = new string[nodeCount];
            for (var i = 0; i < nodeCount; i++)
            {
                var tempName = "";
                while (bytes[pos] != 0)
                {
                    tempName += ((char) bytes[pos]).ToString(); 
                    pos += 1;
                }

                while (bytes[pos] == 0) 
                {
                    pos += 1;
                }

                fileNames[i] = tempName; 
            }

            var dicOut = new Dictionary<string, MSBT>();
            for (var i = 0; i < nodeCount; i++)
            {
                var dataArray = new byte[(int) (nodes[i][1] - nodes[i][0])];
                Array.Copy(bytes, (int) (nodes[i][0] + dataOffset), dataArray, 0, (int) (nodes[i][1] - nodes[i][0]));
                // Console.WriteLine(fileNames[i]);
                dicOut.Add(fileNames[i], MSBT.Open(dataArray));
            }

            return dicOut;
        }
    }
}
