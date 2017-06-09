using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotIM.Core;
using Stateless;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RobotIM.Scene {
    [Serializable]
    public class UnitWithStates : UnitWithVision {
        protected StateMachine<UnitState, UnitTrigger> _stateM;
        public List<UnitState> StateList;
        public List<UnitTrigger> TriggerList = new List<UnitTrigger>(30);
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
            _state.WhatToDo?.Invoke(toTime);
            foreach (var tr in _stateM.PermittedTriggers) {
                if (tr.Condition() && SwitchState(tr)) {
                    break;
                }
            }
            
        }

        /// <summary>
        /// Автоматически связываются состояния и методы 
        /// 
        /// имя Поля состояния - UnitState StateName
        /// имя метода WhatToDo - void StateName_WTD(double t2)
        /// имя метода OnEntry - void StateName_OnEntry()
        /// 
        /// имя поля триггера - UnitTrigger TriggName
        /// имя его функции - bool TriggName_ConditionFunc()
        /// </summary>
        public void InitMe() {
            try {
                var states = this.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(fld => fld.FieldType == typeof(UnitState))
                    .OrderBy(fld => {
                        var match = Regex.Match(fld.Name, @"\d+");
                        return match.Success ? int.Parse(match.Value) : 0;                      
                        })
                    .ToList();

                StateList = states
                    .Select(fld => (UnitState)fld.GetValue(this))
                    .ToList();
                StateList.ForEach(us => us.owner = this);

                var bindingList = states
                    .Select(fld => {
                        var onEntrymethodName = fld.Name + "_OnEntry";
                        var onEntryI = this.GetType()
                            .GetMethod(onEntrymethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var onEntryAct = onEntryI != null ?
                            (Action)Delegate.CreateDelegate(typeof(Action), this, onEntryI) :
                            null;

                        var WTDmethodName = fld.Name + "_WTD";
                        var WTDI = this.GetType()
                            .GetMethod(WTDmethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var WTDAct = WTDI != null ?
                            (Action<double>)Delegate.CreateDelegate(typeof(Action<double>), this, WTDI) :
                            null;
                        //    ?.CreateDelegate(typeof(Action<double>), this);
                        return (state: (UnitState)fld.GetValue(this), onEntry: onEntryAct, WTD: WTDAct);
                    })
                    .ToList();

                foreach (var bt in bindingList) {
                    if (bt.onEntry != null) {
                        SM.Configure(bt.state)
                            .OnEntry(bt.onEntry);
                    }
                    if (bt.WTD != null) {
                        bt.state.WhatToDo += bt.WTD;
                    }
                }



                //TriggerList 
                var triggs = this.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(fld => fld.FieldType == typeof(UnitTrigger))
                    .OrderBy(fld => {
                        var match = Regex.Match(fld.Name, @"\d+");
                        return match.Success ? int.Parse(match.Value) : 0;
                    })
                    .ToList();

                TriggerList = triggs
                    .Select(fld => (UnitTrigger)fld.GetValue(this))
                    .ToList();

                var triggBindingList = triggs
                    .Select(ti => {
                        var condName = ti.Name + "_ConditionFunc";
                        var condFInfo = this.GetType()
                            .GetMethod(condName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var condFun = condFInfo != null ?
                            (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), this, condFInfo) :
                            null;
                        return (trigg: (UnitTrigger)ti.GetValue(this), condF: condFun);
                    })
                    .ToList();

                foreach (var tt in triggBindingList) {
                    if(tt.condF != null) {
                        tt.trigg.ConditionFunc += tt.condF;
                    }
                }
            } catch (Exception e) {

                throw e;
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
