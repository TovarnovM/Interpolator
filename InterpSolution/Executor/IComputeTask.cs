namespace Executor {
    public interface IComputeTask<TParams, TResult> {
        TResult MapAction(TParams taskData);
    }

}
