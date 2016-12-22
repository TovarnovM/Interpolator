using DoubleEnumGenetic;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Infrastructure.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeneticSharp.Domain.Terminations;
using System.Reactive.Linq;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;

namespace GeneticNik {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        VMgenetic vm;
        GeneticAlgorithm ga;
        IDisposable subscrP = null, subDG = null, subF = null, subSX = null, subSY = null;
        FitnessNik fit;

        List<Generation> GenerList;
        IObservable<int> trackbarch;
        IObservable<string> lbXch;
        IObservable<string> lbYch;

        public MainWindow() {
            vm = new VMgenetic();
            DataContext = vm;
            InitializeComponent();
            fit = new FitnessNik();
            GenerList = new List<Generation>(100);
            lbY.ItemsSource = fit.GetAllNames();
            lbX.ItemsSource = fit.GetAllNames();
            //ga.Start();
            trackbarch = Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(slider,"ValueChanged").Select(i => (int)i.EventArgs.NewValue);
            lbXch = Observable.FromEventPattern<SelectionChangedEventArgs>(lbX,"SelectionChanged").Select(e => e.EventArgs.AddedItems[0].ToString());
            lbYch = Observable.FromEventPattern<SelectionChangedEventArgs>(lbY,"SelectionChanged").Select(e => e.EventArgs.AddedItems[0].ToString());

        }

        void startNew() {
            subscrP?.Dispose();
            subDG?.Dispose();
            subF?.Dispose();
            subSX?.Dispose();
            subSY?.Dispose();
            ga?.Stop();

            

            var adam = fit.GetNewChromosome();
            int n = int.Parse(textBox.Text);
            var pop = new PopulationRx(n,n+20,adam);

            var selection = new TournamentSelection(2,true);

            var cross = new CrossoverD(0.5);

            var mutation = new MutationD();

            ga = new GeneticAlgorithm(pop,fit,selection,cross,mutation);
            ga.Termination = new FitnessStagnationTermination(20); //GenerationNumberTermination(10);
            var taskEx = new SmartThreadPoolTaskExecutor();
            taskEx.MinThreads = 2;
            taskEx.MaxThreads = Environment.ProcessorCount;
            ga.TaskExecutor = taskEx;
            ga.Reinsertion = new ElitistReinsertion();
            ga.TerminationReached += Ga_TerminationReached;
            ga.MutationProbability = 0.4f;
            

            subscrP = pop.ObserveOnDispatcher().Subscribe(g => {
                GenerList.Add(g);

                var sliderMax = slider.Value == slider.Maximum;
                slider.Maximum = GenerList.Count - 1;
                if(sliderMax)
                    slider.Value = slider.Maximum;
                button.Content = g.Number.ToString() + "   " + ((int)g.BestChromosome.Fitness).ToString();
                
                vm.PM_fitness_Rx.Update(GenerList);
            });         

            subDG = trackbarch.Subscribe(i => {
                updateDG(i);
                vm.PM_params_Rx.Update(GenerList[i]);
            });

            subSX = lbXch.StartWith("Vd").Subscribe(s => {
                vm.sX = s;
                if(GenerList.Count == 0)
                    return;
                vm.PM_params_Rx.Update(GenerList[(int)slider.Value]);
                vm.PM_params_Rx.Value.ResetAllAxes();
                vm.PM_params_Rx.Value.InvalidatePlot(false);
            });
            subSY = lbYch.StartWith("pmax").Subscribe(s => {
                vm.sY = s;
                if(GenerList.Count == 0)
                    return;
                vm.PM_params_Rx.Update(GenerList[(int)slider.Value]);
                vm.PM_params_Rx.Value.ResetAllAxes();
                vm.PM_params_Rx.Value.InvalidatePlot(false);
            });


            if(ga.State == GeneticAlgorithmState.NotStarted)
                Task.Factory.StartNew(ga.Start,TaskCreationOptions.LongRunning);

        }

        void updateDG(int ind) {
            dataGrid.ItemsSource = GenerList[ind].Chromosomes.Select(cr => {
                var c = cr as ChromosomeD;
                if(c == null)
                    return null;
                return new {
                    Lcone = c["Lcone"],
                    dout = c["dout"],
                    Lpiston = c["Lpiston"],
                    m1 = c["m1"],
                    m2 = c["m2"],
                    Vd = c["Vd"],
                    pmax = c["pmax"],
                    Fitness = c.Fitness ?? 0

                };
            });
        }

        private void Ga_TerminationReached(object sender,EventArgs e) {
            MessageBox.Show("End!");
        }

        private void button_Click(object sender,RoutedEventArgs e) {

            startNew();
        }

        private void lbY_SelectionChanged(object sender,SelectionChangedEventArgs e) {

        }

        private void button_Save_Click(object sender,RoutedEventArgs e) {

        }

        private void button_Copy1_Click(object sender,RoutedEventArgs e) {
            
        }
    }
}
