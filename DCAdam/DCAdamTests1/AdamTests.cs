using Microsoft.VisualStudio.TestTools.UnitTesting;
using DCAdam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdam.Tests
{
    [TestClass()]
    public class AdamTests
    {
        private Adam adam;

        [TestInitialize]
        public void Initialize()
        {
            adam = new Adam();
            adam.Initialize();
            adam.Connect();
        }

        [TestMethod()]
        public void ReadTest()
        {
            byte value = 65;
            adam.Write(17, value);
            byte data = adam.Read(17, 8);

            Assert.AreEqual(value, data);
        }
    }
}