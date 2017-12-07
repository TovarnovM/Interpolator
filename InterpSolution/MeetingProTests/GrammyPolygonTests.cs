﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeetingPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;

namespace MeetingPro.Tests {
    [TestClass()]
    public class GrammyPolygonTests {
        [TestMethod()]
        public void IsCrossTest() {
            var v1 = new Vector(0, 0, 0, 0, 0, 0, 2, 2, 0);
            var v2 = new Vector(0, 0, 0, 0, 0, 0, 1, 0, 1);
            var v3 = new Vector(0, 0, 0, 0, 0, 0, 0, 2, 2);
            var poly = new GrammyPolygon(v1, v2, v3);
            var pr = new Vector3D(0, 0, 0);
            var pd = new Vector3D(1, 0, 0);
            var vvv = new Vector3D(1, 1, 1);
            double dist = 0d;
            var cross = poly.IsCross(pr, pd, ref vvv,ref dist);
        }

        [TestMethod()]
        public void DistanceToSegmentTest() {
            var p = new Vector3D(-2, 1, 0);
            var pl1 = new Vector3D(1, 0, 0);
            var pl2 = new Vector3D(0, 0, 1);
            var (d,cp) = GrammyPolygon.DistanceToSegment(p, pl2, pl1);

        }
    }
}