using System;
using System.IO;
using System.Reflection;
using Omnidek.Models;
using Utils.TsExport.Core;

namespace Utils.TsExport
{
    class Program
    {
        public static void Main(string[] args)
        {
            //var utility = new CSharpToTypescript();
            //utility.BaseClassType = typeof(Entity);
            //utility.Export();

            var configuration = new Configuration();
            configuration.OutputDirectory = Directory.GetCurrentDirectory();
            configuration.Assembly = Assembly.GetAssembly(typeof(BaseEntity));

            var engine = new Engine(configuration);
            engine.Export();

            Console.WriteLine("Program completed. Press any key to exit.");
            Console.Read();

        }
    }
}
