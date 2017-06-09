using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotIM.Core;
using System.Reflection;
using System.Text.RegularExpressions;
using Stateless;
using MyRandomGenerator;
using Sharp3D.Math.Core;

namespace RobotIM.Scene {
    public class TerroristUnit : UnitWithStates {
        MyRandom _rnd = new MyRandom();
        void ConfigureMe() {
            try {

                SM.Configure(_1Alive)
                    .Permit(_20Immobilize, _2Immobilized)
                    .Permit(_9SeeYou, _8SeeSomething)
                    .Permit(_19Attacked, _11MakeDecision);

                SM.Configure(_3NormalBehavor3)
                    .SubstateOf(_1Alive)
                    .Permit(_3HearNoise, _7MoveToNoisePoint);

                SM.Configure(_4Patrool)
                    .SubstateOf(_3NormalBehavor3)
                    .OnActivate(_4Patrool_OnEntry)
                    .Permit(_1ReachDist, _51Rotate);

                SM.Configure(_5LookAround)
                    .SubstateOf(_3NormalBehavor3)
                    .Permit(_2TimeUp, _4Patrool);

                SM.Configure(_51Rotate)
                    .SubstateOf(_5LookAround)
                    .Permit(_22trig, _52Stay);

                SM.Configure(_52Stay)
                    .SubstateOf(_5LookAround)
                    .Permit(_23trig, _51Rotate);

                SM.Configure(_6SuspiciousBehavor)
                    .SubstateOf(_1Alive);

                SM.Configure(_7MoveToNoisePoint)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_6Come, _13SearchAround);

                SM.Configure(_8SeeSomething)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_10LostTarget, _9MakeDecision)
                    .Permit(_11RecognizeTimeUp, _10MakeDecision);

                SM.Configure(_9MakeDecision)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_13trig, _0RunToAlarm)
                    .Permit(_0trig, _14MoveToLastSpot);

