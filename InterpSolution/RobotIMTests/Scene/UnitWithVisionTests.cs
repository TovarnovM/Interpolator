using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotIM.Scene;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotIM.Scene.Tests {
    [TestClass()]
    public class UnitWithVisionTests {
        [TestMethod()]
        public void RotateFromToTest1() {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(0, 1);
            var angle = 45d;
            var speed = 45d;
            var dt = 1d;

            var answ = UnitWithVision.RotateFromTo(v1, v2, speed, dt);
            Assert.IsTrue(Vector2D.ApproxEqual(answ, new Vector2D(Sqrt(2) / 2, Sqrt(2) / 2)));
        }
        [TestMethod()]
        public void RotateFromToTest2() {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(0, 1);
            var speed = 90d;
            var dt = 1d;

            var answ = UnitWithVision.RotateFromTo(v1, v2, speed, dt);
            Assert.IsTrue(Vector2D.ApproxEqual(answ, new Vector2D(0, 1)));
        }
        [TestMethod()]
        public void RotateFromToTest3() {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(0, -1);
            var speed = 45d;
            var dt = 1d;

            var answ = UnitWithVision.RotateFromTo(v1, v2, speed, dt);
            Assert.IsTrue(Vector2D.ApproxEqual(answ, new Vector2D(Sqrt(2) / 2, -Sqrt(2) / 2)));
        }
        [TestMethod()]
        public void RotateFromToTest4() {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(0, 1);
            var speed = 900d;
            var dt = 1d;

            var answ = UnitWithVision.RotateFromTo(v1, v2, speed, dt);
            Assert.IsTrue(Vector2D.ApproxEqual(answ, new Vector2D(0, 1)));
        }
        [TestMethod()]
        public void RotateFromToTest5() {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(0, -1);
            var speed = 900d;
            var dt = 1d;

            var answ = UnitWithVision.RotateFromTo(v1, v2, speed, dt);
            Assert.IsTrue(Vector2D.ApproxEqual(answ, new Vector2D(0, -1)));
        }

        [TestMethod()]
        public void RotateFromToTest6() {
            var v1 = new Vector2D(1, 0);
            var v2 = new Vector2D(1, 0);
            var speed = 900d;
            var dt = 1d;

            var answ = UnitWithVision.RotateFromTo(v1, v2, speed, dt);
            Assert.IsTrue(Vector2D.ApproxEqual(answ, new Vector2D(1, 0)));
        }
    }
}