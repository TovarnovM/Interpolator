using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Equations {
    public abstract class EquationBase {
        public List<double> pars;
        public List<string> parNames;
        public List<string> varNames;
        public int VarsCount { get; }
        public int ParsCount { get; }

        public EquationBase(int varsCount, int parsCount) {
            this.VarsCount = varsCount;
            this.ParsCount = parsCount;
            varNames = Enumerable
                .Range(1,varsCount)
                .Select(i => $"x{i}")
                .ToList();

            var az = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (Char)i);
            parNames = Enumerable
                .Range(0,1000000)
                .SelectMany(i => az
                                .Select(c => {
                                    string filler = i == 0?"":$"{i}";
                                    return $"{c}{filler}";
                                }))
               .Take(parsCount)
               .ToList();
            pars = Enumerable.Range(0,parsCount).Select(i => 0d).ToList();
        }
        
        public string EqStr {
            get {
                var ps = PatternStr;
                for (int i = 0; i < ParsCount; i++) {
                    ps = ps.Replace(parNames[i],$"{pars[i]}");
                }
                return ps;
            }
        }
        public abstract string PatternStr { get; }
        public abstract double GetSolution(params double[] vars);
        public double GetSolution(IEnumerable<double> vars) {
            return GetSolution(vars.ToArray());
        }
        public double GetSolution(IEnumerable<KeyValuePair<string,double>> varPairs) {

            return GetSolution(ConvertDictToArr(varPairs));
        }
        public string GetSolutionString(params double[] vars) {
            var answer = GetSolution(vars);
            var ps = PatternStr;
            for (int i = 0; i < ParsCount; i++) {
                ps = ps.Replace(parNames[i],$"{pars[i]}");
            }
            for (int i = 0; i < VarsCount; i++) {
                ps = ps.Replace(varNames[i],$"{vars[i]}");
            }
            return ps + $" = {answer}";
        }
        public string GetSolutionString(IEnumerable<double> vars) {
            return GetSolutionString(vars.ToArray());
        }
        public string GetSolutionString(IEnumerable<KeyValuePair<string,double>> varPairs) {
            return GetSolutionString(ConvertDictToArr(varPairs));
        }

        double[] ConvertDictToArr(IEnumerable<KeyValuePair<string,double>> varPairs) {
            var vars = new double[VarsCount];
            foreach (var vp in varPairs) {
                int ind = varNames.IndexOf(vp.Key);
                if (ind < 0)
                    continue;
                vars[ind] = vp.Value;
            }
            return vars;
        }
    }
}
