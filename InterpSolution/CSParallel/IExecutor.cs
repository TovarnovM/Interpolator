using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSParallel {
    public interface IExecutor<TTask,TResult> {
        IEnumerable<TResult> Run(IEnumerable<TTask> tasksInfos);
        void StopAll();

    }
}
