using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tsharp
{
    [TestClass]
    public class RollingFlatFileTraceListenerTests
    {
        [TestMethod]
        public void RollingFlatFileTraceListenerTest()
        {


            var writer = new SimpleFileLogger.RollingFlatFileTraceListener(
                "trace.log",
                null,
                null,
                512,
                "HHmmss",
                "yyyyMMdd",
                SimpleFileLogger.RollFileExistsBehavior.Increment,
                SimpleFileLogger.RollInterval.Day);

            writer.WriteLine("rrrrrr");

            writer.Flush();
        }
      

        [TestMethod( )]
        public void WriteCsvLineTest()
        {
            var rsg = new RandomStringGenerator(true, true, true, true);

            using (
                var writer = new SimpleFileLogger.RollingFlatFileTraceListener(
                    "trace.log",
                    null,
                    null,
                    512,
                    "HHmmss",
                    "yyyyMMdd",
                    SimpleFileLogger.RollFileExistsBehavior.Increment,
                    SimpleFileLogger.RollInterval.Day))
            {
                foreach (var i in Enumerable.Range(1, 3)) writer.WriteCsvLine(Enumerable.Range(1, 8).Select(x => rsg.Generate(3, 6)).ToArray());
                foreach (var i in Enumerable.Range(1, 3))
                    writer.WriteCsvLine(
                        Enumerable.Range(1, 8)
                            .Select(x => rsg.Generate(2) + Environment.NewLine + rsg.Generate(1, 2))
                            .ToArray());
                writer.Flush();
            }
        }
    }
}