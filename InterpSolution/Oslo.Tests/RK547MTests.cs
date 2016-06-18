using Microsoft.Research.Oslo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Oslo.Tests {
    [TestClass]
    public class RK547MTests {
        /// <summary>Solves dx/dt = exp(-x) equation with x(0) = 1 initial condition</summary>
        [TestMethod]
        public void ExponentSolveToRKTest() {
            foreach (var sp in Ode.RK547M(0,
                1,
                (t, x) => -x,
                new Options { RelativeTolerance = 1e-3 }).SolveTo(1000))
                Assert.IsTrue(Math.Abs(sp.X[0] - Math.Exp(-sp.T)) < 1e-2);
        }

        /// <summary>Solves dx/dt = exp(-x) equation with x(0) = 1 initial condition</summary>
        [TestMethod]
        public void ExponentSolveToRKTest1() {
            foreach (var sp in Ode.RK45(0,
                1,
                (t, x) => -x,
                new Options { RelativeTolerance = 1e-3, InitialStep = 0.1 }).SolveTo(1000))
                Assert.IsTrue(Math.Abs(sp.X[0] - Math.Exp(-sp.T)) < 1e-2);
        }

        /// <summary>Solves dx/dt = exp(-x) equation an stores results in array</summary>
        [TestMethod]
        public void ExponentSolveToArrayTest() {
            var arr = Ode.RK547M(0,
                1,
                (t, x) => -x,
                new Options { RelativeTolerance = 1e-3 }).SolveTo(1000).ToArray();

            foreach (var sp in arr)
                Assert.IsTrue(Math.Abs(sp.X[0] - Math.Exp(-sp.T)) < 1e-2); // AbsTol instead of 1e-4
        }
    }
}