using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Math;
using Microsoft.CSharp.RuntimeBinder;
using GeneticSharp.Domain.Populations;
using Sharp3D.Math.Core;
using System.Runtime.CompilerServices;

namespace MultiGenetic {
    public interface IGeneInfo {
        string Name { get; set; }
        dynamic GetRandValue();
        bool ValidateValue(dynamic value);
    }

    public class GeneDoubleRange : IGeneInfo {
        public double eps = 0.000000001;
        public string Name { get; set; }
        public dynamic GetRandValue() {
            return RandomizationProvider.Current.GetDouble(Left, Right);
        }
        private double _min = 0d;
        public double Left {
            get { return _min; }
            set {
                if (value <= Right)
                    _min = value;
                else
                    _min = Right;
            }
        }
        private double _max = 0d;
        public double Right {
            get { return _max; }
            set {
                if (value >= Left)
                    _max = value;
                else
                    _max = Left;
            }
        }
        public GeneDoubleRange(string name, double min, double max) {
            Name = name;
            _min = min < max ? min : max;
            _max = max > min ? max : min;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Gauss(double x, double xm, double sko) {
            return Exp(-0.5 * (x - xm) * (x - xm) / (sko * sko)) / (2.506628274631000502415765284811 * sko);
        }
        public double GetRandValue_Norm(double xm, double sko) {
            if (sko < eps)
                return xm >= Left && xm <= Right ? xm : GetRandValue();
            const double SIGMARULE = 3.5;
            double x1 = Max(_min,xm - SIGMARULE * sko);
            double x2 = Min(_max,xm + SIGMARULE * sko);
            if(x1 >= x2)
                return xm <= _min ? _min : _max; 

            double amplit = xm >= x1 && xm <= x2 ?
                Gauss(xm, xm, sko) :
                Max(Gauss(x1, xm, sko), Gauss(x2, xm, sko));
            double x, h, f;
            do {
                x = RandomizationProvider.Current.GetDouble(x1,x2);
                h = RandomizationProvider.Current.GetDouble();
                f = Gauss(x, xm, sko) / amplit;
            } while (f <= h);
            return x;
        }
        public double GetRandValue_Uniform(double x1, double x2) {
            double lft = Min(x1, x2);
            double rgt = Max(x1, x2);
            if (rgt < Left)
                return Left;
            if (lft > Right)
                return Right;
            lft = Max(lft, Left);
            rgt = Min(rgt, Right);
            return RandomizationProvider.Current.GetDouble(lft, rgt);
        }

        public bool ValidateValue(dynamic value) {
            if(!(value is double))
                throw new ArgumentException("Нужно проверять Double, а не ",value.GetType().ToString());
            return Left <= (double)value && (double)value <= Right;
        }
    }

    public class GeneStringEnum : IGeneInfo {
        private List<string> _elements;
        public string Name { get; set; }
        public dynamic GetRandValue() {
            return RandomizationProvider.Current.GetInt(0, Count);
        }
        public GeneStringEnum(string name, params string[] elements) {
            Name = name;
            _elements = new List<string>(elements.Length);
            foreach (var elem in elements) {
                if (!_elements.Contains(elem))
                    _elements.Add(elem);
            }
        }

        public int Count {
            get {
                return _elements.Count;
            }
        }

        public bool IndexValidate(int index) {
            return index > -1 && index < Count;
        }

        public bool Contains(string name) {
            return _elements.Contains(name);
        }

        public int IndexOf(string name) {
            return _elements.IndexOf(name);
        }

        public bool ValidateValue(dynamic value) {
            if(!(value is string || value is int))
                throw new ArgumentException("Нужно проверять String или int, а не ",value.GetType().ToString());
            if(value is string)
                return _elements.Any(el => el == (string)value.Trim());
            if(value is int)
                return (int)value >= 0 && (int)value < _elements.Count;
            return false;
        }

