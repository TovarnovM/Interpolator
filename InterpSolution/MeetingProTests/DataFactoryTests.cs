using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeetingPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro.Tests {
    [TestClass()]
    public class DataFactoryTests {
        [TestMethod()]
        public void GetA3Test() {
            var a3 = DataFactory.GetA3();
            Assert.AreEqual(1055.7, a3[6.69, 4.06], 0.1);
            //Assert.AreEqual(1055.7, a3[5, 7], 0.1);

        }
    }
}