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


    }
}