        public string this[int index] {
            get {
                return _elements[index];
            }
        }
    }

    public enum CritExtremum { fe_min, fe_max };

    public class CritInfo {
        public CritExtremum Extremum { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string Name { get; set; }
        public CritInfo(string name, CritExtremum extr = CritExtremum.fe_max, double? min = null, double? max = null) {
            Name = name;
            Extremum = extr;
            Min = min;
            Max = max;
        }
        public bool ValidValue(double? value) {
            if(value == null)
                return false;
            if(Min != null && value < Min)
                return false;
            if(Max != null && value > Max)
                return false;
            return true;
        }

    }

    public class Criteria {
        public CritInfo Info { get; set; }
        public double? Value { get; set; }
        public Criteria(CritInfo info, double? value = null) {
            Info = info;
            Value = value;
        }
        public Criteria(Criteria cloneMe):this(cloneMe.Info, cloneMe.Value) {  }
        public Criteria Clone() {
            return new Criteria(this);
        }
    }


    public class ChromosomeDE : ChromosomeBase, IChromosome {
        private IList<IGeneInfo> _gInfo;
        public IList<IGeneInfo> GInfo { get { return _gInfo; } }

        public Dictionary<string, Criteria> Crits { get; set; }

        public IEnumerable<CritInfo> CritsInfos() {
            foreach(var crit in Crits.Values) {
                yield return crit.Info;
            }
        }

        public void AddCrit(ChromosomeDE critsFrom, bool copyValues = false) {
            foreach(var ci in critsFrom.Crits.Values) {
                AddCrit(ci.Info, copyValues ? ci.Value : null);
            }
        }
        public void AddCrit(params CritInfo[] crits) {
            foreach(var crit in crits) {
                AddCrit(crit);
            }
        }
        public void AddCrit(CritInfo fi, double? value = null) {
            Crits.Add(fi.Name,new Criteria(fi,value));
        }
        public void AddCrit(params Criteria[] criterias) {
            foreach(var crit in criterias) {
                AddCrit(crit.Info,crit.Value);
            }
        }

        public ChromosomeDE(IList<IGeneInfo> geneInfos, IEnumerable<CritInfo> crits = null) : base(geneInfos.Count()) {
            Crits = new Dictionary<string,Criteria>();
            if(crits != null)
                foreach(var crit in crits) {
                    AddCrit(crit);
                }
            _gInfo = geneInfos;
            CreateGenes();
        }

        public dynamic this[dynamic IndexOrString] {
            get {
                int index = 0;
                if(IndexOrString is string) {
                    var item = _gInfo.FirstOrDefault(gi => gi.Name == (string)IndexOrString);
                    if(item == null)
                        throw new ArgumentException("Такого гена нет ",IndexOrString.ToString());
                    index = _gInfo.IndexOf(item);
                } else
                if(IndexOrString is int) {
                    index = IndexOrString;
                }
                if(_gInfo[index] is GeneDoubleRange)
                    return (double)GetGene(index).Value;
                if(_gInfo[index] is GeneStringEnum)
                    return (_gInfo[index] as GeneStringEnum)[(int)GetGene(index).Value];
                return null;
            } set {
                int index = 0;
                if(IndexOrString is string && _gInfo.Any(gi => gi.Name == IndexOrString)) {
                    var item = _gInfo.FirstOrDefault(gi => gi.Name == IndexOrString);
                    if(item == null)
                        throw new ArgumentException("Такого гена нет ",IndexOrString.ToString());
                    index = _gInfo.IndexOf(item);
                } else
                if(IndexOrString is int) {
                    index = IndexOrString;
                    if(index < 0 || index >= _gInfo.Count)
                        throw new ArgumentOutOfRangeException();
                }
                if(_gInfo[index] is GeneDoubleRange && IsNumber(value)) {
                    double val = ToDouble(value);
                    if(_gInfo[index].ValidateValue(val))
                        ReplaceGene(index,new Gene(val));
                    else
                        throw new ArgumentOutOfRangeException($"value = {value}",$"Значение находится за пределами [{(_gInfo[index] as GeneDoubleRange).Left} ; {(_gInfo[index] as GeneDoubleRange).Right}]");
                }
                    
                if(_gInfo[index] is GeneStringEnum) {
                    if(!_gInfo[index].ValidateValue(value))
                        throw new ArgumentException("Такой разновидности гена нет ",value.ToString());
                    if(value is string)
                        ReplaceGene(index,new Gene((_gInfo[index] as GeneStringEnum).IndexOf((string)value)));
                    if(value is int)
                        ReplaceGene(index,new Gene((int)value));
                }
                //throw new ArgumentException("Такого гена нет ",IndexOrString.ToString());
            }
        }

