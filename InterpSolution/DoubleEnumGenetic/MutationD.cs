using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Randomizations;
using HelperSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic {
    public class MutationD : MutationBase {
        #region Fields
        private int[] m_mutableGenesIndexes;

        private bool m_allGenesMutable;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSharp.Domain.Mutations.UniformMutation"/> class.
        /// </summary>
        /// <param name="mutableGenesIndexes">Mutable genes indexes.</param>
        public MutationD(params int[] mutableGenesIndexes) {
            m_mutableGenesIndexes = mutableGenesIndexes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSharp.Domain.Mutations.UniformMutation"/> class.
        /// </summary>
        /// <param name="allGenesMutable">If set to <c>true</c> all genes are mutable.</param>
        public MutationD(bool allGenesMutable) {
            m_allGenesMutable = allGenesMutable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSharp.Domain.Mutations.UniformMutation"/> class.
        /// </summary>
        /// <remarks>Creates an instance of UniformMutation where some random genes will be mutated.</remarks>
        public MutationD() : this(false) {
        }
        #endregion

        public double Radius { get; set; } = 1.0;

        #region Methods
        /// <summary>
        /// Mutate the specified chromosome.
        /// </summary>
        /// <param name="chromosome">The chromosome.</param>
        /// <param name="probability">The probability to mutate each chromosome.</param>
        protected override void PerformMutate(IChromosome chromosome,float probability) {
            ExceptionHelper.ThrowIfNull("chromosome",chromosome);

            var genesLength = chromosome.Length;

            if(m_mutableGenesIndexes == null || m_mutableGenesIndexes.Length == 0) {
                if(m_allGenesMutable) {
                    m_mutableGenesIndexes = Enumerable.Range(0,genesLength).ToArray();
                } else {
                    m_mutableGenesIndexes = RandomizationProvider.Current.GetInts(1,0,genesLength);
                }
            }

            foreach(var i in m_mutableGenesIndexes) {
                if(i >= genesLength) {
                    throw new MutationException(this,"The chromosome has no gene on index {0}. The chromosome genes length is {1}.".With(i,genesLength));
                }

                if(RandomizationProvider.Current.GetDouble() <= probability) {
                    if(chromosome is ChromosomeD && ((chromosome as ChromosomeD).GInfoDouble[i] is GeneDoubleRange)) {
                        var gi = (chromosome as ChromosomeD).GInfoDouble[i] as GeneDoubleRange;
                        var center = (double)chromosome.GetGene(i).Value;
                        var radius = Math.Abs(gi.Right - gi.Left) * Radius;
                        chromosome.ReplaceGene(i,new Gene(gi.GetRandValue_Uniform(center - radius,center + radius)));
                        continue;
                    }

                    chromosome.ReplaceGene(i,chromosome.GenerateGene(i));
                }
            }
        }
        #endregion
    }
}
