using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveODE {
    public static class PausableObs {
        public static IObservable<T> Pausable<T>(
           this IObservable<T> source,
           IObservable<bool> pauser) {
            return Observable.Create<T>(o => {
                var paused = new SerialDisposable();
                var subscription = Observable.Publish(source,ps => {
                    var values = new ReplaySubject<T>();
                    Func<bool,IObservable<T>> switcher = b => {
                        if(b) {
                            values.Dispose();
                            values = new ReplaySubject<T>();
                            paused.Disposable = ps.Subscribe(values);
                            return Observable.Empty<T>();
                        } else {
                            return values.Concat(ps);
                        }
                    };

                    return pauser.StartWith(false).DistinctUntilChanged()
                        .Select(p => switcher(p))
                        .Switch();
                }).Subscribe(o);
                return new CompositeDisposable(subscription,paused);
            });
        }
    }

    class Program {
        static void WriteSequenceToConsole(IObservable<string> sequence) {
            //The next two lines are equivalent.
            //sequence.Subscribe(value=>Console.WriteLine(value));
            sequence.Subscribe(Console.WriteLine);
        }

        static IEnumerable<int> GetSeq() {
            int j = -1;
            while(true) {
                yield return ++j;
                double d = 0;
                for(int i = 1; i < 1000000; i++) {
                    d = 3232596 * 656 / 31321 * i - i + d / i + j + 3*d;
                }
            }

        }

        static void Main(string[] args) {
            var arr = GetSeq();
            var tst = new EnumerableToObsAnsynch<int>(arr);
            var d1 = tst.Subscribe(i => Console.WriteLine($"n =1 : i = {i }"),() => Console.WriteLine("Done 1"));
            var d2 = tst.Subscribe(i => Console.WriteLine($"n =2 : i = {i }"),() => Console.WriteLine("Done 2"));
            var d3 = tst.Subscribe(i => Console.WriteLine($"n =3 : i = {i }"),() => Console.WriteLine("Done 3"));
            IDisposable d4 = null;
            Task.Factory.StartNew(() => {
                Thread.Sleep(4000);
                d4 = tst.Subscribe(i => Console.WriteLine($"n =4 : i = {i }"),() => Console.WriteLine("Done 4"));
            });

            Console.ReadLine();
            d1.Dispose();
            Console.ReadLine();
            d2.Dispose();
            Console.ReadLine();
            d3.Dispose();
            Console.ReadLine();
            d4?.Dispose();
            //while(true) {
            //    Console.ReadKey();
            //    tst.Paused = !tst.Paused;
            //}
            Console.ReadLine();
            var d5 = tst.Subscribe(i => Console.WriteLine($"n =5 : i = {i }"),() => Console.WriteLine("Done 3"));
            Console.WriteLine("===============");
            Console.ReadKey();
        }
    }
}
