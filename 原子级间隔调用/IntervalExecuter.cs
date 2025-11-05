namespace CommonUtils
{
    public class IntervalExecuter
    {
        private const int LockNotAcquired = 0;
        private const int LockAcquired = 1;
        private int _lockStatus = LockNotAcquired;
        private DateTime LastExecutionTime { get; set; } = DateTime.MinValue;
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
        private readonly Action _action;
        public IntervalExecuter(Action action, TimeSpan interval = default)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            if (interval != default)
            {
                Interval = interval;
            }
        }
        public bool IsTimeUp() => DateTime.Now - LastExecutionTime > Interval;

        public void Execute()
        {
            if (!IsTimeUp()) return;

            if (Interlocked.CompareExchange(ref _lockStatus, LockAcquired, LockNotAcquired) == LockNotAcquired)
            {
                try
                {
                    _action?.Invoke();
                    LastExecutionTime = DateTime.Now;
                }
                catch
                {
                    LastExecutionTime = DateTime.Now;
                    throw;
                }
                finally
                {
                    // 释放锁
                    Interlocked.Exchange(ref _lockStatus, LockNotAcquired);
                }
            }
        }
    }
    public class IntervalExecuter<T>
    {
        private const int LockNotAcquired = 0;
        private const int LockAcquired = 1;
        private int _lockStatus = LockNotAcquired;
        private DateTime LastExecutionTime { get; set; } = DateTime.MinValue;
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
        private readonly Func<T> _func;

        public IntervalExecuter(Func<T> func, TimeSpan interval = default)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
            if (interval != default)
            {
                Interval = interval;
            }
        }

        public bool IsTimeUp() => DateTime.Now - LastExecutionTime > Interval;

        public ExecuterResult<T> Execute()
        {
            if (!IsTimeUp()) return new ExecuterResult<T> { IsTimeUp = false };

            if (Interlocked.CompareExchange(ref _lockStatus, LockAcquired, LockNotAcquired) == LockNotAcquired)
            {
                try
                {
                    var result = _func.Invoke();
                    LastExecutionTime = DateTime.Now;
                    return new ExecuterResult<T>
                    {
                        Result = result,
                        IsTimeUp = true
                    };
                }
                catch
                {
                    LastExecutionTime = DateTime.Now;
                    throw;
                }
                finally
                {
                    Interlocked.Exchange(ref _lockStatus, LockNotAcquired);
                }
            }

            // 如果没有成功获得锁，返回 IsTimeUp 为 false（也可以是 null，这里看你需要）
            return new ExecuterResult<T> { IsTimeUp = false };
        }
    }
    public class ExecuterResult<T>
    {
        public T? Result { get; set; }
        public bool IsTimeUp { get; set; }
    }
}
