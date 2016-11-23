using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveODE {
    public class VMPropRx<TValue,TUpdateInfo> {

        TValue _value;
        Func<TUpdateInfo,TValue,TValue> _updateF;
        protected readonly object _locker = new object();
        CancellationTokenSource _cts = new CancellationTokenSource();
        ConcurrentStack<TUpdateInfo> _stack = new ConcurrentStack<TUpdateInfo>();
        bool _isBusy = false;

        public void Update(TUpdateInfo info) {
            lock(_locker) {
                if(_isBusy) {
                    _stack.Push(info);
                    return;
                }                   
                else {
                    _isBusy = true;
                    _stack.Clear();
                    Task.Factory.StartNew(UpdateFuncWrapper,info,_cts.Token).ContinueWith(UpdateFuncContinue);
                }
            }
        }

        void UpdateFuncWrapper(object state) {
            _value = _updateF((TUpdateInfo)state, _value);
        }

        void UpdateFuncContinue(Task res) {
            TUpdateInfo newerInfo;
            lock(_locker) {
                _isBusy = false;
                if(!_stack.TryPop(out newerInfo))
                    return;
            }
            Update(newerInfo);
        }

        public TValue Value {
            get { return _value; }
        }

        public VMPropRx(Func<TValue> constructor, Func<TUpdateInfo,TValue,TValue> updateFunc) {
            _value = constructor();
            _updateF = updateFunc;
        }
    }
}
