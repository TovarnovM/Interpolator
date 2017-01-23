using GeneticSharp.Domain;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Infrastructure.Framework.Threading;
using GeneticSharp.Infrastructure.Threading;
using HelperSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace DoubleEnumGenetic.DetermOptimization {



    public class Optimizator:IGeneticAlgorithm {
        #region Fields
        private bool m_stopRequested;
        private object m_lock = new object();
        private GeneticAlgorithmState m_state;
        #endregion  

        public Optimizator(ChromosomeD startPoint, IFitness Fitness, ISearchMethod searchMethod) {
            this.SearchMethod = searchMethod;
            this.Fitness = Fitness;
            this.StartPoint = startPoint;
            TaskExecutor = new SmartThreadPoolTaskExecutor();

        }

        public ISearchMethod SearchMethod { get; private set; }

        public ChromosomeD StartPoint { get; private set; }
        public ITaskExecutor TaskExecutor { get; set; }
        /// <summary>
        /// Gets the state.
        /// </summary>
        public GeneticAlgorithmState State {
            get {
                return m_state;
            }

            private set {
                var shouldStop = Stopped != null && m_state != value && value == GeneticAlgorithmState.Stopped;

                m_state = value;

                if(shouldStop) {
                    Stopped(this,EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets the time evolving.
        /// </summary>
        public TimeSpan TimeEvolving { get; private set; }
        /// <summary>
        /// Gets the fitness function.
        /// </summary>
        public IFitness Fitness { get; private set; }

        public int GenerationsNumber {
            get {
                return SearchMethod.Solutions.Count;
            }
        }

        public IChromosome BestChromosome {
            get {
                return SearchMethod.BestSolution;
            }
        }


        #region Events
        /// <summary>
        /// Occurs when generation ran.
        /// </summary>
        public event EventHandler GenerationRan;

        /// <summary>
        /// Occurs when termination reached.
        /// </summary>
        public event EventHandler TerminationReached;

        /// <summary>
        /// Occurs when stopped.
        /// </summary>
        public event EventHandler Stopped;
        #endregion

        #region Methods

        /// <summary>
        /// Starts the genetic algorithm using population, fitness, selection, crossover, mutation and termination configured.
        /// </summary>
        public void Start() {
            lock(m_lock) {
                State = GeneticAlgorithmState.Started;
                var startDateTime = DateTime.Now;
                SearchMethod.FirstCalculation(StartPoint);
                TimeEvolving = DateTime.Now - startDateTime;
            }

            Resume();
        }

        /// <summary>
        /// Resumes the last evolution of the genetic algorithm.
        /// <remarks>
        /// If genetic algorithm was not explicit Stop (calling Stop method), you will need provide a new extended Termination.
        /// </remarks>
        /// </summary>
        public void Resume() {
            try {
                lock(m_lock) {
                    m_stopRequested = false;
                }

                if(SearchMethod.Solutions.Count == 0) {
                    throw new InvalidOperationException("Attempt to resume a genetic algorithm which was not yet started.");
                } else if(SearchMethod.Solutions.Count> 0) {
                    if(SearchMethod.HasReached()) {
                        throw new InvalidOperationException("Attempt to resume a genetic algorithm with a termination ({0}) already reached. Please, specify a new termination or extend the current one.");
                    }

                    State = GeneticAlgorithmState.Resumed;
                }

                if(EndCurrentGeneration()) {
                    return;
                }

                bool terminationConditionReached = false;
                DateTime startDateTime;

                do {
                    if(m_stopRequested) {
                        break;
                    }

                    startDateTime = DateTime.Now;
                    terminationConditionReached = EndCurrentGeneration();
                    TimeEvolving += DateTime.Now - startDateTime;
                }
                while(!terminationConditionReached);
            }
            catch {
                State = GeneticAlgorithmState.Stopped;
                throw;
            }
        }

        /// <summary>
        /// Stops the genetic algorithm..
        /// </summary>
        public void Stop() {
            lock(m_lock) {
                m_stopRequested = true;
            }
        }


        /// <summary>
        /// Ends the current generation.
        /// </summary>
        /// <returns><c>true</c>, if current generation was ended, <c>false</c> otherwise.</returns>
        private bool EndCurrentGeneration() {
            EvaluateFitness();
            SearchMethod.EndCurrentStep();

            GenerationRan?.Invoke(this,EventArgs.Empty);

            if(SearchMethod.HasReached()) {
                State = GeneticAlgorithmState.TerminationReached;

                TerminationReached?.Invoke(this,EventArgs.Empty);

                return true;
            }

            if(m_stopRequested) {
                TaskExecutor.Stop();
                State = GeneticAlgorithmState.Stopped;
            }

            return false;
        }

        /// <summary>
        /// Evaluates the fitness.
        /// </summary>
        private void EvaluateFitness() {
            try {
                var chromosomesWithoutFitness = SearchMethod.WhatCalculateNext();

                for(int i = 0; i < chromosomesWithoutFitness.Count; i++) {
                    var c = chromosomesWithoutFitness[i];

                    TaskExecutor.Add(() => {
                        RunEvaluateFitness(c);
                    });
                }

                if(!TaskExecutor.Start()) {
                    throw new TimeoutException("The fitness evaluation rech the {0} timeout.".With(TaskExecutor.Timeout));
                }
            }
            finally {
                TaskExecutor.Stop();
                TaskExecutor.Clear();
            }
            
        }

        /// <summary>
        /// Runs the evaluate fitness.
        /// </summary>
        /// <returns>The evaluate fitness.</returns>
        /// <param name="chromosome">The chromosome.</param>
        private object RunEvaluateFitness(object chromosome) {
            var c = chromosome as ChromosomeD;

            try {
                c.Fitness = Fitness.Evaluate(c);
            }
            catch(Exception ex) {
                throw new FitnessException(Fitness,"Error executing Fitness.Evaluate for chromosome: {0}".With(ex.Message),ex);
            }

            return c.Fitness;
        }

        #endregion
    }


   
}
