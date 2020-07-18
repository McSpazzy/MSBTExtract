using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MSBTTools
{
    public class MSBTFunctions
    {
        public static string[] Article = { "a", "an", "the", "some" };

        public static Dictionary<byte, string> Function = new Dictionary<byte, string>()
        {
            {0x6E, "GetString"},
            {0x5A, "GetNumber"}
        };

        public static Dictionary<ushort, string> PlayerStringName = new Dictionary<ushort, string>()
        {
            {0, "..."},
            {1, "{PlayerName}"},
            {3, "{Nickname}"},
            {5, "{Catchphrase}"},
            {8, "{OtherIsland}"},
            {9, "{Island}"}
        };

        public static Dictionary<ushort, string> TextModName = new Dictionary<ushort, string>()
        {
            {0, "Furigana"},
            {2, "TextSize"},
            {3, "TextColor"}
        };

        public static Dictionary<ushort, string> NumberName = new Dictionary<ushort, string>()
        {
            {2, "Units"},
            {17, "Bells"},
            {20, "TurnipBuyPrice"},
            {21, "OfferPrice"},
            {26, "BuyPrice"},
            {34, "RelativeYear"},
            {35, "RelativeMonth"},
            {36, "RelativeDay"},
            {37, "RelativeHour"},
            {38, "RelativeMinute"},
            {0x69, "HHAPoints"}
        };

        public static string FunctionName(byte index) => Function.ContainsKey(index) ? Function[index] : "Unknown";
        public static string GetPlayerStringName(ushort index) => PlayerStringName.ContainsKey(index) ? PlayerStringName[index] : "Unknown";
        public static string GetNumberName(ushort index) => NumberName.ContainsKey(index) ? NumberName[index] : "Unknown";
        public static string GetTextModName(ushort index) => TextModName.ContainsKey(index) ? TextModName[index] : "Unknown";

        static MSBTFunctions()
        {

        }

        public static byte[] ReplaceFunctions(byte[] array)
        {
            var outArray = new List<byte>();
            for (var i = 0; i < array.Length; i++)
            {
                switch (array[i])
                {
                    case 0x0E: // String?
                        var func = GetFunctionString(array.SubArrayDeepClone(i, 12));
                        var str = Encoding.Unicode.GetBytes(func.Item1);
                        outArray.AddRange(str);
                        i += func.Item2 - 1;
                        break;
                    default:
                        outArray.Add(array[i]);
                        break;
                }
            }
            return outArray.ToArray();
        }

        public static Tuple<string, int> GetFunctionString(byte[] data)
        {
            var items = Array.IndexOf(data, (byte)14, 1);

            if (items > 0)
            {
                data = data.SubArrayDeepClone(0, items);
            }

            if (data.Length <= 2)
            {
                return new Tuple<string, int>("UnknownFuncX()", data.Length);
            }

            if (data.Length <= 4)
            {
                return new Tuple<string, int>($"UnknownFunc{data[2]}()", data.Length);
            }

            var val1 = BitConverter.ToUInt16(data, 4);

            if (data.Length < 8)
            {
                return new Tuple<string, int>($"UnknownFunc{data[2]}({val1})", data.Length);
            }

            var val2 = BitConverter.ToUInt16(data, 6);
            var val3 = 0;

            if (data.Length > 8)
            {
                val3 = BitConverter.ToUInt16(data, 8);
            }

            switch (data[2])
            {
                case 0x0: // Text modification
                    return new Tuple<string, int>($"TextMod({GetTextModName(val1)}, {val3})", 10);
                case 0x5A: // Assorted values
                    return new Tuple<string, int>($"Value({GetNumberName(val1)}, {val2})", 10);
                case 0x6E: // Player info
                    return new Tuple<string, int>($"{GetPlayerStringName(val1)}", 8);
                case 0x7D: // Item
                    return new Tuple<string, int>($"Item({val1}, {val2})", 10);
                case 0x32: // Language article based on STR_Article
                    return new Tuple<string, int>($"Article({val1}, {val2}, {val3})", data.Length);
                case 0x73: // Other player info
                    return new Tuple<string, int>($"String({val1}, {val2}, {val3})", data.Length);
                default:
                    break;
            }

            if (data.Length <= 8)
            {
                return new Tuple<string, int>($"UnknownFunc{data[2]}({val1}, {val2})", 8);
            }

            return new Tuple<string, int>($"UnknownFunc{data[2]}({val1}, {val2})", data[8] == 0x0e ? 8 : 10);
        }
    }

    public static class ByteArrayExtensions
    {
        public static T[] Slice<T>(this T[] arr, uint indexFrom, uint size)
        {
            T[] result = new T[size];
            Array.Copy(arr, indexFrom, result, 0, size);

            return result;
        }

        public static T[] SubArrayDeepClone<T>(this T[] data, int index, int length)
        {
            T[] arrCopy = new T[length];

            if (length + index > data.Length)
            {
                length = data.Length - index;
            }

            Array.Copy(data, index, arrCopy, 0, length);
            using (MemoryStream ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, arrCopy);
                ms.Position = 0;
                return (T[])bf.Deserialize(ms);
            }
        }
    }
}
