﻿#region USING BLOCK

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

#endregion

namespace Tsharp
{
    [TestClass]
    public class LogSourceTests
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void LogSourceTest()
        {
            var source = SimpleLogger.LogManager.GetCurrentClassLogger();
            RandomStringGenerator random = new RandomStringGenerator();

            foreach (var i in Enumerable.Range(1, 3000))
            {
                source.Info(random.Generate(1000));
            }
        }

        [TestMethod]
        public void LogSourceTest1()
        {
            return;
            DateTimeOffset dt = DateTimeOffset.Now;

            //dt = dt.ToUniversalTime().ToUniversalTime().ToUniversalTime();
            string dateTimePattern = "u";
            Console.WriteLine($"{dateTimePattern}\t={dt.ToString(dateTimePattern)}");
            CultureInfo ci = CultureInfo.CurrentCulture;
            for (var enumerator = ci.DateTimeFormat.GetAllDateTimePatterns().GetEnumerator();
                enumerator.MoveNext();
            dateTimePattern = enumerator.Current.ToString())
            {
                Console.WriteLine($"{dateTimePattern}\t={dt.ToString(dateTimePattern)}");
            }


        }

        [TestMethod]
        public void DisposeTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void WriteLineTest()
        {
            RandomStringGenerator random = new RandomStringGenerator();

            foreach (var i in Enumerable.Range(1, 3000))
            {
                SimpleLogger.LogManager.GetCurrentClassLogger().Info(random.Generate(1000));
            }
            foreach (var i in Enumerable.Range(1, 3000))
            {
                SimpleLogger.LogManager.GetCurrentClassLogger().Debug(random.Generate(1000));
            }
        }
    }
}