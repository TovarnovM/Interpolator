using GeneticSharp.Domain;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Infrastructure.Threading;
using MultiGenetic;
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

namespace GeneticNik {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        VMgenetic vm;
        GeneticAlgorithm ga;
        FitnessNik fit;
        PopulationRx pop;

        public MainWindow() {
            vm = new VMgenetic();
            DataContext = vm;
            InitializeComponent();
            fit = new FitnessNik();

            var adam = fit.GetNewChromosome();

            pop = new PopulationRx(70,130,adam);

            var selection = new TournamentSelection(4,true);

            var cross = new CrossoverDE(0.7);

            var mutation = new MutationDE();

            ga = new GeneticAlgorithm(pop,fit,selection,cross,mutation);

            var taskEx = new SmartThreadPoolTaskExecutor();
            taskEx.MinThreads = 2;
            taskEx.MaxThreads = Environment.ProcessorCount;
            ga.TaskExecutor = taskEx;

            //ga.Start();
            

        }

        private void button_Click(object sender,RoutedEventArgs e) {
            pop.CreateInitialGeneration();
            var chromo = pop.CurrentGeneration.Chromosomes[0];

            fit.Evaluate(chromo);
            
            //ga.Start();
        }

        private void button_Save_Click(object sender,RoutedEventArgs e) {

        }

        private void button_Copy1_Click(object sender,RoutedEventArgs e) {
            
        }
    }
}
