﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Equations {
    public abstract class EquationBase {
        public List<double> Pars { get; }
        public List<string> ParNames { get; }
        public List<string> VarNames { get; }
        public int VarsCount { get; }
        public int ParsCount { get; }

        public EquationBase(int varsCount, int parsCount, bool fillParsVarsPs = true) {
            this.VarsCount = varsCount;
            this.ParsCount = parsCount;
            if (!fillParsVarsPs) {
                VarNames = new List<string>(VarsCount);
                ParNames = new List<string>(ParsCount);
                Pars = new List<double>(ParsCount);
                return;
            }
            

            VarNames = Enumerable
                .Range(1,varsCount)
                .Select(i => $"x{i}")
                .ToList();

            var az = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (Char)i);
            ParNames = Enumerable
                .Range(0,1000000)
                .SelectMany(i => az
                                .Select(c => {
                                    string filler = i == 0?"":$"{i}";
                                    return $"{c}{filler}";
                                }))
               .Take(parsCount)
               .ToList();
            Pars = Enumerable.Range(0,parsCount).Select(i => 0d).ToList();
            ps = CreatePatternStr();
        }
        
        public string EqStr {
            get {
                var ps = PatternStr;
                for (int i = 0; i < ParsCount; i++) {
                    ps = ps.Replace(ParNames[i],$"{Pars[i]}");
                }
                return ps;
            }
        }
        public abstract string CreatePatternStr();
        protected string ps = "";

        public string PatternStr => ps;
        public abstract double GetSolution(params double[] vars);
        public abstract EquationBase Clone();
        public EquationBase Clone(IEnumerable<double> newPars) {
            var c = Clone();
            c.FillPars(newPars);
            return c;
        }
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
                ps = ps.Replace(ParNames[i],$"{Pars[i]}");
            }
            for (int i = 0; i < VarsCount; i++) {
                ps = ps.Replace(VarNames[i],$"{vars[i]}");
            }
            return ps + $" = {answer}";
        }
        public string GetSolutionString(IEnumerable<double> vars) {
            return GetSolutionString(vars.ToArray());
        }
        public string GetSolutionString(IEnumerable<KeyValuePair<string,double>> varPairs) {
            return GetSolutionString(ConvertDictToArr(varPairs));
        }

        public double[] ConvertDictToArr(IEnumerable<KeyValuePair<string,double>> varPairs) {
            var vars = new double[VarsCount];
            foreach (var vp in varPairs) {
                int ind = VarNames.IndexOf(vp.Key);
                if (ind < 0)
                    continue;
                vars[ind] = vp.Value;
            }
            return vars;
        }
        public void FillPars(IEnumerable<double> pars) {
            Pars.Clear();
            Pars.AddRange(pars);
        }

        public double this[string key] {
            get {
                int ind = ParNames.IndexOf(key);
                ind = ind < 0 ? ParNames.IndexOf(key.ToUpper()) : ind;
                return ind >= 0 ? Pars[ind] : double.NaN;
            }

            set {
                int ind = ParNames.IndexOf(key);
                ind = ind < 0 ? ParNames.IndexOf(key.ToUpper()) : ind;
                if (ind >= 0)
                    Pars[ind] = value;
            }
        }

    }
}
