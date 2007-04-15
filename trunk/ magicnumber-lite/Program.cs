using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MagicNumberLite;

namespace MagicNumberLite
{
    class Program
    {
        static void Main(string[] args)
        {
            Inspector inspector = new Inspector();
            foreach (string arg in args)
            {
                DataType type = inspector.GetDataType(new StreamReader(arg).BaseStream);
                if (type == null) Console.WriteLine("{0} not identified", arg);
                else Console.WriteLine(String.Join("\t", new string[] { arg, type.Name, type.MimeType, type.Extension }));
            }
        }
    }
}
