using GeneticSharp.Domain.Chromosomes;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic {
    public class ChromosomeD : ChromosomeBase, IChromosome {
        private IList<GeneDoubleRange> _gInfoDouble;
        public IList<GeneDoubleRange> GInfoDouble { get { return _gInfoDouble; } }

        public Dictionary<string,Criteria> Crits { get; set; }

        public Dictionary<string,double> DopParams;

        public IEnumerable<CritInfo> CritsInfos() {
            foreach(var crit in Crits.Values) {
                yield return crit.Info;
            }
        }

        public IEnumerable<string> GetAllNames() {
            return _gInfoDouble.Select(gi => gi.Name).Concat(Crits.Keys);
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

        public ChromosomeD(IList<GeneDoubleRange> geneInfos,IEnumerable<CritInfo> crits = null) : base(geneInfos.Count()) {
            Crits = new Dictionary<string,Criteria>();
            if(crits != null)
                foreach(var crit in crits) {
                    AddCrit(new Criteria(crit));
                }
            _gInfoDouble = geneInfos;
            CreateGenes();
        }


        public double this[string DoubleGeneName] {
            get {
                int index = 0;
                //if(DoubleGeneName is string) {
                var item = _gInfoDouble.FirstOrDefault(gi => gi.Name == (string)DoubleGeneName);
                if(item == null) {
                    if(Crits.ContainsKey(DoubleGeneName))
                        return Crits[DoubleGeneName].Value ?? 0d;
                    throw new ArgumentException("Такого гена нет ",DoubleGeneName.ToString());
                }
                    
                index = _gInfoDouble.IndexOf(item);
                // } else
                //if(IndexOrString is int) {
                //    index = IndexOrString;
                //}
                //if(_gInfoDouble[index] is GeneDoubleRange)
                return (double)GetGene(index).Value;
                //if(_gInfoDouble[index] is GeneStringEnum)
                //    return (_gInfoDouble[index] as GeneStringEnum)[(int)GetGene(index).Value];
                //return null;
            }
            set {
                int index = 0;
                if(DoubleGeneName is string && _gInfoDouble.Any(gi => gi.Name == DoubleGeneName)) {
                    var item = _gInfoDouble.FirstOrDefault(gi => gi.Name == DoubleGeneName);
                    if(item == null) {
                        if(Crits.ContainsKey(DoubleGeneName))
                            Crits[DoubleGeneName].Value = value;
                        throw new ArgumentException("Такого гена нет ",DoubleGeneName.ToString());
                    }
                        
                    index = _gInfoDouble.IndexOf(item);
                } //else
                //if(DoubleGeneName is int) {
                //    index = DoubleGeneName;
                //    if(index < 0 || index >= _gInfoDouble.Count)
                //        throw new ArgumentOutOfRangeException();
                //}
                if(_gInfoDouble[index] is GeneDoubleRange && IsNumber(value)) {
                    double val = ToDouble(value);
                    if(_gInfoDouble[index].ValidateValue(val))
                        ReplaceGene(index,new Gene(val));
                    else
                        throw new ArgumentOutOfRangeException($"value = {value}",$"Значение находится за пределами [{(_gInfoDouble[index] as GeneDoubleRange).Left} ; {(_gInfoDouble[index] as GeneDoubleRange).Right}]");
                }

                //if(_gInfoDouble[index] is GeneStringEnum) {
                //    if(!_gInfoDouble[index].ValidateValue(value))
                //        throw new ArgumentException("Такой разновидности гена нет ",value.ToString());
                //    if(value is string)
                //        ReplaceGene(index,new Gene((_gInfoDouble[index] as GeneStringEnum).IndexOf((string)value)));
                //    if(value is int)
                //        ReplaceGene(index,new Gene((int)value));
                //}
                //throw new ArgumentException("Такого гена нет ",IndexOrString.ToString());
            }
        }

        public override IChromosome CreateNew() {
            return new ChromosomeD(_gInfoDouble,CritsInfos());
        }

        public override Gene GenerateGene(int geneIndex) {
            return new Gene(_gInfoDouble[geneIndex].GetRandValue());

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
            ;
            bool secondCooler = true;
            foreach(var critName in first.Crits.Keys) {
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
        public static void TryGetInPareto(IList<ChromosomeD> pareto,ChromosomeD candidate) {
            for(int i = pareto.Count - 1; i >= 0; i--) {
                var pr = pareto[i];
                switch(ChromosomeD.ParetoRel(pr,candidate)) {
                    case 1: {
                        return;
                    }
                    case -1: {
                        pareto.RemoveAt(i);
                    }
                    break;
                    default:
                    break;
                }
            }
            pareto.Add(candidate);
        }
        public static IList<ChromosomeD> Pareto(IEnumerable<IChromosome> all) {
            var par = new List<ChromosomeD>();
            foreach(var chr in all) {
                TryGetInPareto(par,chr as ChromosomeD);
            }
            return par;
        }
        public static void PerformParetoRange(IList<IChromosome> chromosomes) {
            var allCr = new List<IChromosome>(chromosomes);
            int i = MAX_PAR;
            while(allCr.Count > 0) {
                var par = Pareto(allCr);
                foreach(var cr in par) {
                    cr.Fitness = i--;
                    allCr.Remove(cr);
                }
            }
        }
        public static MatrixD GeneDifference(IList<ChromosomeD> crGroup) {
            var dRange =
                crGroup.
                First().
                GInfoDouble.
                Where(gInfo => gInfo is GeneDoubleRange).
                Select(gInfo => {
                    int gIndex = crGroup.First().GInfoDouble.IndexOf(gInfo);
                    var max = crGroup.Max(cr => (double)cr.GetGene(gIndex).Value);
                    var min = crGroup.Max(cr => (double)cr.GetGene(gIndex).Value);
                    return new {
                        Info = gInfo as GeneDoubleRange,
                        Delta = max - min,
                        TooSmall = (max - min) < 1E-10
                    };
                });

            var rMatr = new MatrixD(crGroup.Count,crGroup.Count);
            for(int i = 0; i < crGroup.Count; i++) {
                var cr = crGroup[i];
                rMatr[i,i] = 0d;
                for(int j = i; j < crGroup.Count; j++) {
                    var crIn = crGroup[j];
                    var r = 0d;
                    foreach(var gInfo in cr.GInfoDouble) {
                        var rr2 = 0d;
                        if(gInfo is GeneDoubleRange) {
                            var gRange = dRange.First(rn => rn.Info == gInfo);
                            if(gRange.TooSmall)
                                continue;
                            rr2 = Math.Abs((double)crIn[gInfo.Name] - (double)cr[gInfo.Name]) / gRange.Delta;
                        }
                        //} else if(gInfo is GeneStringEnum)
                        //    rr2 = (int)crIn[gInfo.Name] != (int)cr[gInfo.Name] ? 0d : 1d;
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
        public static MatrixD CritDifference(IList<ChromosomeD> crGroup) {
            var crRange =
                crGroup.
                First().
                Crits.
                Values.
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
                for(int j = i; j < crGroup.Count; j++) {
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
    }

}
