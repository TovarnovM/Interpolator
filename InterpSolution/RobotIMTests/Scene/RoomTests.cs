using Interpolator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotIM.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RobotIM.Scene.Tests {
    [TestClass()]
    public class RoomTests {
        [TestMethod()]
        public void SynchCellSizeNsWithGabTest() {
            var r = new Room();
            r.gabarit = (new Vector(10, 20), new Vector(30, 50));
            r.cellsize = 0.5;
            r.SynchCellSizeNsWithGab();
            Assert.AreEqual(40, r.nw);
            Assert.AreEqual(60, r.nh);
        }

        [TestMethod()]
        public void GetGridCoordsTest1() {
            var r = new Room();
            r.gabarit = (new Vector(10, 20), new Vector(30, 50));
            r.cellsize = 0.5;
            r.SynchCellSizeNsWithGab();

            var answ = r.GetGridCoords(0, 0);

            Assert.AreEqual((0, 0), answ);

        }

        [TestMethod()]
        public void GetGridCoordsTest2() {
            var r = new Room();
            r.gabarit = (new Vector(10, 20), new Vector(30, 50));
            r.cellsize = 0.5;
            r.SynchCellSizeNsWithGab();

            var answ = r.GetGridCoords(100, 100);

            Assert.AreEqual((39, 59), answ);

        }

        [TestMethod()]
        public void GetGridCoordsTest3() {
            var r = new Room();
            r.gabarit = (new Vector(10, 20), new Vector(30, 50));
            r.cellsize = 0.5;
            r.SynchCellSizeNsWithGab();

            var answ = r.GetGridCoords(10.6, 20.6);

            Assert.AreEqual((1, 1), answ);

        }

        [TestMethod()]
        public void GetGridCoordsTest4() {
            var r = new Room();
            r.gabarit = (new Vector(10, 20), new Vector(30, 50));
            r.cellsize = 0.5;
            r.SynchCellSizeNsWithGab();

            var answ = r.GetGridCoords(25.1, 45.1);

            Assert.AreEqual((30, 50), answ);

        }

        [TestMethod()]
        public void GetGridCoordsTest5() {
            var r = new Room();
            r.gabarit = (new Vector(10, 20), new Vector(30, 50));
            r.cellsize = 0.5;
            r.SynchCellSizeNsWithGab();

            var answ = r.GetGridCoords(29.9, 49.9);

            Assert.AreEqual((39, 59), answ);

        }

        [TestMethod()]
        public void GetAllCellCentersTest() {
            var r = new Room();
            r.gabarit = (new Vector(10, 20), new Vector(30, 50));
            r.cellsize = 0.5;
            r.SynchCellSizeNsWithGab();

            var allC = r.GetAllCellCenters().ToArray();
            Assert.AreEqual(40 * 60, allC.Length);
        }

        [TestMethod()]
        public void GetBoolGridTest() {
            var r = new Room();
            r.gabarit = (new Vector(10, 20), new Vector(30, 40));
            r.cellsize = 1;
            r.SynchCellSizeNsWithGab();
            var wall = new LevelLine();
            wall.AddPoint(10, 20);
            wall.AddPoint(10, 40);
            wall.AddPoint(30, 40);
            wall.AddPoint(10, 20);
            r.walls.Add(wall);

            var gr = r.GetBoolGrid();

        }

        [TestMethod()]
        public void FindPathTest() {
            var r = new Room();
            r.gabarit = (new Vector(0, 0), new Vector(20, 20));
            r.cellsize = 1;

            var wall = new LevelLine();
            wall.AddPoint(0, 0);
            wall.AddPoint(0, 20);
            wall.AddPoint(20, 20);
            wall.AddPoint(20, 0);
            wall.AddPoint(0, 0);
            r.walls.Add(wall);

            wall = new LevelLine();
            wall.AddPoint(10, 0);
            wall.AddPoint(10, 15);
            wall.AddPoint(4, 15);
            r.walls.Add(wall);

            wall = new LevelLine();
            wall.AddPoint(0, 10);
            wall.AddPoint(6, 10);
            r.walls.Add(wall);

            r.CreateScene();
            var path = r.FindPath(new Vector(2, 2), new Vector(18, 18));

        }
    }
}