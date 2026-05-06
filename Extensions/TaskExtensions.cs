namespace ForQab.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<(T1, T2)> WhenBoth<T1, T2>(
            this (Task<T1> t1, Task<T2> t2) tasks)
        {
            await Task.WhenAll(tasks.t1, tasks.t2);
            return (tasks.t1.Result, tasks.t2.Result);
        }
    }
}
