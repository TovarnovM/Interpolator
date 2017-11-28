using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeetingPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp3D.Math.Core;
using Microsoft.Research.Oslo;

namespace MeetingPro.Tests {
    [TestClass()]
    public class TrInterpolatorTests {
        [TestMethod()]
        public void InterpTest() {
            var va = new Vector2D(1, 1);
            var vb = new Vector2D(1, 2);
            var vc = new Vector2D(0, 3);
            var fa = new Vector(1);
            var fb = new Vector(2);
            var fc = new Vector(-2);
            var interp = new TrInterpolator(va, vb, vc, fa, fb, fc);
            var f1 = interp.Interp(new Vector2D(1, 1.5));
            var f2 = interp.Interp(new Vector2D(1, 2));
            var f3 = interp.Interp(new Vector2D(0, 3));
        }
    }
}