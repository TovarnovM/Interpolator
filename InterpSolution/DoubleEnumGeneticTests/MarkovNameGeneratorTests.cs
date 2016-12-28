using Microsoft.VisualStudio.TestTools.UnitTesting;
using DoubleEnumGenetic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic.Tests {
    [TestClass()]
    public class MarkovNameGeneratorTests {
        [TestMethod()]
        public void GetStandartTest() {
            var std = MarkovNameGenerator.GetStandart();
            int namesNeed = 10000;
            var manynames = Enumerable.Range(0,namesNeed).Select(ii => std.GetNextName()).ToList();

            int count = manynames.Count;
            int countUnique = manynames.Distinct().Count();
            Assert.AreEqual(namesNeed,count);
            Assert.AreEqual(count,countUnique);
        }
    }
}