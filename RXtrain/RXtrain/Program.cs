using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace RXtrain {
    internal class Generate_Simple {
        private static void Main() {
            IObservable<int> source = Observable.Range(1, 10);
            IDisposable subscription = source.Subscribe(
            x => Console.WriteLine("OnNext: {0}", x),
            ex => Console.WriteLine("OnError: {0}", ex.Message),
            () => Console.WriteLine("OnCompleted"));
            Console.WriteLine("Press ENTER to unsubscribe...");
            Console.ReadLine();
            subscription.Dispose();
        }
    }
}

