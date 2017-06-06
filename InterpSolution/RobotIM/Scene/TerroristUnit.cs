using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotIM.Core;
using System.Reflection;
using System.Text.RegularExpressions;
using Stateless;

namespace RobotIM.Scene {
    public class TerroristUnit : UnitWithStates {
        void InitMe() {
            StateList = typeof(TerroristUnit)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(fld => fld.FieldType == typeof(UnitState))
                .OrderBy(fld => int.Parse(Regex.Match(fld.Name, @"\d+").Value))
                .Select(fld => (UnitState)fld.GetValue(this))
                .ToList();
            StateList.ForEach(us => us.owner = this);

            TriggerList = typeof(TerroristUnit)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(fld => fld.FieldType == typeof(UnitTrigger))
                .OrderBy(fld => int.Parse(Regex.Match(fld.Name, @"\d+").Value))
                .Select(fld => (UnitTrigger)fld.GetValue(this))
                .ToList();
        }

        #region states
        List<UnitState> StateList;
        UnitState _1Alive            = new UnitState(null, nameof(_1Alive));
        UnitState _2Immobilize       = new UnitState(null, nameof(_2Immobilize));
        UnitState _3NormalBehavor3   = new UnitState(null, nameof(_3NormalBehavor3));
        UnitState _4Patrool          = new UnitState(null, nameof(_4Patrool));
        UnitState _5LookAround       = new UnitState(null, nameof(_5LookAround));

        UnitState _6SuspiciousBehavor= new UnitState(null, nameof(_6SuspiciousBehavor));
        UnitState _7MoveToNoisePoint = new UnitState(null, nameof(_7MoveToNoisePoint));
        UnitState _8SeeSomething     = new UnitState(null, nameof(_8SeeSomething));
        UnitState _9MakeDecision     = new UnitState(null, nameof(_9MakeDecision));
        UnitState _10MakeDecision    = new UnitState(null, nameof(_10MakeDecision));
        UnitState _11MakeDecision    = new UnitState(null, nameof(_11MakeDecision));
        UnitState _12MakeDecision    = new UnitState(null, nameof(_12MakeDecision));
        UnitState _13SearchAround    = new UnitState(null, nameof(_13SearchAround));
        UnitState _14MoveToLastSpot  = new UnitState(null, nameof(_14MoveToLastSpot));
        UnitState _15MoveToCurios    = new UnitState(null, nameof(_15MoveToCurios));
        UnitState _16Attacking       = new UnitState(null, nameof(_16Attacking));
        UnitState _0RunToAlarm       = new UnitState(null, nameof(_0RunToAlarm));
        #endregion

        #region Triggers
        List<UnitTrigger> TriggerList = new List<UnitTrigger>(30);
        UnitTrigger _1ReachDist = new UnitTrigger(nameof(_1ReachDist));
        UnitTrigger _2TimeUp = new UnitTrigger(nameof(_2TimeUp));
        UnitTrigger _3HearNoise = new UnitTrigger(nameof(_3HearNoise));
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
        UnitTrigger _20Immobilized = new UnitTrigger(nameof(_20Immobilized));
        UnitTrigger _0trig = new UnitTrigger(nameof(_0trig));
        #endregion

        #region StateWTD

        #endregion

        #region TrigFunct

        #endregion
        public TerroristUnit(string Name, GameLoop Owner = null) : base(Name, Owner) {
            InitMe();
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
