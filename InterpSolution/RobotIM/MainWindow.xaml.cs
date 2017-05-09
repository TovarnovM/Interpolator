using Interpolator;
using RobotIM.Core;
using RobotIM.Scene;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace RobotIM {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        ViewModel vm;
        GameLoop mainLoop;
        Room r;
        public MainWindow() {
            vm = new ViewModel();
            DataContext = vm;
            InitializeComponent();
            mainLoop = InitLoop();
            vm.DrawRoom(r);
        }

        public Room GetRoom() {
            var r = new Room();
            r.gabarit = (new Vector2D(0, 0), new Vector2D(20, 20));
            r.cellsize = 0.3;

            var wall = new LevelLine();
            wall.AddPoint(0, 0);
            wall.AddPoint(0, 20);
            wall.AddPoint(20, 20);
            wall.AddPoint(20, 0);
            wall.AddPoint(0, 0);
            r.walls.Add(wall);

            wall = new LevelLine();
            wall.AddPoint(10, 0);
            wall.AddPoint(10, 15);
            wall.AddPoint(4, 15);
            r.walls.Add(wall);

            wall = new LevelLine();
            wall.AddPoint(0, 10);
            wall.AddPoint(6, 10);
            r.walls.Add(wall);

            r.CreateScene();
            return r;
        }

        public GameLoop InitLoop() {
            var l = new GameLoop();
            l.dT = 0.1;
            var u = new UnitWithVision("unit");
            r = GetRoom();
            l.AddUnit(u);
            u.X = 3;
            u.Y = 3;
            u.WayPoints = new WayPoints(r.FindPath(u.Pos, new Vector2D(17, 4)));
            l.EnableAllUnits();
            return l;
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
                for (int i = 0; i < 1; i++) {
                    mainLoop.StepUp();
                }
                vm.Model1Rx.Update(mainLoop);
            

        }
    }
}
