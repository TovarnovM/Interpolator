using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveODE {
    public class VMPropRx<TValue,TInfo> {
        private TValue _value;

        public TValue Value {
            get { return _value; }
        }

        public VMPropRx(Func<TValue> constructor, Func<TInfo,TValue> update) {
            _value = constructor();
        }
    }
}
