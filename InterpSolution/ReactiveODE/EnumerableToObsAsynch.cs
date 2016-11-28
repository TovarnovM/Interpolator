using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveODE {
    public interface IEquasionController {
        void Start();
        void Cancel(bool waitEnd = true);
        void Pause();
        void Resume();
        bool Paused { get; set; }
    }

    public class EnumerableToObsAnsynch<T> : IObservable<T>, IEquasionController {
        protected IEnumerator<T> enumenator;
        protected IEnumerable<T> source;
        protected Subject<T> Sbj;
        protected CancellationTokenSource cts;

        protected readonly object _locker = new object();
        protected bool _go;
        protected bool getStarted;
        private Task dostuffTsk;

        public EnumerableToObsAnsynch(IEnumerable<T> source, bool paused = true) {
            getStarted = false;
            this.source = source;
            _go = !paused;
            cts = new CancellationTokenSource();
            Sbj = new Subject<T>();
        }

        protected virtual void DoStuff() {
            while(!cts.IsCancellationRequested 
                && enumenator.MoveNext()) {
                lock(_locker)
                    while(!_go)
                        Monitor.Wait(_locker);
                Sbj.OnNext(enumenator.Current);
            }
            Sbj.OnCompleted();

        }

        public void Cancel(bool waitEnd = true) {
            cts.Cancel();
            if(waitEnd) {
                Resume();
                dostuffTsk.Wait();
            }
                

        }

        public void Resume() {
            lock(_locker) {
                _go = true;
                Monitor.Pulse(_locker);
            }
        }
        public void Pause() {
            lock(_locker) {
                _go = false;
                Monitor.Pulse(_locker);
            }
        }

        public bool Paused {
            get {
                return !_go;
            }
            set {
                if(value)
                    Pause();
                else
                    Resume();
            }
        }

        public void Start() {
            lock(_locker) {
                if(!getStarted) {
                    getStarted = true;
                    enumenator = source.GetEnumerator();
                    dostuffTsk = Task.Factory.StartNew(DoStuff,cts.Token);
                }
            }
        }

        public bool SubscribeOnCurrThread { get; set; } = true;

        public virtual IDisposable Subscribe(IObserver<T> observer) {
            IDisposable res;
            lock(_locker) {
                if(SubscribeOnCurrThread)
                    res = Sbj.SubscribeOn(Scheduler.CurrentThread).Subscribe(observer);
                else
                    res = Sbj.SubscribeOn(Scheduler.Default).Subscribe(observer);
                Monitor.Pulse(_locker);
            }
            if(!getStarted)
                Start();
            return res;
        }
    }
}
