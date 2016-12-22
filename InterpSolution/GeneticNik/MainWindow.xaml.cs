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

namespace GeneticNik {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        VMgenetic vm;
        GeneticAlgorithm ga;
        IDisposable subscr = null;
        FitnessNik fit;

        public MainWindow() {
            vm = new VMgenetic();
            DataContext = vm;
            InitializeComponent();
            fit = new FitnessNik();
            lbY.ItemsSource = fit.GInfo.Select(gi => gi.Name);
            lbX.ItemsSource = fit.GInfo.Select(gi => gi.Name);
            //ga.Start();


        }

        void startNew() {
            subscr?.Dispose();
            ga?.Stop();

            

            var adam = fit.GetNewChromosome();

            var pop = new PopulationRx(200,250,adam);

            var selection = new TournamentSelection(4,true);

            var cross = new CrossoverD(0.7);

            var mutation = new MutationD();

            ga = new GeneticAlgorithm(pop,fit,selection,cross,mutation);
            ga.Termination = new FitnessStagnationTermination(80); //GenerationNumberTermination(10);
            var taskEx = new SmartThreadPoolTaskExecutor();
            taskEx.MinThreads = 2;
            taskEx.MaxThreads = Environment.ProcessorCount;
            ga.TaskExecutor = taskEx;
            ga.TerminationReached += Ga_TerminationReached;

            subscr = pop.ObserveOnDispatcher().Subscribe(g => {
                button.Content = g.Number.ToString() + "   " + ((int)g.BestChromosome.Fitness).ToString();
                vm.PM_params_Rx.Update(g);
            });



            if(ga.State == GeneticAlgorithmState.NotStarted)
                Task.Factory.StartNew(ga.Start,TaskCreationOptions.LongRunning);

        }

        private void Ga_TerminationReached(object sender,EventArgs e) {
            MessageBox.Show("End!");
        }

        private void button_Click(object sender,RoutedEventArgs e) {

            startNew();
        }

        private void button_Save_Click(object sender,RoutedEventArgs e) {

        }

        private void button_Copy1_Click(object sender,RoutedEventArgs e) {
            
        }
    }
}
