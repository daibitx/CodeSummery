namespace STD
{
    // 真正的连接池实现
    public class DbContextPool : IDisposable
    {
        private readonly ConcurrentQueue<YBDZContext> _pool = new();
        private readonly SemaphoreSlim _semaphore;
        private readonly DbContextOptionsBuilder<YBDZContext> _optionsBuilder;
        private readonly int _maxPoolSize;
        private int _currentCount = 0;

        public DbContextPool(DbConfig config, int maxPoolSize = 20)
        {
            _maxPoolSize = maxPoolSize;
            _semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);

            _optionsBuilder = new DbContextOptionsBuilder<YBDZContext>();
            _optionsBuilder.UseDm($"Server={config.Ip}; User Id={config.UserId}; PWD={config.Password};DATABASE={config.Database};Pooling=true;Max Pool Size={maxPoolSize};Min Pool Size=5;Connection Timeout=30;")
                          .EnableServiceProviderCaching()
                          .EnableSensitiveDataLogging(false);
        }

        public async Task<YBDZContext> GetContextAsync()
        {
            await _semaphore.WaitAsync();

            if (_pool.TryDequeue(out var context))
            {
                // 检查连接是否还有效
                if (IsContextValid(context))
                {
                    return context;
                }
                else
                {
                    context.Dispose();
                    Interlocked.Decrement(ref _currentCount);
                }
            }

            // 创建新的 context
            context = new YBDZContext(_optionsBuilder.Options);
            Interlocked.Increment(ref _currentCount);
            return context;
        }

        public YBDZContext GetContext()
        {
            _semaphore.Wait();

            if (_pool.TryDequeue(out var context))
            {
                if (IsContextValid(context))
                {
                    return context;
                }
                else
                {
                    context.Dispose();
                    Interlocked.Decrement(ref _currentCount);
                }
            }

            context = new YBDZContext(_optionsBuilder.Options);
            Interlocked.Increment(ref _currentCount);
            return context;
        }

        public void ReturnContext(YBDZContext context)
        {
            if (context != null && IsContextValid(context) && _currentCount <= _maxPoolSize)
            {
                // 清理 context 状态
                context.ChangeTracker.Clear();
                _pool.Enqueue(context);
            }
            else
            {
                context?.Dispose();
                if (_currentCount > 0)
                    Interlocked.Decrement(ref _currentCount);
            }

            _semaphore.Release();
        }

        private bool IsContextValid(YBDZContext context)
        {
            try
            {
                return context?.Database?.CanConnect() == true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            while (_pool.TryDequeue(out var context))
            {
                context.Dispose();
            }
            _semaphore?.Dispose();
        }
    }

    public static class DbContextManager
    {
        private static DbContextPool _pool;
        private static readonly object _lock = new object();

        public static void Initialize(DbConfig config, int maxPoolSize = 20)
        {
            lock (_lock)
            {
                if (_pool == null)
                {
                    _pool = new DbContextPool(config, maxPoolSize);
                }
            }
        }

        public static async Task<T> ExecuteAsync<T>(Func<YBDZContext, Task<T>> operation)
        {
            if (_pool == null)
                throw new InvalidOperationException("DbContextManager not initialized. Call Initialize() first.");

            var context = await _pool.GetContextAsync();
            try
            {
                return await operation(context);
            }
            finally
            {
                _pool.ReturnContext(context);
            }
        }

        public static T Execute<T>(Func<YBDZContext, T> operation)
        {
            if (_pool == null)
                throw new InvalidOperationException("DbContextManager not initialized. Call Initialize() first.");

            var context = _pool.GetContext();
            try
            {
                return operation(context);
            }
            finally
            {
                _pool.ReturnContext(context);
            }
        }

        public static async Task ExecuteAsync(Func<YBDZContext, Task> operation)
        {
            if (_pool == null)
                throw new InvalidOperationException("DbContextManager not initialized. Call Initialize() first.");

            var context = await _pool.GetContextAsync();
            try
            {
                await operation(context);
            }
            finally
            {
                _pool.ReturnContext(context);
            }
        }

        public static void Execute(Action<YBDZContext> operation)
        {
            if (_pool == null)
                throw new InvalidOperationException("DbContextManager not initialized. Call Initialize() first.");

            var context = _pool.GetContext();
            try
            {
                operation(context);
            }
            finally
            {
                _pool.ReturnContext(context);
            }
        }

        public static void Dispose()
        {
            lock (_lock)
            {
                _pool?.Dispose();
                _pool = null;
            }
        }
    }

    public class DbConfig
    {
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
    }
}