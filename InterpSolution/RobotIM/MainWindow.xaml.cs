﻿using Interpolator;
using MyRandomGenerator;
using Newtonsoft.Json;
using RobotIM.Core;
using RobotIM.IM;
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
        public ViewModel vm { get; set; }
        public vmTrg vmT { get; set; }
        public vmTrg_IM vmT_IM { get; set; }
        GameLoop mainLoop;
        Room r;
        public MainWindow() {
            vm = new ViewModel();
            vmT = new vmTrg();
            vmT_IM = new vmTrg_IM();
            DataContext = this;
            InitializeComponent();
            mainLoop = InitLoop();
            vm.DrawRoom(r,false);
        }

        public Room GetRoom() {
            var rr = new Room();

            rr.walls = RoomGenerator.GetWalls(30, 40, 7, 7);
            rr.cellsize = 0.4;
            rr.CreateScene();
            return rr;

            
            //r.gabarit = (new Vector2D(0, 0), new Vector2D(20, 20));
            
            var wall = new LevelLine();
            wall.AddPoint(0, 0);
            wall.AddPoint(0, 20);
            wall.AddPoint(20, 20);
            wall.AddPoint(20, 0);
            wall.AddPoint(0, 0);
            rr.walls.Add(wall);

            wall = new LevelLine();
            wall.AddPoint(10, 0);
            wall.AddPoint(10, 15);
            wall.AddPoint(4, 15);
            rr.walls.Add(wall);

            wall = new LevelLine();
            wall.AddPoint(0, 10);
            wall.AddPoint(6, 10);
            rr.walls.Add(wall);

            rr.CreateScene();
            return rr;
        }

        public GameLoop InitLoop() {
            var l = new GameLoop();
            l.dT = 0.02;
            r = GetRoom();
            //for (int i = 0; i <130; i++) {
            //    var u = new TerrorTest($"unit{i}", r);
            //    l.AddUnit(u);
            //}


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


            var tstterror = new TerroristUnit("eee",r);
            l.AddUnit(tstterror);
            var immr = new IMMR("robot", r);
            l.AddUnit(immr);
            immr.Pos = r.GetWalkableCoord();
            tstterror.UnitNoises.Add(immr);

            //staticnoises = new List<INoisePoint>();
            //staticnoises.Add(new StaticNoisePoint(new Vector2D(2, 2), 30));
            //staticnoises.Add(new StaticNoisePoint(new Vector2D(25, 2), 10));
            //staticnoises.Add(new StaticNoisePoint(new Vector2D(25, 18), 30));
            //staticnoises.Add(new StaticNoisePoint(new Vector2D(2, 18), 30));
            //r.staticNoisesList = staticnoises;
            //r.InitNoiseMap();
            //tstterror.BackgroundNoise = staticnoises;


            return l;
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            if (timer == null) {
                timer = new System.Timers.Timer(100);
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
        private List<INoisePoint> staticnoises;

        private void Button_Click(object sender, RoutedEventArgs e) {
            //var bf = new BinaryFormatter();
            using (var fs = new StreamWriter(@"C:\Users\User\Desktop\log.txt")) {
                fs.Write(mainLoop.Logger.Text);

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            mainLoop = InitLoop();
            vm.DrawRoom(r, true);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            vm.DrawRoom(r, true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            vm.DrawRoom(r, false);
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e) {
         
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            vmT.DrawAim(vmT.Pm, Target.Factory("fire"));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e) {
            var exp = new Experim1();
            exp.Start();
            vmT.DrawAim(vmT.Pm, exp.Targ,exp.Info.ResultIndex,exp.Info.N_rounds, exp.Info.N_rounds, exp.Info.Prob);
        }

        private async void btn_IM1_Click(object sender, RoutedEventArgs e) {
            try {
                var sd = new Microsoft.Win32.SaveFileDialog() {
                    Filter = "infoList Files|*.json",
                    FileName = "Infos"
                };
                if (sd.ShowDialog() == true) {
                    btn_IM1.IsEnabled = false;
                    await StartCalcAsync();
                    using (var f = new StreamWriter(sd.FileName)) {
                        f.WriteLine(JsonConvert.SerializeObject(infoList));
                        f.Close();
                    }
                    vmT_IM.Print(infoList);
                }
                

            } finally {
                btn_IM1.IsEnabled = true;
            }
            

        }
        Task StartCalcAsync() {
            return Task.Factory.StartNew(StartCalc, TaskCreationOptions.LongRunning);
        }
        void StartCalc() {
            double h = 0;
            double fi0 = 0.001, fi1 = 90, fi_shag = 14.99;
            double r0 = 3, r1 = 50, r_shag = 3;

            int i_max = (int)((fi1 - fi0) / fi_shag) + 1;
            int j_max = (int)((r1 - r0) / r_shag) + 1;
            int ind_id = 1000;
            infoList.Clear();
            for (int i = 0; i < i_max; i++) {
                for (int j = 0; j < j_max; j++) {
                    var exp_info = new Experim1Info(3) {
                        id = ind_id++,
                        I_ind = i,
                        J_ind = j,
                        TrgTetta0 = fi0 + fi_shag * i,
                        TrgR0 = r0 + j * r_shag,

                    };
                    exp_info.Name = $"{exp_info.id}_{exp_info.Name}";
                    infoList.Add(exp_info);
                }
            }
            P_data = new double[i_max, j_max];
            progr_curr = 0;
            progr_max = infoList.Count;
            Parallel.ForEach(infoList, StartVar);
        }
        List<Experim1Info> infoList = new List<Experim1Info>();
        double[,] P_data;
        object _locker1 = new object(), _locker = new object();
        int progr_curr = 0, progr_max = 0;

        private void btn_IM1_Copy_Click(object sender, RoutedEventArgs e) {
            try {
                var sd = new Microsoft.Win32.OpenFileDialog() {
                    Filter = "infoList Files|*.json",
                    FileName = "Infos"
                };
                if (sd.ShowDialog() == true) {
                    using (var f = new StreamReader(sd.FileName)) {
                        infoList = JsonConvert.DeserializeObject<List<Experim1Info>>(f.ReadToEnd());
                        f.Close();
                    }
                    vmT_IM.Print(infoList);
                }


            } finally {
               
            }
        }

        void StartVar(Experim1Info info) {
            var exp = new Experim1();
            exp.Info = info;
            exp.Start();
            lock (_locker1) {
                P_data[info.I_ind, info.J_ind] = exp.Info.Prob;
            }
            Dispatcher.Invoke(new Action(() => {
                lock (_locker) {
                    progr_curr++;
                    btn_IM1.Content = $"Calc {progr_curr} / {progr_max}";
                }
            }));
        }
    }
    [Serializable]
    class TerrorTest : UnitWithStates {
        UnitTrigger utComeToTarget = new UnitTrigger("ComeToTarget");

        public TerrorTest(string Name, Room r, GameLoop Owner = null) : base(Name, Owner) {
            _r = r;

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

            InitMe();

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
        bool utComeToTarget_ConditionFunc() {
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
