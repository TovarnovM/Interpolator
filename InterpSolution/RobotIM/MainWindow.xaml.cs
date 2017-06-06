using Interpolator;
using MyRandomGenerator;
using RobotIM.Core;
using RobotIM.Scene;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
            l.dT = 0.01;
            r = GetRoom();
            for (int i = 0; i <130; i++) {
                var u = new TerrorTest($"unit{i}", r);
                l.AddUnit(u);
            }


            //var u = new UnitWithStates("unit");
            //u.SM.Configure(UnitState.Factory(u,"moving"))
            //    .InternalTransition()

            //r = GetRoom();
            //l.AddUnit(u);
            //u.X = 3;
            //u.Y = 3;
            //u.WayPoints = new WayPoints(r.FindPath(u.Pos, new Vector2D(17, 4)));
            //l.EnableAllUnits();

            var tsm = new Stateless.StateMachine<string, string>("2");
            tsm.Configure("1")
                .SubstateOf("2")
                .OnEntry(()=>tsm.Fire("1end"))
                .Permit("1end", "end");
            tsm.Configure("2")
                .SubstateOf("3")
                .Permit("2end", "end")
                .Permit("21","1");
            tsm.Configure("3")
                .Permit("3end", "end")
                .Permit("ss","1");

            var gg = tsm.PermittedTriggers.ToList();
            tsm.Fire("21");


            var tstterror = new TerroristUnit("eee");

            return l;
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            if (timer == null) {
                timer = new System.Timers.Timer(50);
                timer.Elapsed += (s, ee) => {
                    for (int i = 0; i < 30; i++) {
                        mainLoop.StepUp();
                    }
                    vm.Model1Rx.Update(mainLoop);
                };
                timer.Enabled = true;

            } else {
                timer.Enabled = !timer.Enabled;
            }


        }

        System.Timers.Timer timer = null;

        private void Button_Click(object sender, RoutedEventArgs e) {
            //var bf = new BinaryFormatter();
            using (var fs = new StreamWriter(@"C:\Users\User\Desktop\log.txt")) {
                fs.Write(mainLoop.Logger.Text);

            }
        }
    }
    [Serializable]
    class TerrorTest : UnitWithStates {
        public TerrorTest(string Name, Room r, GameLoop Owner = null) : base(Name, Owner) {
            _r = r;
            var utComeToTarget = new UnitTrigger("ComeToTarget");
            utComeToTarget.ConditionFunc += Ut1Cond;

            var utSpinStop = new UnitTrigger("StopSpin");
            utSpinStop.ConditionFunc += SpinProc;

            var utFindTarg = new UnitTrigger("FindTarget");
            utFindTarg.ConditionFunc += ScanForTargets;

            var utKillSomeone = new UnitTrigger("KillSomeOne");
            utKillSomeone.ConditionFunc += () => _killedSomone;

            var utKilled = new UnitTrigger("Killed");
            utKilled.ConditionFunc += () => {
                while(MessQueue.Count > 0) {
                    var m = MessQueue.Dequeue();
                    if (m.name == "BANG")
                        return true;

                }
                return false;

            };

            State = UnitState.Factory(this, "moving");
            State.WhatToDo += SignToVel;

            var spinning = UnitState.Factory(this, "scaning");

            var shootingState = new UnitState(this, "shooting");
            shootStream = new ActionStream(2, 0.5);
            shootStream.HitAction += Shoot;
            shootingState.WhatToDo += tt => shootStream.Hit(tt);

            var deadState = new UnitState(this, "dead");
            var liveState = new UnitState(this, "live");
            SM.Configure(State)
                // .InternalTransition(ut1, GetNewDistPos)
                .OnEntry(() => {
                    GetNewDistPos();
                    VelAbs = _rnd.GetDouble(0.5, 1.5);
                })
                .SubstateOf(liveState)
                .Permit(utComeToTarget, spinning)
                .Permit(utFindTarg, shootingState);

            SM.Configure(spinning)
                .OnEntry(() => {
                    spinStartTime = UnitTime;
                    RotDir = _rnd.GetInt(-2, 2);
                    rotateSpeed = _rnd.GetDouble(20, 90);
                    spinDur = _rnd.GetDouble(5, 10);
                })
                .SubstateOf(liveState)
                .Permit(utSpinStop, State)
                .Permit(utFindTarg, shootingState);

            SM.Configure(shootingState)
                .OnEntry(() => {
                    _killedSomone = false;

                    shootStream.Reset(UnitTime);
                })
                .SubstateOf(liveState)
                .Permit(utKillSomeone, State);

            SM.Configure(liveState)
                .Permit(utKilled, deadState);

            SM.Configure(deadState)
                .OnEntry(() => {
                    this.Enabled = false;
                    //  MessageBox.Show("Bang(");
                });

            Pos = r.GetWalkableCoord();
            GetNewDistPos();
            do {
                viewDir.X = _rnd.GetDouble(-1, 1);
                viewDir.Y = _rnd.GetDouble(-1, 1);
                viewDir.Normalize();
            } while (viewDir.GetLength() < 0.999999);


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

        UnitXY _myTarget;
        bool _killedSomone = false;
        bool ScanForTargets() {
            foreach (var pt in Owner.GetUnitsSpec<UnitXY>(false)) {
                if (Object.ReferenceEquals(pt, this))
                    continue;
                if (!pt.Enabled)
                    continue;
                if (SeeYou(pt.Pos, _r)) {
                    _myTarget = pt;
                 //   MessageBox.Show("ss");
                    break;
                }
            }
            return _myTarget != null;
        }

        ActionStream shootStream;
        void Shoot(double t) {
            if (_myTarget == null)
                return;
            var dist = (_myTarget.Pos - Pos).GetLength();
            double dist0 = 20, prob0 = 0.5;
            var prob = 1 - (1 - prob0) * dist / dist0;
            if(_rnd.GetDouble() < prob) {
                _myTarget.SendMessageToMe(new UnitMessage() { name = "BANG", from = this });
                _killedSomone = true;
                _myTarget = null;
            }
        }

    }
}
