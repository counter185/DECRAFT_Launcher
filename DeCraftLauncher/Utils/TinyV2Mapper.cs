using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCraftLauncher.Utils
{
    public class TinyV2Mapper
    {
        public string nameFrom;
        public string nameTo;

        public List<ClassMapping> remappedClasses = new List<ClassMapping>();

        public class Mappable
        {
            public string from;
            public string to;
        }
        public class MethodParameterMapping
        {
            public int paramNumber;
            public string name;

            public void Parse(string a)
            {
                string[] tabSplits = a.Split('\t');
                this.paramNumber = int.Parse(tabSplits[3]);
                this.name = tabSplits[5];
            }
        }
        public class MethodMapping : Mappable
        {
            public string descriptor;
            public List<MethodParameterMapping> methodParams = new List<MethodParameterMapping>();

            public void Parse(string a)
            {
                string[] tabSplits = a.Split('\t');
                this.descriptor = tabSplits[2];
                this.from = tabSplits[3];
                this.to = tabSplits[4];
            }
        }
        public class FieldMapping : Mappable
        {
            public string type;

            public void Parse(string a)
            {
                string[] tabSplits = a.Split('\t');
                this.type = tabSplits[2];
                this.from = tabSplits[3];
                this.to = tabSplits[4];
            }
        }
        public class ClassMapping : Mappable
        {
            public List<MethodMapping> remappedMethods = new List<MethodMapping>();
            public List<FieldMapping> remappedFields = new List<FieldMapping>();

            public void Parse(string a)
            {
                string[] tabSplits = a.Split('\t');
                this.from = tabSplits[1];
                this.to = tabSplits[2];
            }
        }

        public static TinyV2Mapper FromMappingsFile(string file)
        {
            TinyV2Mapper mapper = new TinyV2Mapper();

            FileStream inFile = File.OpenRead(file);
            StreamReader fileReader = new StreamReader(inFile);
            string nLine = null;
            try
            {
                while ((nLine = fileReader.ReadLine()) != null)
                {
                    if (nLine.StartsWith("tiny\t"))
                    {
                        string[] tabSplit = nLine.Split('\t');
                        mapper.nameFrom = tabSplit[3];
                        mapper.nameTo = tabSplit[4];
                    }
                    else if (nLine.StartsWith("c\t"))
                    {
                        ClassMapping nClass = new ClassMapping();
                        nClass.Parse(nLine);
                        mapper.remappedClasses.Add(nClass);
                    }
                    else if (nLine.StartsWith("\tm"))
                    {
                        MethodMapping nMethod = new MethodMapping();
                        nMethod.Parse(nLine);
                        mapper.remappedClasses.Last().remappedMethods.Add(nMethod);
                    }
                    else if (nLine.StartsWith("\tf"))
                    {
                        FieldMapping nField = new FieldMapping();
                        nField.Parse(nLine);
                        mapper.remappedClasses.Last().remappedFields.Add(nField);
                    }
                    else if (nLine.StartsWith("\t\tp"))
                    {
                        MethodParameterMapping nParam = new MethodParameterMapping();
                        nParam.Parse(nLine);
                        mapper.remappedClasses.Last().remappedMethods.Last().methodParams.Add(nParam);
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            } finally
            {
                fileReader.Close();
            }

            return mapper;
        }
    }
}
