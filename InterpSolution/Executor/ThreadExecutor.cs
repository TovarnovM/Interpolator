using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Executor {
    public class ThreadExecutor<TParams, TResult>: ExecutorAbstract<TParams, TResult> {
        protected List<(Task, CancellationTokenSource)> workers;
        protected int workerCountMax;
        public bool saveToDoneQueue = false;

        public IComputeTask<TParams, TResult> ComputeTask { get; }
        public int WorkerCountMax {
            get { return workerCountMax; }
            set { workerCountMax = value; }
        }
        public int WorkerCount {
            get {
                int c = 0;
                lock (locker) {
                    c = workers.Count;
                }
                return c;
            }
        }

        void SpawnWorker() {
            lock (locker) {
                var cts = new CancellationTokenSource();
                var ct = cts.Token;
                int ind = workers.Count;
                var state = new WorkerInfo() {
                    ct = ct,
                    index = ind
                };
                
                var task = Task.Factory.StartNew(WorkerAction, state, ct, TaskCreationOptions.LongRunning,TaskScheduler.Default);
                var tp = (task, cts);
                workers.Add(tp);
                task.ContinueWith(t => {
                    if (workers.Contains(tp))
                        workers.Remove(tp);
                    
                });
                //task.Start();
            }
        }

        void DeleteWorker(int index = -1) {
            lock (locker) {
                index = index < 0
                    ? workers.Count - 1
                    : index;
                var (task, cts) = workers[index];
                cts.Cancel();
                workers.RemoveAt(index);
            }
        }

        public void KillAllWorkers() {
            lock (locker) {
                foreach (var (task,cts) in workers) {
                    cts.Cancel();                   
                }
                foreach (var (task, cts) in workers) {
                    task.Dispose();
                }
                workers.Clear();
            }
        }

        void SynchWorkersCount() {
            int diff = WorkerCountMax - WorkerCount;
            if (diff == 0) {
                return;
            } else if(diff > 0) {
                SpawnWorker();
            } else {
                DeleteWorker();
            }
            SynchWorkersCount();

        }

        class WorkerInfo {
            public CancellationToken ct;
            public int index;
        }

        void WorkerAction(object state) {
            var ct = ((WorkerInfo)state).ct;
            var ind = ((WorkerInfo)state).index;
            foreach (var tsk in inputCollection.GetConsumingEnumerable(ct)) {
                lock (locker) {
                    if (blackList.Contains(tsk.Params)) {
                        blackList.Remove(tsk.Params);
                        continue;
                    }

                    inprogressBag.TryAdd(ind, tsk);
                    OnExecutAddNew(tsk);
                }
                tsk.StartExecuting = DateTime.Now;
                tsk.Status = ResStatus.calculating;
                var res = ComputeTask.MapAction(tsk.Params);
                tsk.Result = res;
                tsk.StopExecuting = DateTime.Now;
                tsk.Status = ResStatus.finished;
                lock (locker) {                   
                    if(inprogressBag.TryRemove(ind,out Res<TParams, TResult> fd)) {
                        if(Status == ExecStatus.executing) {
                            if(saveToDoneQueue)
                                doneQueue.Enqueue(fd);
                            OnExecutDoneNew(fd);
                        }                      
                    }
                }
            }

        }

        public ThreadExecutor(IComputeTask<TParams, TResult> computeTask) {
            ComputeTask = computeTask;
            workers = new List<(Task, CancellationTokenSource)>();
            blackList = new HashSet<TParams>();
            Status = ExecStatus.ready;

        }

        #region Stop/Start


        public override void Run() {
            Status = ExecStatus.executing;
            SynchWorkersCount();
        }

        public override void Stop(bool waitExecut = true) {
            try {

                Task[] tsks;
                lock (locker) {
                    tsks = new Task[workers.Count];
                    int ind = 0;
                    foreach (var (tsk, ct) in workers) {
                        ct.Cancel();
                        tsks[ind++] = tsk;
                    }
                }
                //Task.WaitAll(tsks);
                //workers.Clear();
                if (waitExecut) {
                    WaitAll(InProgress);
                }
                lock (locker) {
                    workers.Clear();
                }
                Status = ExecStatus.ready;

            } catch (Exception) {
                Status = ExecStatus.error;
                // throw;
            }

        }
        #endregion

        #region WaitAny/All

        public override bool WaitAll(TimeSpan time4execMax, params Res<TParams, TResult>[] tasks) {
            var loosers = new HashSet<Res<TParams, TResult>>(tasks);
            var beg = DateTime.Now;
            while (loosers.Count > 0 || (DateTime.Now - beg>time4execMax)) {
                loosers.RemoveWhere(l => l.Status == ResStatus.finished || l.Status == ResStatus.caclError);
                Thread.Sleep(100);
            }
            return true;
        }

        public override Res<TParams, TResult> WaitAny(TimeSpan time4execMax, params Res<TParams, TResult>[] tasks) {
            var loosers = new HashSet<Res<TParams, TResult>>(tasks);
            int count = tasks.Length;
            var beg = DateTime.Now;
            while (loosers.Count == count || (DateTime.Now - beg > time4execMax)) {
                if (loosers.RemoveWhere(l => l.Status == ResStatus.finished || l.Status == ResStatus.caclError) > 0) {
                    return tasks.FirstOrDefault(l => l.Status == ResStatus.finished || l.Status == ResStatus.caclError);

                }
                Thread.Sleep(100);
            }
            return null;
        }

        #endregion
    }

}
