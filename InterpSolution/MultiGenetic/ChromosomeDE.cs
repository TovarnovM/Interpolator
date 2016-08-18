using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Math;

namespace MultiGenetic {
    public interface IGeneInfo {
        string Name { get; set; }
        dynamic GetRandValue();
    }

    public class DoubleRange : IGeneInfo {
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
        public DoubleRange(string name, double min, double max) {
            Name = name;
            _min = min < max ? min : max;
            _max = max > min ? max : min;
        }

        public double Gauss(double x, double xm, double sko) {
            return Exp(-0.5 * (x - xm) * (x - xm) / (sko * sko)) / (2.506628274631000502415765284811 * sko);
        }
        public double GetRandValue_Norm(double xm, double sko) {
            if (sko < Double.Epsilon)
                return xm >= Left && xm <= Right ? xm : GetRandValue();

            double amplit = xm >= Left && xm <= Right ?
                Gauss(xm, xm, sko) :
                Max(Gauss(Left, xm, sko), Gauss(Right, xm, sko));
            double x1, h, f;
            do {
                x1 = GetRandValue();
                h = RandomizationProvider.Current.GetDouble();
                f = Gauss(x1, xm, sko) / amplit;
            } while (f <= h);
            return x1;
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

    }

    //public struct GeneDouble {
    //    private double _value;
    //    public DoubleRange Range { get; private set; }
    //    public string Name {
    //        get {
    //            return Range.Name;
    //        }
    //    }
    //    public double Value {
    //        get { return _value; }
    //        set {
    //            _value =
    //                value > Range.Max ? Range.Max :
    //                value < Range.Min ? Range.Min :
    //                value;
    //        }
    //    }
    //    public GeneDouble(DoubleRange range, double value) {
    //        Range = range;
    //        _value =
    //            value > Range.Max ? Range.Max :
    //            value < Range.Min ? Range.Min :
    //            value;
    //    }
    //}

    public class StringRange : IGeneInfo {
        private List<string> _elements;
        public string Name { get; set; }
        public dynamic GetRandValue() {
            return RandomizationProvider.Current.GetInt(0, Count);
        }
        public StringRange(string name, params string[] elements) {
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

        public string this[int index] {
            get {
                return _elements[index];
            }
        }
    }

    //public struct GeneEnum {
    //    private int curElemIndex;
    //    public StringEnum Elem { get; private set; }
    //    public string Name {
    //        get {
    //            return Elem.Name;

    //        }
    //    }
    //    public string Value {
    //        get {
    //            return Elem[curElemIndex];
    //        }

    //        set {
    //            if (Elem.Contains(value)) {
    //                curElemIndex = Elem.IndexOf(value);
    //            }
    //        }
    //    }
    //    public int EnumIndex {
    //        get {
    //            return curElemIndex;
    //        }

    //        set {
    //            if (Elem.IndexValidate(value))
    //                curElemIndex = value;
    //        }
    //    }
    //    public GeneEnum(StringEnum myEnum, int enumIndex = 0) {
    //        Elem = myEnum;
    //        EnumIndex = enumIndex;
    //    }
    //}

    public enum FitnessExtremum { fe_min, fe_max };

    public class FitnesInfo {
        public FitnessExtremum Extremum { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string Name { get; set; }
        public FitnesInfo(string name, FitnessExtremum extr = FitnessExtremum.fe_max, double? min = null, double? max = null) {
            Name = name;
            Extremum = extr;
            Min = min;
            Max = max;
        }
    }


    public class ChromosomeDE : ChromosomeBase {
        private IList<IGeneInfo> _gInfo;
        public IList<IGeneInfo> GInfo { get { return _gInfo; } }

        public Dictionary<string, double?> MultiFitnes { get; set; }
        public Dictionary<string, FitnesInfo> FInfo { get; set; }

        public ChromosomeDE(IList<IGeneInfo> geneInfos) : base(geneInfos.Count()) {
            MultiFitnes = new Dictionary<string, double?>();
            _gInfo = geneInfos;
            CreateGenes();
        }

        public override IChromosome CreateNew() {
            return new ChromosomeDE(_gInfo);
        }

        public override Gene GenerateGene(int geneIndex) {
            return new Gene(_gInfo[geneIndex].GetRandValue());
            throw new Exception($"Чет не так с типом или индексом гена( {nameof(ChromosomeDE)} - GenerateGene Method");
        }

        public override IChromosome Clone() {
            var clone = base.Clone() as ChromosomeDE;
            foreach (var key in MultiFitnes.Keys) {
                clone.MultiFitnes.Add(key, MultiFitnes[key]);
            }
            clone.FInfo = FInfo;
            return clone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>1 - first cooler by pareto, 0 - pareto ==, -1 second cooler</returns>
        public static int ParetoRel(ChromosomeDE first, ChromosomeDE second) {
            bool firstCooler = true; ;
            bool secondCooler = true;
            foreach (var fiName in first.FInfo.Keys) {
                var kritBattle = first.FInfo[fiName].Extremum == FitnessExtremum.fe_max ?
                    first.MultiFitnes[fiName] > second.MultiFitnes[fiName] :
                    first.MultiFitnes[fiName] < second.MultiFitnes[fiName];
                firstCooler &= kritBattle;
                secondCooler &= !kritBattle;
                if ((!firstCooler) && (!secondCooler))
                    break;
            }
            return firstCooler ? 1 :
                secondCooler ? -1 :
                0;
        }
    }

}
