using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSParallel
{
    public abstract class CSTask<T>
    {
        public CSTask(CSExecutor executor) {
            Executor = executor;
        }

        public CSExecutor Executor { get; }

        public abstract void SolvAction(T initCond);

    }
}
