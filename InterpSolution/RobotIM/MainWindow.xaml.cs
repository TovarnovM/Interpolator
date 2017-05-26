using Interpolator;
using MyRandomGenerator;
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
            r = GetRoom();
            var u = new TerrorTest("u1", r);
            l.AddUnit(u);
            //var u = new UnitWithStates("unit");
            //u.SM.Configure(UnitState.Factory(u,"moving"))
            //    .InternalTransition()

            //r = GetRoom();
            //l.AddUnit(u);
            //u.X = 3;
            //u.Y = 3;
            //u.WayPoints = new WayPoints(r.FindPath(u.Pos, new Vector2D(17, 4)));
            //l.EnableAllUnits();
            return l;
        }


        private void button1_Click(object sender, RoutedEventArgs e) {
                for (int i = 0; i < 10; i++) {
                    mainLoop.StepUp();
                }
                vm.Model1Rx.Update(mainLoop);
            

        }
    }

    class TerrorTest : UnitWithStates {
        public TerrorTest(string Name, Room r, GameLoop Owner = null) : base(Name, Owner) {
            _r = r;
            var ut1 = new UnitTrigger();
            ut1.ConditionFunc += Ut1Cond;

            var ut2 = new UnitTrigger();
            ut2.ConditionFunc += SpinProc;
            X = 1;
            Y = 1;
            GetNewDistPos();
            
            State = UnitState.Factory(this, "moving");
            State.WhatToDo += SignToVel;
            var spinning = UnitState.Factory(this, "scaning");
            SM.Configure(State)
               // .InternalTransition(ut1, GetNewDistPos)
                .OnEntry(()=> {
                    GetNewDistPos();
                    VelAbs = _rnd.GetDouble(0.5, 1.5);
                })
                .Permit(ut1, spinning);
            SM.Configure(spinning)
                .OnEntry(()=> {
                    spinStartTime = UnitTime;
                    RotDir = _rnd.GetInt(-2, 2);
                    rotateSpeed = _rnd.GetDouble(20, 90);
                    spinDur = _rnd.GetDouble(5, 10);
                })
                .Permit(ut2, State);
        }
        Room _r;
        Vector2D _distPoint;
        MyRandom _rnd = new MyRandom();
        bool Ut1Cond() {
            return (Pos - _distPoint).GetLength() < 0.01;
        }

        void GetNewDistPos() {
            var p = new Vector2D();
            do {
                p.X = _rnd.GetDouble(_r.gabarit.p1.X, _r.gabarit.p2.X);

                p.Y = _rnd.GetDouble(_r.gabarit.p1.Y, _r.gabarit.p2.Y);
                
            } while (!_r.WalkableCoord(p));
            WayPoints = new WayPoints(_r.FindPath(Pos, p));
            _distPoint = p;
        }

        double spinStartTime = 0d, spinDur = 5;
        bool SpinProc() {
            return UnitTime - spinStartTime > spinDur;
        }

    }
}
