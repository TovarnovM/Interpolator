using System;

namespace Executor {
    public enum ResStatus { notCalcYet, calculating, finished, caclError };

    public class Res<TParams, TResult>  {
        public TResult Result { get; set; }
        public TParams Params { get; set; }
        public DateTime StartExecuting { get; set; }
        public DateTime StopExecuting { get; set; }
        public ResStatus Status { get; set; }
        public string Info { get; set; }
        public int Id { get; set; }
      //  public ExecutorAbstract<TParams, TResult> Executor { get; set; }
    }

}
