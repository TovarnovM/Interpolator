using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic.DetermSearch {
    public class DetermPopulation : Population {
        int _shagNumber = 1000;
        public Dictionary<string,double> shagDict;
        public List<GeneDoubleRange> geneInfo;
        public int ShagNumber {
            get {
                return _shagNumber;
            }
            set {
                _shagNumber = value;
                UpdateShag();
            }
        }

        
        public List<ChromosomeD> Solutions;

        public DetermPopulation(ChromosomeD adamChromosome) : base(2,adamChromosome.GetMatterGenes().Count() * 2 + 1,adamChromosome) {
            shagDict = new Dictionary<string,double>(adamChromosome.GInfoDouble.Count);
            geneInfo = adamChromosome
                .GetMatterGenes()
                .Where(gi => gi is GeneDoubleRange)
                .Cast<GeneDoubleRange>()
                .ToList();
            foreach(var igene in geneInfo) {
                shagDict.Add(igene.Name,0d);
            }
            UpdateShag();
            Solutions = new List<ChromosomeD>();
            Solutions.Add(adamChromosome);
            GenerationStrategy = new TrackingGenerationStrategy();
        }
        void UpdateShag() {
            foreach(var igene in geneInfo) {
                shagDict[igene.Name] = (igene.Right - igene.Left) / ShagNumber;
            }
        }


        public override void CreateInitialGeneration() {
            Generations = new List<Generation>();
            GenerationsNumber = 0;

            var chromosomes = GetNeibsPlusCenter(AdamChromosome as ChromosomeD);

            CreateNewGeneration(chromosomes.Cast<IChromosome>().ToList());
        }

        public override void CreateNewGeneration(IList<IChromosome> chromosomes) {
            base.CreateNewGeneration(chromosomes);
            Solutions.Add(chromosomes.Cast<ChromosomeD>().First(ch => ch.DopInfo == null));
        }

        public IList<ChromosomeD> GetNeibsPlusCenter(ChromosomeD center) {
            var res = new List<ChromosomeD>(geneInfo.Count + 1);
            res.Add(center);
            foreach(var gi in geneInfo) {
                var bigger = center.CloneWithoutFitness();
                double val = bigger[gi.Name] + shagDict[gi.Name];
                if(gi.ValidateValue(val)) {
                    bigger[gi.Name] = val;
                    bigger.DopInfo = new DopInfoClass() {
                        Name = gi.Name,
                        diff = 1d

                    };
                    res.Add(bigger);
                }


                var smaller = center.CloneWithoutFitness();
                val = bigger[gi.Name] - shagDict[gi.Name];
                if(gi.ValidateValue(val)) {
                    smaller[gi.Name] = val;
                    smaller.DopInfo = new DopInfoClass() {
                        Name = gi.Name,
                        diff = -1d
                    };
                    res.Add(smaller);
                }
            }
            return res;
        }

    }

    public class DopInfoClass {
        public string Name;
        public double diff = 0d;
    }
}
