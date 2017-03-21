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
            var writer = new SimpleLogger.RollingFlatFileTraceListener(
                "trace.log",
                null,
                null,
                512,
                "HHmmss",
                "yyyyMMdd",
                SimpleLogger.RollFileExistsBehavior.Increment,
                SimpleLogger.RollInterval.Day);

            writer.WriteLine("rrrrrr");

            writer.Flush();
        }


    }
}