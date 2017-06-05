using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotIM.Core;
using Stateless;

namespace RobotIM.Scene {
    [Serializable]
    public class UnitWithStates : UnitWithVision {
        StateMachine<UnitState, UnitTrigger> _stateM;
        public StateMachine<UnitState, UnitTrigger> SM {
            get { return _stateM;  }
        }

        private UnitState _state;

        public UnitState State {
            get { return _state; }
            set { _state = value; }
        }


        public bool SwitchState(UnitTrigger trigg) {
            var can = _stateM.CanFire(trigg);
            if (!can) {
                Owner.Logger.AddLine(this, $"failed trying to switch state to trigger [{trigg.Name}]");
                return false;
            }
            var prevStateName = _state.Name;
            _stateM.Fire(trigg);
            Owner.Logger.AddLine(this, $"switched state form [{prevStateName}] to [{_state.Name}] by trigger [{trigg.Name}]");
            return true;
        }
        public UnitWithStates(string Name, GameLoop Owner = null) : base(Name, Owner) {
            _stateM = new StateMachine<UnitState, UnitTrigger>(() => _state, s => _state = s);
        }
        protected override void PerformUpdate(double toTime) {
            _state.WhatToDo(toTime);
            foreach (var tr in _stateM.PermittedTriggers) {
                if (tr.Condition() && SwitchState(tr)) {
                    break;
                }
            }
            
        }
    }
    [Serializable]
    public class UnitState {
        public string Name { get; set; }
        public Action<double> WhatToDo;
        public UnitWithStates owner;
        public UnitState(UnitWithStates owner, string name) {
            this.owner = owner;
            Name = name;
        }

        public static UnitState Factory(UnitWithStates owner, string name) {
            var us = new UnitState(owner, name);
            us.Name = name;
            switch (name) {
                case "moving":
                    us.WhatToDo += owner.Move;
                    break;
                case "scaning":
                    us.WhatToDo += owner.Rotate;
                    break;
                default:
                    throw new ArgumentException("Нэт такого состояния");
            }
            return us;         
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }
    }
    [Serializable]
    public class UnitTrigger {
        public UnitTrigger(string Name = "") {
            this.Name = Name;
        }
        public string Name { get; set; }
        public bool Condition() {
            return ConditionFunc != null ? ConditionFunc() : false; 
        }
        public Func<bool> ConditionFunc;
    }
}