                SM.Configure(_10MakeDecision)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_16trig, _0RunToAlarm)
                    .Permit(_21trig, _16Attacking)
                    .Permit(_8Curios, _15MoveToCurios);

                SM.Configure(_11MakeDecision)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_14trig, _0RunToAlarm)
                    .Permit(_15trig, _16Attacking);

                SM.Configure(_12MakeDecision)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_17trig, _0RunToAlarm)
                    .Permit(_18trig, _51Rotate);

                SM.Configure(_13SearchAround)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_7TimeUp, _12MakeDecision);

                SM.Configure(_14MoveToLastSpot)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_4HearNoise, _7MoveToNoisePoint)
                    .Permit(_5Come, _13SearchAround);

                SM.Configure(_15MoveToCurios)
                    .SubstateOf(_6SuspiciousBehavor)
                    .Permit(_12Recognized, _10MakeDecision);

                SM.Configure(_16Attacking)
                    .SubstateOf(_17Battle);

                SM.Configure(_0RunToAlarm)
                    .SubstateOf(_17Battle);

                SM.Configure(_17Battle)
                    .SubstateOf(_1Alive);

                State = _4Patrool;
                SM.Activate();
            } catch (Exception e) {

                throw e;
            }
        }

        #region states
        
        UnitState _1Alive            = new UnitState(null, nameof(_1Alive));
        UnitState _2Immobilized      = new UnitState(null, nameof(_2Immobilized));
        UnitState _3NormalBehavor3   = new UnitState(null, nameof(_3NormalBehavor3));
        UnitState _4Patrool          = new UnitState(null, nameof(_4Patrool));
        void _4Patrool_OnEntry() {
            var distVec = _r.GetWalkableCoord();
            WayPoints = new WayPoints(_r.FindPath(Pos, distVec));
        }
        void _4Patrool_WTD(double t2) {
            Move(t2);
            SignToVel(t2);         
        }
        UnitState _5LookAround       = new UnitState(null, nameof(_5LookAround));
        double 
            rotTime0, rotTime, rotTimeMin = 2, rotTimeMax = 5, rotateSpeed0 = 30 , rotSpeedSKO = 7; 
        int nSwitch = 0, nSwitchmax = 5;
        void _5LookAround_OnEntry() {
            RotDir = _rnd.GetInt(-2, 2);
            nSwitch = 0;
            nSwitchmax = _rnd.GetInt(3, 7);          
        }

        UnitState _51Rotate = new UnitState(null, nameof(_51Rotate));
        void _51Rotate_OnEntry() {
            rotTime0 = UnitTime;
            rotTime = _rnd.GetDouble(rotTimeMin, rotTimeMax);
            RotDir *= _rnd.GetInt(-2, 2);
            rotateSpeed = _rnd.GetNorm(rotateSpeed0, rotSpeedSKO);
            nSwitch++;
        }
        void _51Rotate_WTD(double t2) {
            Rotate(t2);
        }
        UnitState _52Stay= new UnitState(null, nameof(_52Stay));
        void _52Stay_OnEntry() {
            rotTime0 = UnitTime;
            rotTime = _rnd.GetDouble(rotTimeMin, rotTimeMax);
        }
        UnitState _6SuspiciousBehavor= new UnitState(null, nameof(_6SuspiciousBehavor));
        UnitState _7MoveToNoisePoint = new UnitState(null, nameof(_7MoveToNoisePoint));
        void _7MoveToNoisePoint_OnEntry() {
            WayPoints = new WayPoints(_r.FindPath(Pos, _noisePos));
        }
        void _7MoveToNoisePoint_WTD(double t2) {
            Move(t2);
            SignToVel(t2);
            //if (_3HearNoise_ConditionFunc()) {
            //    _7MoveToNoisePoint_OnEntry();
            //}
        }
        UnitState _8SeeSomething     = new UnitState(null, nameof(_8SeeSomething));
        UnitState _9MakeDecision     = new UnitState(null, nameof(_9MakeDecision));
        UnitState _10MakeDecision    = new UnitState(null, nameof(_10MakeDecision));
        UnitState _11MakeDecision    = new UnitState(null, nameof(_11MakeDecision));
        UnitState _12MakeDecision    = new UnitState(null, nameof(_12MakeDecision));
        UnitState _13SearchAround    = new UnitState(null, nameof(_13SearchAround));
        UnitState _14MoveToLastSpot  = new UnitState(null, nameof(_14MoveToLastSpot));
        UnitState _15MoveToCurios    = new UnitState(null, nameof(_15MoveToCurios));
        UnitState _16Attacking       = new UnitState(null, nameof(_16Attacking));
        UnitState _17Battle          = new UnitState(null, nameof(_17Battle));
        UnitState _0RunToAlarm       = new UnitState(null, nameof(_0RunToAlarm));
        #endregion

        #region Triggers
        
        UnitTrigger _1ReachDist = new UnitTrigger(nameof(_1ReachDist));
        bool _1ReachDist_ConditionFunc() {
            return (Pos - WayPoints.Last).GetLength() < 0.01;
        }

        UnitTrigger _2TimeUp = new UnitTrigger(nameof(_2TimeUp));
        bool _2TimeUp_ConditionFunc() {
            return nSwitch >= nSwitchmax;
        }

        UnitTrigger _3HearNoise = new UnitTrigger(nameof(_3HearNoise));
        double _noiseDelta = 1d;
        bool _3HearNoise_ConditionFunc() {
            var bgn = 0d;
            if (_r.staticNoisesList.Count != 0)
                bgn = _r.GetStaticNoiseAt(Pos);
            foreach (var un in UnitNoises) {
                if(un.GetDBTo(Pos, _r) - bgn > _noiseDelta) {
                    _noisePos = un.GetPos();
                    targKnowlege.Hear();
                    return true;
                }
            }
            return false;
        }
        
        UnitTrigger _4HearNoise = new UnitTrigger(nameof(_4HearNoise));
        UnitTrigger _5Come = new UnitTrigger(nameof(_5Come));
        UnitTrigger _6Come = new UnitTrigger(nameof(_6Come));
        UnitTrigger _7TimeUp = new UnitTrigger(nameof(_7TimeUp));
        UnitTrigger _8Curios = new UnitTrigger(nameof(_8Curios));
        UnitTrigger _9SeeYou = new UnitTrigger(nameof(_9SeeYou));
        UnitTrigger _10LostTarget = new UnitTrigger(nameof(_10LostTarget));
        UnitTrigger _11RecognizeTimeUp = new UnitTrigger(nameof(_11RecognizeTimeUp));
        UnitTrigger _12Recognized = new UnitTrigger(nameof(_12Recognized));
        UnitTrigger _13trig = new UnitTrigger(nameof(_13trig));
        UnitTrigger _14trig = new UnitTrigger(nameof(_14trig));
        UnitTrigger _15trig = new UnitTrigger(nameof(_15trig));
        UnitTrigger _16trig = new UnitTrigger(nameof(_16trig));
        UnitTrigger _17trig = new UnitTrigger(nameof(_17trig));
        UnitTrigger _18trig = new UnitTrigger(nameof(_18trig));
        UnitTrigger _19Attacked = new UnitTrigger(nameof(_19Attacked));
        UnitTrigger _20Immobilize = new UnitTrigger(nameof(_20Immobilize));
        UnitTrigger _21trig = new UnitTrigger(nameof(_21trig));
        UnitTrigger _0trig = new UnitTrigger(nameof(_0trig));

        UnitTrigger _22trig = new UnitTrigger(nameof(_22trig));
        bool _22trig_ConditionFunc() {
            return UnitTime > rotTime0 + rotTime;
        }
        UnitTrigger _23trig = new UnitTrigger(nameof(_23trig));
        bool _23trig_ConditionFunc() {
            return UnitTime > rotTime0 + rotTime;
        }
        #endregion

        #region StateWTD

        #endregion

        #region TrigFunct

        #endregion

        private Room _r;
        private Vector2D _noisePos;

        public List<INoisePoint> BackgroundNoise { get; set; } = new List<INoisePoint>();
        public List<INoisePoint> UnitNoises { get; set; } = new List<INoisePoint>();

        public TargetKnowlege targKnowlege = new TargetKnowlege();

        public TerroristUnit(string Name, Room r, GameLoop Owner = null) : base(Name, Owner) {
            _r = r;
            Pos = _r.GetWalkableCoord();
            InitMe();
            ConfigureMe();
        }

    }

    public class TargetKnowlege {
        public enum TargInfo { nothing, susp , enemyAlmost, enemy };
        public enum InfoTrigg { unknown = 0, see = 0b0001, hear = 0b0010, seeMoving = 0b0011, attacked = 0b0111 };
        InfoTrigg _infoInt = InfoTrigg.unknown;
        public TargInfo info {
            get {
                switch ((int)_infoInt) {
                    case (0):
                        return TargInfo.nothing;
                    case (0b0001):
                        return TargInfo.susp;
                    case (0b0010):
                        return TargInfo.susp;
                    case (0b0011):
                        return TargInfo.enemyAlmost;

                    default:
                        return TargInfo.enemy;
                }

            }
        }
        public void PerformTrigg(InfoTrigg trigg) {
            _infoInt |= trigg;
        }
        public void See() {
            PerformTrigg(InfoTrigg.see);
        }
        public void Hear() {
            PerformTrigg(InfoTrigg.hear);
        }
        public void SeeMoving() {
            PerformTrigg(InfoTrigg.seeMoving);
        }
        public void Attacked() {
            PerformTrigg(InfoTrigg.attacked);
        }
    }
}
