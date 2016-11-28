
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Research.Oslo;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading;

namespace ReactiveODE {
    public class ODE_Rx : EnumerableToObsAnsynch<SolPoint> {
        double delta;
       

        public ODE_Rx(IEnumerable<SolPoint> source, double delta = -1d) : base(source) {
            this.delta = delta;
        }

        protected override void DoStuff() {
            if(delta <= 0) {
                base.DoStuff();
                return;
            }


            var en = enumenator;
            if(!cts.IsCancellationRequested
                  && !en.MoveNext()) {
                Sbj.OnCompleted();
                return;
            }
            SolPoint prev = en.Current;
            double n = Math.Ceiling(prev.T / delta);
            double tout = n * delta;
            if(tout == prev.T) {
                lock(_locker)
                    while(!_go)
                        Monitor.Wait(_locker);
                Sbj.OnNext(prev);
            }
                
            
            n++;
            tout = n * delta;

            while(!cts.IsCancellationRequested
                  && en.MoveNext()) 
            {
                
                SolPoint current = en.Current;
                while(current.T >= tout) {
                    lock(_locker)
                        while(!_go)
                            Monitor.Wait(_locker);

                    Sbj.OnNext(new SolPoint(tout,Vector.Lerp(tout,prev.T,prev.X,current.T,current.X)));
 
                    n++;
                    tout = n * delta;
                }
                prev = current;
            }
            Sbj.OnCompleted();
        }
    
    }

    public static class OdeHelpersRx {

        /// <summary>Extracts points with time less than or equal to <paramref name="to"/></summary>
        /// <param name="solution">Solution</param>
        /// <param name="to">Finish time</param>
        /// <returns>New sequence of solution points</returns>
        public static IObservable<SolPoint> ToRx(this IEnumerable<SolPoint> solution,out IEquasionController controller) {
            var obs = new ODE_Rx(solution);
            controller = obs;
            return obs;
        }

        /// <summary>Extracts points with time less than or equal to <paramref name="to"/></summary>
        /// <param name="solution">Solution</param>
        /// <param name="to">Finish time</param>
        /// <returns>New sequence of solution points</returns>
        public static IObservable<SolPoint> ToRx(this IEnumerable<SolPoint> solution) {
            return new ODE_Rx(solution);

        }

        /// <summary>Interpolates solution at points with specified time step</summary>
        /// <param name="solution">Solution</param>
        /// <param name="delta">Time step</param>
        /// <returns>New sequence of solution points at moments i * delta</returns>
        /// <remarks>Linear intepolation is used to find phase vector between two solution points</remarks>
        public static IObservable<SolPoint> WithStepRx(this IEnumerable<SolPoint> solution,double delta,out IEquasionController controller) {
            var obs = new ODE_Rx(solution,delta);
            controller = obs;
            return obs;
        }

        /// <summary>Interpolates solution at points with specified time step</summary>
        /// <param name="solution">Solution</param>
        /// <param name="delta">Time step</param>
        /// <returns>New sequence of solution points at moments i * delta</returns>
        /// <remarks>Linear intepolation is used to find phase vector between two solution points</remarks>
        public static IObservable<SolPoint> WithStepRx(this IEnumerable<SolPoint> solution,double delta) {
            return new ODE_Rx(solution,delta);
            
        }
    }
}