        public override IChromosome CreateNew() {
            return new ChromosomeDE(_gInfo, CritsInfos());
        }

        public override Gene GenerateGene(int geneIndex) {
            return new Gene(_gInfo[geneIndex].GetRandValue());

            throw new Exception($"Чет не так с типом или индексом гена( {nameof(ChromosomeDE)} - GenerateGene Method");
        }

        public override IChromosome Clone() {
            var clone = base.Clone() as ChromosomeDE;
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
        public static int ParetoRel(ChromosomeDE first, ChromosomeDE second) {
            bool firstCooler = true; ;
            bool secondCooler = true;
            foreach (var critName in first.Crits.Keys) {
                if(first.Crits[critName].Value == second.Crits[critName].Value)
                    continue;
                var kritBattle = first.Crits[critName].Info.Extremum == CritExtremum.fe_max ?
                    first.Crits[critName].Value > second.Crits[critName].Value :
                    first.Crits[critName].Value < second.Crits[critName].Value;
                firstCooler &= kritBattle;
                secondCooler &= !kritBattle;
                if ((!firstCooler) && (!secondCooler))
                    break;
            }
            return
                firstCooler && secondCooler ? 0:
                firstCooler ? 1 :
                secondCooler ? -1 :
                0;
        }        
        public static void TryGetInPareto(IList<ChromosomeDE> pareto,ChromosomeDE candidate) {
            for(int i = pareto.Count - 1; i >= 0; i--) {
                var pr = pareto[i];
                switch(ChromosomeDE.ParetoRel(pr,candidate)) {
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
        public static IList<ChromosomeDE> Pareto(IEnumerable<IChromosome> all) {
            var par = new List<ChromosomeDE>();
            foreach(var chr in all) {
                TryGetInPareto(par,chr as ChromosomeDE);
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
        public static MatrixD GeneDifference(IList<ChromosomeDE> crGroup) {
            var dRange =
                crGroup.
                First().
                GInfo.
                Where(gInfo => gInfo is GeneDoubleRange).
                Select(gInfo => {
                    int gIndex = crGroup.First().GInfo.IndexOf(gInfo);
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
                    foreach(var gInfo in cr.GInfo) {
                        var rr2 = 0d;
                        if(gInfo is GeneDoubleRange) {
                            var gRange = dRange.First(rn => rn.Info == gInfo);
                            if(gRange.TooSmall)
                                continue;
                            rr2 = Math.Abs((double)crIn[gInfo.Name] - (double)cr[gInfo.Name]) / gRange.Delta;
                        } else if(gInfo is GeneStringEnum)
                            rr2 = (int)crIn[gInfo.Name] != (int)cr[gInfo.Name] ? 0d : 1d;
                        rr2 *= rr2;
                        r += rr2;
                    }
                    r = Math.Sqrt(r / cr.GInfo.Count);
                    rMatr[i,j] = r;
                    rMatr[j,i] = r;
                }
            }
            return rMatr;
        }
        public static MatrixD CritDifference(IList<ChromosomeDE> crGroup) {
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
