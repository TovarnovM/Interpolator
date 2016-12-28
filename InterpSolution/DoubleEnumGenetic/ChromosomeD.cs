using GeneticSharp.Domain.Chromosomes;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic {
    public class ChromosomeD : ChromosomeBase, IChromosome {
        private IList<IGeneDE> _gInfo;
        public IList<IGeneDE> GInfoDouble { get { return _gInfo; } }

        public Dictionary<string,Criteria> Crits { get; set; }

        public Dictionary<string,double> DopParams;

        public string Name { get; set; }

        public IEnumerable<CritInfo> CritsInfos() {
            foreach(var crit in Crits.Values) {
                yield return crit.Info;
            }
        }

        public IEnumerable<string> GetAllNames() {
            return _gInfo.Select(gi => gi.Name).Concat(Crits.Keys);
        }

        public void AddCrit(ChromosomeD critsFrom,bool copyValues = false) {
            foreach(var ci in critsFrom.Crits.Values) {
                AddCrit(ci.Info,copyValues ? ci.Value : null);
            }
        }
        public void AddCrit(params CritInfo[] crits) {
            foreach(var crit in crits) {
                AddCrit(crit);
            }
        }
        public void AddCrit(CritInfo fi,double? value = null) {
            Crits.Add(fi.Name,new Criteria(fi,value));
        }
        public void AddCrit(params Criteria[] criterias) {
            foreach(var crit in criterias) {
                AddCrit(crit.Info,crit.Value);
            }
        }

        public ChromosomeD(IList<IGeneDE> geneInfos,IEnumerable<CritInfo> crits = null) : base(geneInfos.Count()) {
            Crits = new Dictionary<string,Criteria>();
            if(crits != null)
                AddCrit(crits.ToArray());
                //foreach(var crit in crits) {
                //    AddCrit(new Criteria(crit));
                //}
            _gInfo = geneInfos;
            CreateGenes();
        }


        public double this[string DoubleGeneName] {
            get {
                int index = 0;
                //if(DoubleGeneName is string) {
                var item = _gInfo.FirstOrDefault(gi => gi.Name == (string)DoubleGeneName);
                if(item == null) {
                    if(Crits.ContainsKey(DoubleGeneName))
                        return Crits[DoubleGeneName].Value ?? 0d;
                    throw new ArgumentException("Такого гена нет ",DoubleGeneName.ToString());
                }
                    
                index = _gInfo.IndexOf(item);
                return (double)GetGene(index).Value;
            }
            set {
                int index = 0;
                var item = _gInfo.FirstOrDefault(gi => gi.Name == DoubleGeneName);
                if(item == null) {
                    if(Crits.ContainsKey(DoubleGeneName)) {
                        Crits[DoubleGeneName].Value = value;
                        return;
                    }
                        
                    throw new ArgumentException("Такого гена нет ",DoubleGeneName.ToString());
                }
                        
                index = _gInfo.IndexOf(item);
                 
                if(_gInfo[index] is GeneDoubleRange && IsNumber(value)) {
                    double val = ToDouble(value);
                    if(_gInfo[index].ValidateValue(val))
                        ReplaceGene(index,new Gene(val));
                    else
                        throw new ArgumentOutOfRangeException($"value = {value}",$"Значение находится за пределами [{(_gInfo[index] as GeneDoubleRange).Left} ; {(_gInfo[index] as GeneDoubleRange).Right}]");
                }

            }
        }

        public override IChromosome CreateNew() {
            return new ChromosomeD(_gInfo,CritsInfos());
        }

        public override Gene GenerateGene(int geneIndex) {
            return new Gene(_gInfo[geneIndex].GetRandValue());

            throw new Exception($"Чет не так с типом или индексом гена( {nameof(ChromosomeD)} - GenerateGene Method");
        }

        public override IChromosome Clone() {
            var clone = base.Clone() as ChromosomeD;
            foreach(var critName in Crits.Keys) {
                clone.Crits[critName].Value = Crits[critName].Value;
            }
            return clone;
        }

        public bool ValidCrits() {
            return Crits.Values.All(cr => cr.Info.ValidValue(cr.Value));
        }

        #region ParetoStatic Methods


        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>1 - first cooler by pareto, 0 - pareto ==, -1 second cooler</returns>
        public static int MAX_PAR = 100;
        public static int ParetoRel(ChromosomeD first,ChromosomeD second) {
            bool firstCooler = true;          
            bool secondCooler = true;
            foreach(var critName in first.Crits.Where(c => c.Value.Info.matters).Select(c => c.Key)) {
                if(first.Crits[critName].Value == second.Crits[critName].Value)
                    continue;
                var kritBattle = first.Crits[critName].Info.Extremum == CritExtremum.maximize ?
                    first.Crits[critName].Value > second.Crits[critName].Value :
                    first.Crits[critName].Value < second.Crits[critName].Value;
                firstCooler &= kritBattle;
                secondCooler &= !kritBattle;
                if((!firstCooler) && (!secondCooler))
                    break;
            }
            return
                firstCooler && secondCooler ? 0 :
                firstCooler ? 1 :
                secondCooler ? -1 :
                0;
        }
        public static bool TryGetInPareto(IList<ChromosomeD> pareto,ChromosomeD candidate) {
            for(int i = pareto.Count - 1; i >= 0; i--) {
                var pr = pareto[i];
                var answ = ChromosomeD.ParetoRel(pr,candidate);
                if(answ == 1)
                    return false;
                if(answ == -1)
                    pareto.RemoveAt(i);
            }
            pareto.Add(candidate);
            return true;
        }
        public static IList<ChromosomeD> Pareto(List<ChromosomeD> all, bool removeParFromAll = false) {
            var par = new List<ChromosomeD>();
            foreach(var chr in all) {
                TryGetInPareto(par,chr);
            }
            if(removeParFromAll) {
                all.RemoveAll(c => par.Any(cp => ReferenceEquals(cp,c)));
            }
            return par;
        }
        public static void PerformParetoRange(IList<ChromosomeD> chromosomes) {
            var allCr = new List<ChromosomeD>(chromosomes);
            int i = MAX_PAR;
            while(allCr.Count > 0) {
                var par = Pareto(allCr, true);
                foreach(var cr in par) {
                    cr.Fitness = i--;
                }
            }
        }

        class DRange {
            public DRange(GeneDoubleRange Info, double Delta) {
                this.Info = Info;
                this.Delta = Delta;
                this.TooSmall = Delta < 1E-10;
            }

            public double Delta { get; private set; }
            public GeneDoubleRange Info { get; private set; }
            public bool TooSmall { get; private set; }
        }



        public static MatrixD GetGeneDifferenceMatrix(IList<ChromosomeD> crGroup) {
            var dRange =
                crGroup.
                First().
                GInfoDouble.
                Where(gInfo => gInfo is GeneDoubleRange).
                Select(gInfo => {
                    int gIndex = crGroup.First().GInfoDouble.IndexOf(gInfo);
                    var max = crGroup.Max(cr => (double)cr.GetGene(gIndex).Value);
                    var min = crGroup.Min(cr => (double)cr.GetGene(gIndex).Value);
                    return new DRange(gInfo as GeneDoubleRange,max - min);
                }).
                ToList();

            var rMatr = new MatrixD(crGroup.Count,crGroup.Count);
            for(int i = 0; i < crGroup.Count; i++) {
                var cr = crGroup[i];
                rMatr[i,i] = 0d;
                for(int j = i+1; j < crGroup.Count; j++) {
                    var crIn = crGroup[j];
                    var r = 0d;
                    foreach(var gInfo in cr.GInfoDouble) {
                        var rr2 = 0d;
                        if(gInfo is GeneDoubleRange) {
                            var gRange = dRange.First(rn => rn.Info == gInfo);
                            if(gRange.TooSmall)
                                continue;
                            rr2 = Math.Abs((double)crIn[gInfo.Name] - (double)cr[gInfo.Name]) / gRange.Delta;
                        
                        } else if(gInfo is GeneEnumString)
                            rr2 = (int)crIn[gInfo.Name] != (int)cr[gInfo.Name] ? 0d : 1d;
                        rr2 *= rr2;
                        r += rr2;
                    }
                    r = Math.Sqrt(r / cr.GInfoDouble.Count);
                    rMatr[i,j] = r;
                    rMatr[j,i] = r;
                }
            }
            return rMatr;
        }
        public static MatrixD GetCritDifferenceMatrix(IList<ChromosomeD> crGroup) {
            var crRange =
                crGroup.
                First().
                Crits.
                Values.
                Where(v => v.Info.matters).
                Select(crit => {
                    var max = (double)crGroup.Max(cr => cr.Crits[crit.Info.Name].Value);
                    var min = (double)crGroup.Min(cr => cr.Crits[crit.Info.Name].Value);
                    return new {
                        Info = crit.Info,
                        Delta = max - min,
                        DeltaTooSmall = (max - min) < 1E-10
                    };
                });

            var rMatr = new MatrixD(crGroup.Count,crGroup.Count);
            for(int i = 0; i < crGroup.Count; i++) {
                var cr = crGroup[i];
                rMatr[i,i] = 0d;
                for(int j = i+1; j < crGroup.Count; j++) {
                    var crIn = crGroup[j];
                    var r = 0d;
                    foreach(var crRn in crRange) {
                        if(crRn.DeltaTooSmall)
                            continue;
                        double rr2 = (double)cr.Crits[crRn.Info.Name].Value - (double)crIn.Crits[crRn.Info.Name].Value;
                        rr2 /= crRn.Delta;
                        rr2 *= rr2;
                        r += rr2;
                    }
                    r = Math.Sqrt(r);
                    rMatr[i,j] = r;
                    rMatr[j,i] = r;
                }
            }

            return rMatr;
        }

        class diffEntity {
            public int index;
            public double summdiff;
            public diffEntity(int index, double diff) {
                this.index = index;
                this.summdiff = diff;
            }
        }

        public static List<int> GetUniquestGuysIndexes_old_and_wrong(MatrixD diffMatr, int unicestCount) {
            if(!diffMatr.IsSquare || !diffMatr.IsSymmetric)
                throw new Exception("Матрица неправильная!) То ли не квадратная, то ли не симметричная");
            var diffSums = Enumerable.Repeat(0d,diffMatr.Rows).ToList();
            for(int i = 0; i < diffMatr.Rows; i++) {
                for(int j = i+1; j < diffMatr.Columns; j++) {
                    double diffIJ = diffMatr[i,j];
                    diffSums[i] += diffIJ;
                    diffSums[j] += diffIJ;
                }
            }
            var diffs = new List<diffEntity>(diffSums.Count);
            for(int i = 0; i < diffSums.Count; i++) {
                diffs.Add(new diffEntity(i,diffSums[i]));
            }
            while(diffs.Count > unicestCount) {
                int looserInd = diffs.OrderBy(de => de.summdiff).First().index;
                diffs.RemoveAll(de => de.index == looserInd);
                for(int i = 0; i < diffs.Count; i++) {
                    diffs[i].summdiff -= diffMatr[looserInd,diffs[i].index];
                }
            }
            var res = diffs.Select(de => de.index).OrderBy(ind => ind).ToList();
            return res;

        }

        public static List<int> GetUniquestGuysIndexes(MatrixD diffMatr,int unicestCount) {
            if(!diffMatr.IsSquare || !diffMatr.IsSymmetric)
                throw new Exception("Матрица неправильная!) То ли не квадратная, то ли не симметричная");
            var uniqueIndexes = Enumerable.Range(0,diffMatr.Rows).ToList();
            while(uniqueIndexes.Count > unicestCount) {
                int iimin = 0, jjmin = 1;
                double min_curr = diffMatr[uniqueIndexes[iimin],uniqueIndexes[jjmin]];
                for(int ii = 0; ii < uniqueIndexes.Count; ii++) {
                    int i = uniqueIndexes[ii];
                    for(int jj = ii+1; jj < uniqueIndexes.Count; jj++) {
                        int j = uniqueIndexes[jj];
                        if(diffMatr[i,j] < min_curr) {
                            min_curr = diffMatr[i,j];
                            iimin = ii;
                            jjmin = jj;
                        }
                    }
                }

                int imin = uniqueIndexes[iimin];
                double miniinext_curr = 1e9;
                for(int jj = 0; jj < uniqueIndexes.Count; jj++) {
                    int j = uniqueIndexes[jj];
                    if(j == imin || jj == jjmin)
                        continue;
                    if(diffMatr[imin][j] < miniinext_curr)
                        miniinext_curr = diffMatr[imin][j];
                }

                int jmin = uniqueIndexes[jjmin];
                double minjjnext_curr = 1e9;
                for(int ii= 0; ii < uniqueIndexes.Count; ii++) {
                    int i = uniqueIndexes[ii];
                    if(i == jmin || ii == iimin)
                        continue;
                    if(diffMatr[jmin][i] < minjjnext_curr)
                        minjjnext_curr = diffMatr[jmin][i];
                }

                if(minjjnext_curr < miniinext_curr) {
                    uniqueIndexes.RemoveAt(jjmin);
                } else {
                    uniqueIndexes.RemoveAt(iimin);
                }

            }
            return uniqueIndexes;

        }
        #endregion

        public static bool IsNumber(object value) {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        public static double ToDouble(object value) {
            return value is sbyte ? (sbyte)value :
                   value is byte ? (byte)value :
                   value is short ? (short)value :
                   value is ushort ? (ushort)value :
                   value is int ? (int)value :
                   value is uint ? (uint)value :
                   value is long ? (long)value :
                   value is ulong ? (ulong)value :
                   value is float ? (float)value :
                   value is double ? (double)value :
                   value is decimal ? Decimal.ToDouble((decimal)value) :
                   0d;
        }

        #region saveLoad
        public Dictionary<string, double> SaveToDictionary() {
            var res = GetAllNames().ToDictionary(s => s,s => this[s]);
            res.Add("Fitness",Fitness ?? double.NaN);
            return res;
        }
        public void LoadFromDictionary(IDictionary<string,double> dict) {
            foreach(var s in GetAllNames()) {
                this[s] = dict[s];
            }
            Fitness = null;
            if(!Double.IsNaN(dict["Fitness"]))
                Fitness = dict["Fitness"];
        }
        #endregion
    }



}
