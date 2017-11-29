using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Executor {
    public enum ExecStatus { notReady = 2, ready = 4, executing = 8, error = 16 };

    public abstract class ExecutorAbstract<TParams, TResult> {
        protected ExecStatus status;
        public ExecStatus Status {
            get {
                return status;
            }
            protected set {
                status = value;
                
            }
        }

        protected BlockingCollection<Res<TParams, TResult>> inputCollection;
        protected ConcurrentQueue<Res<TParams, TResult>> doneQueue;
        protected ConcurrentDictionary<int,Res<TParams, TResult>> inprogressBag;
        protected HashSet<TParams> blackList;
        protected object locker;

        

        public ExecutorAbstract() {
            inprogressBag = new ConcurrentDictionary<int, Res<TParams, TResult>>();
            doneQueue = new ConcurrentQueue<Res<TParams, TResult>>();
            inputCollection = new BlockingCollection<Res<TParams, TResult>>();
            locker = new object();
            
            Status = ExecStatus.notReady;
        }

        public virtual List<Res<TParams, TResult>> AddToQueue(params TParams[] tasks) {
            var lst = new List<Res<TParams, TResult>>(tasks.Length + 1);
            foreach (var pr in tasks) {
                var res = new Res<TParams, TResult>() {
                    //Executor = this,
                    Params = pr,
                    Status = ResStatus.notCalcYet
                };
                lst.Add(res);
                inputCollection.Add(res);
                OnQueueAddNew(res);
            }
            return lst;
        }

        public List<Res<TParams, TResult>> AddToQueue(IEnumerable<TParams> tasks) {
            return AddToQueue(tasks.ToArray());
        }

        public List<Res<TParams, TResult>> Run(IEnumerable<TParams> tasks) {
            return Run(tasks.ToArray());
        }

        public List<Res<TParams, TResult>> Run(TParams[] tasks) {
            var lst = AddToQueue(tasks);
            Run();
            return lst;
        }

        public bool WaitAll(IEnumerable<Res<TParams, TResult>> tasks) {
            return WaitAll(tasks.ToArray());
        }
        public bool WaitAll(params Res<TParams, TResult>[] tasks) {
            var ts = TimeSpan.MaxValue;
            return WaitAll(ts, tasks);
        }
        public bool WaitAll(IEnumerable<Res<TParams, TResult>> tasks, TimeSpan time4execMax) {
            return WaitAll(time4execMax, tasks.ToArray());
        }
        public Res<TParams, TResult> WaitAny(params Res<TParams, TResult>[] tasks) {
            var ts = TimeSpan.MaxValue;
            return WaitAny(ts, tasks);
        }
        public Res<TParams, TResult> WaitAny(IEnumerable<Res<TParams, TResult>> tasks) {
            var ts = TimeSpan.MaxValue;
            return WaitAny(tasks.ToArray());
        }
        public Res<TParams, TResult> WaitAny(IEnumerable<Res<TParams, TResult>> tasks, TimeSpan time4execMax) {
            return WaitAny(tasks.ToArray(), time4execMax);
        }


        public abstract void Run();
        public abstract void Stop(bool waitExecut = true);
        public abstract bool WaitAll(TimeSpan time4execMax, params Res<TParams, TResult>[] tasks);
        public abstract Res<TParams, TResult> WaitAny(TimeSpan time4execMax, params Res<TParams, TResult>[] tasks);
        #region Events
        public event EventHandler<Res<TParams, TResult>> QueueAddNew;
        public virtual void OnQueueAddNew(Res<TParams, TResult> e) {
            QueueAddNew?.Invoke(this, e);
        }
        public event EventHandler<Res<TParams, TResult>> ExecutAddNew;
        public virtual void OnExecutAddNew(Res<TParams, TResult> e) {
            ExecutAddNew?.Invoke(this, e);
        }
        public event EventHandler<Res<TParams, TResult>> ExecutDoneNew;
        public virtual void OnExecutDoneNew(Res<TParams, TResult> e) {
            ExecutDoneNew?.Invoke(this, e);
        }
        public List<Res<TParams, TResult>> ClearQueue() {
            lock (locker) {
                var res = new List<Res<TParams, TResult>>();
                foreach (var item in inputCollection.GetConsumingEnumerable()) {
                    res.Add(item);
                    if (inputCollection.Count == 0)
                        break;
                }
                return res;
            }
        }
        public List<Res<TParams, TResult>> ClearDoneList() {
            lock (locker) {
                var res = new List<Res<TParams, TResult>>(doneQueue.Count);
                while (!doneQueue.IsEmpty) {
                    if (doneQueue.TryDequeue(out Res<TParams, TResult> elem)) {
                        res.Add(elem);
                    }
                }
                return res;
            }

        }
        public void RemoveFromQueue(params TParams[] who) {
            lock (locker) {
                foreach (var item in who) {
                    if (!blackList.Contains(item)) {
                        blackList.Add(item);
                    }
                }
            }

        }

        public void RemoveFromQueue(params Res<TParams, TResult>[] who) {
            var arr = who.Select(r => r.Params).ToArray();
            RemoveFromQueue(arr);
        }
        #endregion

        #region Срезы
        public List<Res<TParams, TResult>> DoneList {
            get {
                return doneQueue.ToList();
            }
        }

        public List<Res<TParams, TResult>> InProgress {
            get {
                var dct = inprogressBag.ToList();
                return dct.Select(kv => kv.Value).ToList();
            }
        }

        public List<Res<TParams, TResult>> InQueue {
            get {
                return inputCollection.ToList();
            }
        }


        #endregion
    }

}
