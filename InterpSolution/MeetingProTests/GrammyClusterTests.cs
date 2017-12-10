using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeetingPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro.Tests {
    [TestClass()]
    public class GrammyClusterTests {
        [TestMethod()]
        public void GoToNextPosTest() {
            var mt0 = new MT_pos() {
                X = 10,
                Y = 10,
                Z = 10,
                V_x = 1,
                V_y = 1,
                V_z = 1
            };
            var mt1 = new MT_pos() {
                X = 10,
                Y = 10,
                Z = 10,
                V_x = 1,
                V_y = 0,
                V_z = 0
            };
            var mtit = GrammyCluster.GoToNextPos(mt0, mt1);
        }
    }
}