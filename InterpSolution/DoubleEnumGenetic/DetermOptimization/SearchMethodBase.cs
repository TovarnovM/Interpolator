using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic.DetermOptimization {
    public abstract class SearchMethodBase: ISearchMethod {
        #region abstract stuff

        public abstract ChromosomeD BestSolution { get; }
        public abstract IList<ChromosomeD> WhatCalculateNext();
        public abstract void EndCurrentStep();
        public abstract bool HasReached();

        #endregion
        public event EventHandler BestChromosomeChanged;


        ulong _shagNumber = 1000;
        public IList<GeneDoubleRange> geneInfo;
        public Dictionary<string,double> shagDict;
        public ulong ShagNumber {
            get {
                return _shagNumber;
            }
            set {
                _shagNumber = value;
                UpdateShag();
            }
        }

        public IList<ChromosomeD> Solutions { get; protected set; } 

        public IList<ChromosomeD> GetPoints4Jacobian(ChromosomeD center, bool includeCenter = true) { 
            var res = new List<ChromosomeD>(geneInfo.Count + 1);
            if(includeCenter)
                res.Add(center);
            
            foreach(var gi in geneInfo) {
                var bigger = center.CloneWithoutFitness();
                double val = bigger[gi.Name] + shagDict[gi.Name];
                if(gi.ValidateValue(val)) {
                    bigger[gi.Name] = val;
                    bigger.DopInfo = new DopInfoClass() {
                        Name = gi.Name,
                        diff = shagDict[gi.Name]

                    };
                    res.Add(bigger);
                }


                var smaller = center.CloneWithoutFitness();
                val = smaller[gi.Name] - shagDict[gi.Name];
                if(gi.ValidateValue(val)) {
                    smaller[gi.Name] = val;
                    smaller.DopInfo = new DopInfoClass() {
                        Name = gi.Name,
                        diff = -shagDict[gi.Name]
                    };
                    res.Add(smaller);
                }
            }
            return res;
        }

        public IDictionary<string, double> GetJacobian(IList<ChromosomeD> solvedJacobianPoints) {

            var center = solvedJacobianPoints.First(p => p.DopInfo == null);
            var jac = geneInfo
                .ToDictionary(
                    gi => gi.Name,
                    gi => new JacHelper(center,gi.Name)
                );


            foreach(var p in solvedJacobianPoints) {
                if(p.DopInfo == null)
                    continue;
                var dopInfo = p.DopInfo as DopInfoClass;
                if(dopInfo == null)
                    throw new Exception("Чет не то в хромосомах популяции");
                jac[dopInfo.Name].UpdateLR(p);

            }

            foreach(var j in jac.Values) {
                j.dfdx = j.GetdFdx();
            }

            //var norm = jac.Values.Sum(jh => jh.dfdx * jh.dfdx);

            //foreach(var j in jac.Values) {
            //    j.dfdx /= norm;
            //}

            return jac.ToDictionary(j => j.Key,j => j.Value.dfdx);


        }

        public IList<ChromosomeD> FirstCalculation(ChromosomeD startPoint) {
            shagDict = new Dictionary<string,double>(startPoint.GInfoDouble.Count);
            geneInfo = startPoint
                .GetMatterGenes()
                .Where(gi => gi is GeneDoubleRange)
                .Cast<GeneDoubleRange>()
                .ToList();
            foreach(var igene in geneInfo) {
                shagDict.Add(igene.Name,0d);
            }
            UpdateShag();
            Solutions = new List<ChromosomeD>();
            Solutions.Add(startPoint);
            return WhatCalculateNext();
        }
        void UpdateShag() {
            if(geneInfo == null)
                return;
            foreach(var igene in geneInfo) {
                shagDict[igene.Name] = (igene.Right - igene.Left) / ShagNumber;
            }
        }



        class JacHelper {
            double leftX, rightX, leftF, rightF;
            public string paramName;
            public JacHelper(ChromosomeD center,string paramName) {
                this.paramName = paramName;
                leftX = center[paramName];
                rightX = leftX;
                leftF = center.Fitness.Value;
                rightF = leftF;
            }

            public void UpdateLR(ChromosomeD chr) {
                var x = chr[paramName];
                var f = chr.Fitness.Value;
                if(x < leftX) {
                    leftX = x;
                    leftF = f;
                    return;
                }

                if(x > rightX) {
                    rightX = x;
                    rightF = f;
                    return;
                }
            }

            public double DeltaX {
                get {
                    return rightX - leftX;
                }
            }
            public double DeltaF {
                get {
                    return rightF - leftF;
                }
            }
            public double GetdFdx() {
                if(Math.Abs(DeltaX) < 1E-10)
                    return 0;
                return DeltaF / DeltaX;
            }
            public double dfdx;
        }

        public class DopInfoClass {
            public string Name;
            public double diff = 0d;
        }
    }
}
