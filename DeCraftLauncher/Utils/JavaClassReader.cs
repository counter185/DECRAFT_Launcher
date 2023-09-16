﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeCraftLauncher.Utils
{
    public class JavaClassReader
    {

        public class JavaClassInfo
        {
            public int magicNumber;
            public short versionMinor;
            public short versionMajor;
            public short classAccessFlags;
            public short thisClassNameIndex;
            public short superClassNameIndex;
            public List<ConstantPoolEntry> entries;
            public List<JavaMethodInfo> methods;
        }

        public class JavaMethodInfo
        {
            public short accessFlags;
            public short nameIndex;
            public short descriptorIndex;

            static readonly Dictionary<int, string> accessFlagNames = new Dictionary<int, string>()
            {
                { 0x0001, "public" },
                { 0x0002, "private" },
                { 0x0004, "protected" },
                { 0x0008, "static" },
                { 0x0010, "final" },
                { 0x0020, "synchronized" },
                { 0x0100, "native" },
                { 0x0400, "abstract" },
            };

            public bool IsPublic => (accessFlags & 0x0001) != 0;
            public bool IsStatic => (accessFlags & 0x0008) != 0;

            public string Name(List<ConstantPoolEntry> constantPool) => ((ConstantPoolEntry.StringEntry)constantPool[nameIndex]).value;
            public string Descriptor(List<ConstantPoolEntry> constantPool) => ((ConstantPoolEntry.StringEntry)constantPool[descriptorIndex]).value;

            [Obsolete]
            public string GetNameAndDescriptor(List<ConstantPoolEntry> constantPool)
            {
                string modifier = String.Join(" ", (from a in accessFlagNames
                                                    where (accessFlags & (short)a.Key) != 0
                                                    select a.Value));

                return $"{modifier} {Name(constantPool)}{Descriptor(constantPool)}";
            }
        }
        public class ConstantPoolEntry
        {
            int tag;

            public static ConstantPoolEntry[] idMap = new ConstantPoolEntry[] {
                null,
                new StringEntry(),
                null,
                new IntegerEntry(),
                new FloatEntry(),
                new LongEntry(),
                new DoubleEntry(),
                new ClassReferenceEntry(),
                new StringReferenceEntry(),
                new FieldReferenceEntry(),
                new MethodReferenceEntry(),
                new InterfaceMethodReferenceEntry(),
                new NameAndTypeDescriptorEntry(),
                null,
                null,
                new MethodHandleEntry(),
                new MethodTypeEntry(),
                new DynamicEntry(),
                new InvokeDynamicEntry(),
                new ModuleEntry(),
                new PackageEntry()
            };

            public virtual ConstantPoolEntry Parse(Stream target)
            {
                return null;
            }

            public class StringEntry : ConstantPoolEntry { 
                new int tag = 1; 
                public string value = ""; 
                public override ConstantPoolEntry Parse(Stream target)
                {
                    StringEntry newEntry = new StringEntry();
                    short length = Util.StreamReadShort(target);
                    for (int x = 0; x < length; x++)
                    {
                        newEntry.value += (char)target.ReadByte();
                    }
                    return newEntry;
                }
            }
            class IntegerEntry : ConstantPoolEntry { 
                new int tag = 3; 
                int value;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    IntegerEntry newEntry = new IntegerEntry();
                    newEntry.value = Util.StreamReadInt(target);
                    return newEntry;
                }
            }
            class FloatEntry : ConstantPoolEntry { 
                new int tag = 4; 
                float value;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    FloatEntry newEntry = new FloatEntry();
                    //don't care
                    newEntry.value = Util.StreamReadInt(target);
                    return newEntry;
                }
            }
            public class LongEntry : ConstantPoolEntry { 
                new int tag = 5; 
                long value;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    LongEntry newEntry = new LongEntry();
                    newEntry.value = Util.StreamReadLong(target);
                    return newEntry;
                }
            }
            public class DoubleEntry : ConstantPoolEntry { 
                new int tag = 6; 
                double value;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    DoubleEntry newEntry = new DoubleEntry();
                    //don't care
                    newEntry.value = Util.StreamReadLong(target);
                    return newEntry;
                }
            }
            public class ClassReferenceEntry : ConstantPoolEntry { 
                new int tag = 7; 
                public int indexOfClassNameString;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    ClassReferenceEntry newEntry = new ClassReferenceEntry();
                    newEntry.indexOfClassNameString = Util.StreamReadShort(target);
                    return newEntry;
                }
                public string GetName(List<ConstantPoolEntry> constantPool)
                {
                    return ((StringEntry)constantPool[indexOfClassNameString]).value;
                }
            }
            class StringReferenceEntry : ConstantPoolEntry { 
                new int tag = 8; 
                int indexOfTargetString;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    StringReferenceEntry newEntry = new StringReferenceEntry();
                    newEntry.indexOfTargetString = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
            class FieldReferenceEntry : ConstantPoolEntry { 
                new int tag = 9; 
                int indexOfClassReference; 
                int indexOfNameAndTypeDescriptor;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    FieldReferenceEntry newEntry = new FieldReferenceEntry();
                    newEntry.indexOfClassReference = Util.StreamReadShort(target);
                    newEntry.indexOfNameAndTypeDescriptor = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
            class MethodReferenceEntry : ConstantPoolEntry { 
                new int tag = 10; 
                int indexOfClassReference; 
                int indexOfNameAndTypeDescriptor;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    MethodReferenceEntry newEntry = new MethodReferenceEntry();
                    newEntry.indexOfClassReference = Util.StreamReadShort(target);
                    newEntry.indexOfNameAndTypeDescriptor = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
            class InterfaceMethodReferenceEntry : ConstantPoolEntry { 
                new int tag = 11; 
                int indexOfClassReference; 
                int indexOfNameAndTypeDescriptor;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    InterfaceMethodReferenceEntry newEntry = new InterfaceMethodReferenceEntry();
                    newEntry.indexOfClassReference = Util.StreamReadShort(target);
                    newEntry.indexOfNameAndTypeDescriptor = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
            class NameAndTypeDescriptorEntry : ConstantPoolEntry { 
                new int tag = 12; 
                int indexOfNameString; 
                int indexOfTypeDescriptor;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    NameAndTypeDescriptorEntry newEntry = new NameAndTypeDescriptorEntry();
                    newEntry.indexOfNameString = Util.StreamReadShort(target);
                    newEntry.indexOfTypeDescriptor = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
            class MethodHandleEntry : ConstantPoolEntry { 
                new int tag = 15; 
                byte typeDescriptor; 
                int indexOfMethod;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    MethodHandleEntry newEntry = new MethodHandleEntry();
                    newEntry.typeDescriptor = (byte)target.ReadByte();
                    newEntry.indexOfMethod = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
            class MethodTypeEntry : ConstantPoolEntry { 
                new int tag = 16; 
                int indexOf;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    MethodTypeEntry newEntry = new MethodTypeEntry();
                    newEntry.indexOf = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
            class DynamicEntry : ConstantPoolEntry { 
                new int tag = 17; 
                int data;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    DynamicEntry newEntry = new DynamicEntry();
                    newEntry.data = Util.StreamReadInt(target);
                    return newEntry;
                }
            }
            class InvokeDynamicEntry : ConstantPoolEntry { 
                new int tag = 18; 
                int data;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    InvokeDynamicEntry newEntry = new InvokeDynamicEntry();
                    newEntry.data = Util.StreamReadInt(target);
                    return newEntry;
                }
            }
            class ModuleEntry : ConstantPoolEntry { 
                new int tag = 19; 
                int id;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    ModuleEntry newEntry = new ModuleEntry();
                    newEntry.id = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
            class PackageEntry : ConstantPoolEntry { 
                new int tag = 20; 
                int id;
                public override ConstantPoolEntry Parse(Stream target)
                {
                    PackageEntry newEntry = new PackageEntry();
                    newEntry.id = Util.StreamReadShort(target);
                    return newEntry;
                }
            }
        }
        public static JavaClassInfo ReadJavaClassFromStream(Stream input)
        {
            JavaClassInfo ret = new JavaClassInfo();
            ret.magicNumber = Util.StreamReadInt(input);

            ret.versionMinor = Util.StreamReadShort(input);
            ret.versionMajor = Util.StreamReadShort(input);

            short nEntriesConstantPool = Util.StreamReadShort(input);
            ret.entries = new List<ConstantPoolEntry>();
            ret.entries.Add(new ConstantPoolEntry());
            for (int x = 1; x < nEntriesConstantPool; x++)
            {
                int index = input.ReadByte();
                if (index < ConstantPoolEntry.idMap.Length && ConstantPoolEntry.idMap[index] != null)
                {
                    ConstantPoolEntry newCPoolEntry = ConstantPoolEntry.idMap[index].Parse(input);
                    //Console.WriteLine($"[{x}/{nEntriesConstantPool}] Adding new " + newCPoolEntry.GetType().Name + (newCPoolEntry is ConstantPoolEntry.StringEntry ? ": " + ((ConstantPoolEntry.StringEntry)newCPoolEntry).value : ""));
                    if (newCPoolEntry is ConstantPoolEntry.DoubleEntry || newCPoolEntry is ConstantPoolEntry.LongEntry)
                    {
                        //i honestly have no idea why this works
                        ret.entries.Add(new ConstantPoolEntry());
                        x++;
                    }
                    ret.entries.Add(newCPoolEntry);
                } else
                {
                    Console.WriteLine($"!! INVALID ConstPool ID: {index}");
                }
            }

            ret.classAccessFlags = Util.StreamReadShort(input);
            ret.thisClassNameIndex = Util.StreamReadShort(input);
            ret.superClassNameIndex = Util.StreamReadShort(input);
            //Console.WriteLine(((ConstantPoolEntry.ClassReferenceEntry)ret.entries[ret.thisClassNameIndex]).GetName(ret.entries));
            //Console.WriteLine(((ConstantPoolEntry.ClassReferenceEntry)ret.entries[ret.superClassNameIndex]).GetName(ret.entries));
            //ConstantPoolEntry.ClassReferenceEntry superClassRef = (ConstantPoolEntry.ClassReferenceEntry)ret.entries[ret.superClassNameIndex];
            //Console.WriteLine("This class: " + ((ConstantPoolEntry.StringEntry)ret.entries[thisClassRef.indexOfClassNameString]).value);
            //Console.WriteLine("Superclass: " + ((ConstantPoolEntry.StringEntry)ret.entries[superClassRef.indexOfClassNameString]).value);
            

            short nEntriesInterfacesTable = Util.StreamReadShort(input);
            List<short> interfaceTable = new List<short>();
            for (int x = 0; x < nEntriesInterfacesTable; x++)
            {
                interfaceTable.Add(Util.StreamReadShort(input));
            }

            short nEntriesFieldTable = Util.StreamReadShort(input);
            for (int x = 0; x < nEntriesFieldTable; x++)
            {
                //don't care
                Util.StreamReadShort(input);   //access flags
                Util.StreamReadShort(input);   //name index
                Util.StreamReadShort(input);   //descriptor index
                short attributes_count = Util.StreamReadShort(input);
                for (int y = 0; y < attributes_count; y++)
                {
                    //don't care about any of this
                    Util.StreamReadShort(input);   //attribute name index
                    int attribute_length = Util.StreamReadInt(input);
                    for (int z = 0; z < attribute_length; z++)
                    {
                        input.ReadByte();   //info element
                    }
                }
            }

            short nEntriesMethodTable = Util.StreamReadShort(input);
            ret.methods = new List<JavaMethodInfo>();
            for (int x = 0; x < nEntriesMethodTable; x++)
            {
                JavaMethodInfo newMethod = new JavaMethodInfo();
                newMethod.accessFlags = Util.StreamReadShort(input);
                newMethod.nameIndex = Util.StreamReadShort(input);
                newMethod.descriptorIndex = Util.StreamReadShort(input);
                short attributes_count = Util.StreamReadShort(input);
                for (int y = 0; y < attributes_count; y++)
                {
                    //don't care
                    Util.StreamReadShort(input);   //attribute name index
                    int attribute_length = Util.StreamReadInt(input);
                    for (int z = 0; z < attribute_length; z++)
                    {
                        input.ReadByte();   //info element
                    }
                }
                ret.methods.Add(newMethod);
            }

            /*foreach (JavaMethodInfo method in ret.methods)
            {
                Console.WriteLine(method.GetNameAndDescriptor(ret.entries));
            }*/

            //Console.WriteLine("Parse finished");
            return ret;
        }
    }
}