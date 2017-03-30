using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tsharp.SimpleLogger;

namespace FileConfigTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigReader reader = new ConfigReader("test.conf");
            reader.ConfigChange += (s, x) => GetValue(x);
            do
            {
                GetValue(reader);
            } while ("Q" != Console.ReadLine());
        }

        private static void GetValue(ConfigReader reader)
        {
            Console.Clear();
            foreach (var key in reader.GetKeys())
            {
                Console.WriteLine($"{key} = {reader.GetValue(key, "")}");
            }
        }
    }
}
