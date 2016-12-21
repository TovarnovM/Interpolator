using GeneticSharp.Domain.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using System.Reactive.Subjects;

namespace GeneticNik {
    class PopulationRx : Population, IObservable<Generation> {
        Subject<Generation> subj;
        public PopulationRx(int minSize,int maxSize,IChromosome adamChromosome) : base(minSize,maxSize,adamChromosome) {
            subj = new Subject<Generation>();
        }
        public override void EndCurrentGeneration() {
            base.EndCurrentGeneration();
            subj.OnNext(CurrentGeneration);
        }

        public IDisposable Subscribe(IObserver<Generation> observer) {
            return subj.Subscribe(observer);
        }
    }
}
