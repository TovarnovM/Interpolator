using Microsoft.Research.Oslo;
using System;
using System.Collections.Generic;

namespace Experiment {
   // public interface 


    public static class MethNavFactory {
        private static Dictionary<string,MethNav> _dict;
        static MethNavFactory() {
            _dict = new Dictionary<string,MethNav>();
            _dict.Add("3p",MissileTarget.MethNav3p);
            _dict.Add("cleanchase",MissileTarget.MethNavCleanChase);
            _dict.Add("paralelsblizj",MissileTarget.MethNavParalelSblizj);
        }
        public static MethNav GetDelegate(string methName) {
            return _dict[methName.ToLower()];
        }
        public static IEnumerable<string> GetAllVariants() {
            return _dict.Keys;
        }
    }

    public delegate IEnumerable<SolPoint> ODEMethod(double t0,Vector x0,Func<double,Vector,Vector> f,double initialStep);
    public static class ODEMethodFactory {
        private static Dictionary<string,ODEMethod> _dict;
        static ODEMethodFactory() {
            _dict = new Dictionary<string,ODEMethod>();
            _dict.Add("euler",Ode.Euler);
            _dict.Add("midpoint",Ode.MidPoint);
            _dict.Add("rk45",Ode.RK45);
            _dict.Add("rk547M",Ode.RK547M);
        }
        public static ODEMethod GetDelegate(string methName) {
            return _dict[methName.ToLower()];
            
        }
        public static IEnumerable<string> GetAllVariants() {
            return _dict.Keys;
        }
    }

    public class SolverOptions {
        public string ODEMethodName { get; set; }
        public double StepODE { get; set; }
        public double StepOut { get; set; }
        public List<int> MyProperty { get; set; }


    }

    public class Solver {
        public List<string> Names { get; set; } = new List<string>();
        public List<Vector> Params { get; set; } = new List<Vector>();

    }
}
