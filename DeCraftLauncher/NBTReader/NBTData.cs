using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeCraftLauncher.Utils;
using System.IO.Compression;

namespace DeCraftLauncher.NBTReader
{
    public class NBTData
    {
        public bool wasGzipCompressed = false;

        public abstract class NBTBase
        {
            public byte Tag;
            public string Name;

            public abstract string GetTypeName();
            public abstract string GetValue();
        }
        public class NBTNode<T> : NBTBase
        {
            public T Value;

            public override string GetTypeName() => Value.GetType().Name;
            public override string GetValue() => Value.ToString();
        }
        public class NBTTagCompoundNode : NBTNode<List<NBTBase>> { 
            public NBTTagCompoundNode()
            {
                Value = new List<NBTBase>();
            }
            public override string GetTypeName() => "Compound";
            public override string GetValue() => $"({Value.Count} elements)";
        }
        public class NBTTagListNode : NBTTagCompoundNode {
            public byte innerType;
            public override string GetTypeName() => "List";
            public override string GetValue() => $"({Value.Count} elements)";

        } //tell noone
        public class NBTTagEndCompoundNode : NBTNode<object> { }

        public NBTTagCompoundNode rootNode;

        public static void PrintNBT(NBTBase nbtNode, int stage = 0)
        {
            for (int x = 0; x < stage; x++)
            {
                Console.Write("-");
            }
            Console.Write($"> {nbtNode.Name}");

            if (nbtNode is NBTTagCompoundNode)
            {
                Console.WriteLine(" (compound)");
                foreach (NBTBase nbtTag in ((NBTTagCompoundNode)nbtNode).Value) 
                { 
                    PrintNBT(nbtTag, stage + 1);
                }
            } 
            else 
            {
                Console.WriteLine($" ({nbtNode.GetValue()})");
            }
        }

        public static NBTTagCompoundNode ReadNBTTagCompound(Stream input)
        {
            NBTTagCompoundNode ret = new NBTTagCompoundNode();
            while (true)
            {
                byte[] nextType = new byte[] { 0 };
                if (input.Read(nextType, 0, 1) == 0)
                {
                    Console.WriteLine("[ReadNBTTagCompound] input.Read() read 0 bytes, ending.");
                    break;
                }
                NBTBase nextNBT = ReadNBTTagFromStream(input, true, nextType[0]);
                if (nextNBT is NBTTagEndCompoundNode)
                {
                    break;
                } else
                {
                    ret.Value.Add(nextNBT);
                }

            }
            ret.Value = ret.Value.OrderBy(x => x.Name).ToList();
            return ret;
        }

        public static NBTBase ReadNBTTagFromStream(Stream input, bool readName = true, byte? predefType = null)
        {
            byte typeID = predefType == null ? (byte)input.ReadByte() : predefType.Value;

            readName = (typeID == 0) ? false : readName;

            short nameLength = readName ? Util.StreamReadShort(input) : (short)0;
            byte[] nameB = new byte[nameLength];
            input.Read(nameB, 0, nameLength);
            string name = readName ? Encoding.UTF8.GetString(nameB) : "";

            NBTBase newNBT = null;
            switch (typeID)
            {
                case 0:
                    newNBT = new NBTTagEndCompoundNode();
                    break;
                case 1:
                    newNBT = new NBTNode<byte>
                    {
                        Value = (byte)input.ReadByte()
                    };
                    break;
                case 2:
                    newNBT = new NBTNode<short>
                    {
                        Value = Util.StreamReadShort(input)
                    };
                    break;
                case 3:
                    newNBT = new NBTNode<int>
                    {
                        Value = Util.StreamReadInt(input)
                    };
                    break;
                case 4:
                    newNBT = new NBTNode<long>
                    {
                        Value = Util.StreamReadLong(input)
                    };
                    break;
                case 5:
                    newNBT = new NBTNode<float>
                    {
                        Value = Util.StreamReadFloat(input)
                    };
                    break;
                case 6:
                    newNBT = new NBTNode<double>
                    {
                        Value = Util.StreamReadDouble(input)
                    };
                    break;
                //todo: implement 7
                case 8:
                    short strLen = Util.StreamReadShort(input);
                    byte[] strBuffer = new byte[strLen];
                    input.Read(strBuffer, 0, strLen);
                    newNBT = new NBTNode<string>
                    {
                        Value = Encoding.UTF8.GetString(strBuffer)
                    };
                    break;
                case 9:
                    newNBT = new NBTTagListNode();
                    ((NBTTagListNode)newNBT).innerType = (byte)input.ReadByte();
                    int count = Util.StreamReadInt(input);
                    //Console.WriteLine($"Begin nameless list {name}");
                    for (int x = 0; x < count; x++)
                    {
                        ((NBTTagListNode)newNBT).Value.Add(ReadNBTTagFromStream(input, false, ((NBTTagListNode)newNBT).innerType));
                    }
                    /*if (count == 0)
                    {
                        input.ReadByte();   //stray 0x00
                    }*/
                    //Console.WriteLine($"End nameless list {name}");
                    break;
                case 10:
                    newNBT = ReadNBTTagCompound(input);
                    break;
                //todo: implement 11,12
                default:
                    throw new Exception($"Bad NBT Tag {typeID}");
            }

            newNBT.Tag = typeID;
            newNBT.Name = name;
            Console.WriteLine($"Read tag {(readName ? newNBT.Name : "<nameless list element>")} of tag {newNBT.Tag}\t({newNBT.GetType().Name})");
            return newNBT;
        }

