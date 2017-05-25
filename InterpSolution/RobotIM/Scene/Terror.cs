using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotIM.Core;
using Stateless;

namespace RobotIM.Scene {
    class UnitWithStates : UnitWithVision {
        StateMachine<UnitState, string> _stateM;
        UnitState _state;
        public bool SwitchState(string newStateName) {
            if (newStateName == "")
                return false;
            var can = _stateM.CanFire(newStateName);
            if (!can) {
                return false;
            }
            _stateM.Fire(newStateName);
            return true;
        }
        public UnitWithStates(string Name, GameLoop Owner = null) : base(Name, Owner) {
            _stateM = new StateMachine<UnitState, string>(() => _state, s => _state = s);
        }
        protected override void PerformUpdate(double toTime) {
            _state.WhatToDo(UnitTime, toTime);
            foreach (var tr in _state.triggerList) {
                var newStateName = tr();
                if(newStateName != "" && SwitchState(newStateName)) {
                    break;
                }
            }
        }
    }

    class UnitState {
        public string Name { get; set; }
        public Action<double> WhatToDo;
        public List<Func<string>> triggerList = new List<Func<string>>(); 
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

                default:
                    throw new ArgumentException("Нэт такого состояния");
            }
            return us;         
        }
    }

}
