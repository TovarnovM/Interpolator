using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotIM.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM.Scene.Tests {
    [TestClass()]
    public class TargetKnowlegeTests {
        [TestMethod()]
        public void PerformTriggTest1() {
            var tk = new TargetKnowlege();
            Assert.AreEqual(TargetKnowlege.TargInfo.nothing, tk.info);
        }
        [TestMethod()]
        public void PerformTriggTest2() {
            var tk = new TargetKnowlege();
            tk.Attacked();
            Assert.AreEqual(TargetKnowlege.TargInfo.enemy, tk.info);
        }
        [TestMethod()]
        public void PerformTriggTest3() {
            var tk = new TargetKnowlege();
            tk.See();
            Assert.AreEqual(TargetKnowlege.TargInfo.susp, tk.info);
        }
        [TestMethod()]
        public void PerformTriggTest4() {
            var tk = new TargetKnowlege();
            tk.Hear();
            Assert.AreEqual(TargetKnowlege.TargInfo.susp, tk.info);
        }
        [TestMethod()]
        public void PerformTriggTest5() {
            var tk = new TargetKnowlege();
            tk.Hear();
            tk.See();
            Assert.AreEqual(TargetKnowlege.TargInfo.enemyAlmost, tk.info);
        }
        [TestMethod()]
        public void PerformTriggTest6() {
            var tk = new TargetKnowlege();
            tk.SeeMoving();
            Assert.AreEqual(TargetKnowlege.TargInfo.enemyAlmost, tk.info);
        }
        [TestMethod()]
        public void PerformTriggTest7() {
            var tk = new TargetKnowlege();
            tk.Hear();
            tk.See();
            tk.Hear();
            tk.See();
            tk.Hear();
            tk.See();
            Assert.AreEqual(TargetKnowlege.TargInfo.enemyAlmost, tk.info);
        }
        [TestMethod()]
        public void PerformTriggTest8() {
            var tk = new TargetKnowlege();
            tk.Hear();
            tk.SeeMoving();
            tk.Hear();
            tk.See();
            tk.SeeMoving();
            tk.See();
            Assert.AreEqual(TargetKnowlege.TargInfo.enemyAlmost, tk.info);
        }

        [TestMethod()]
        public void PerformTriggTest9() {
            var tk = new TargetKnowlege();
            tk.Attacked();
            tk.SeeMoving();
            tk.Hear();
            tk.See();
            tk.SeeMoving();
            tk.See();
            Assert.AreEqual(TargetKnowlege.TargInfo.enemy, tk.info);
        }
    }
}