        public static NBTData FromFile(string filepath)
        {
            NBTData ret = new NBTData();

            Stream nbtStream = File.OpenRead(filepath);

            byte[] header = new byte[2];
            nbtStream.Read(header,0, 2);
            nbtStream.Seek(0, SeekOrigin.Begin);
            if (header.SequenceEqual(new byte[] { 0x1F, 0x8B }))
            {
                ret.wasGzipCompressed = true;
                nbtStream = new GZipStream(nbtStream, CompressionMode.Decompress);
            }

            ret.rootNode = (NBTTagCompoundNode)ReadNBTTagCompound(nbtStream).Value[0];

            //PrintNBT(ret.rootNode);

            nbtStream.Dispose();

            return ret;
        }

        public void WriteTagContentsToFile(Stream output, NBTBase a)
        {
            switch (a.Tag)
            {
                case 1:
                    output.WriteByte(((NBTNode<byte>)a).Value);
                    break;
                case 2:
                    Util.StreamWriteShort(output, ((NBTNode<short>)a).Value);
                    break;
                case 3:
                    Util.StreamWriteInt(output, ((NBTNode<int>)a).Value);
                    break;
                case 4:
                    Util.StreamWriteLong(output, ((NBTNode<long>)a).Value);
                    break;
                case 5:
                    Util.StreamWriteFloat(output, ((NBTNode<float>)a).Value);
                    break;
                case 6:
                    Util.StreamWriteDouble(output, ((NBTNode<double>)a).Value);
                    break;
                //todo: implement 7
                case 8:
                    byte[] textUTF8 = Encoding.UTF8.GetBytes(((NBTNode<string>)a).Value);
                    Util.StreamWriteShort(output, (short)textUTF8.Length);
                    output.Write(textUTF8, 0, textUTF8.Length);
                    break;
                case 9:
                    List<NBTBase> namelessList = ((NBTTagListNode)a).Value;
                    byte innerType = ((NBTTagListNode)a).innerType;
                    output.WriteByte(innerType);
                    Util.StreamWriteInt(output, namelessList.Count);
                    foreach (NBTBase b in namelessList)
                    {
                        if (innerType == 10)
                        {
                            WriteCompoundToFile(output, (NBTTagCompoundNode)b, false);
                        }
                        else
                        {
                            WriteTagContentsToFile(output, b);
                        }
                    }
                    break;
                default:
                    throw new Exception($"Unimplemented tag ID {a.Tag}");
            }
        }

        public void WriteCompoundToFile(Stream output, NBTTagCompoundNode tag, bool writeNameAndType = true)
        {
            if (writeNameAndType)
            {
                output.WriteByte(0x0a);
                byte[] nameUTF8 = Encoding.UTF8.GetBytes(tag.Name ?? "");
                Util.StreamWriteShort(output, (short)nameUTF8.Length);
                output.Write(nameUTF8, 0, nameUTF8.Length);
            }

            //write subtags here
            foreach (NBTBase a in tag.Value)
            {
                if (a.Tag == 10)
                {
                    WriteCompoundToFile(output, ((NBTTagCompoundNode)a));
                }
                else
                {
                    output.WriteByte(a.Tag);

                    byte[] subNameUTF8 = Encoding.UTF8.GetBytes(a.Name);
                    Util.StreamWriteShort(output, (short)subNameUTF8.Length);
                    output.Write(subNameUTF8, 0, subNameUTF8.Length);

                    WriteTagContentsToFile(output, a);
                }
            }

            output.WriteByte(0x00);
        }

        public void ToFile(string filepath, bool? compressIntoGzip = null)
        {
            Stream fs = File.Open(filepath, FileMode.Create);
            if ((compressIntoGzip == null && wasGzipCompressed) 
                || (compressIntoGzip.HasValue && compressIntoGzip.Value))
            {
                fs = new GZipStream(fs, CompressionMode.Compress);
            }
            WriteCompoundToFile(fs, rootNode);
            fs.Close();
        }

    }
}
