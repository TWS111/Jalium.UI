using System.ComponentModel;
using System.Collections.Concurrent;

namespace Jalium.UI;

/// <summary>
/// Provides services for managing the queue of work items for a thread.
/// </summary>
public sealed class Dispatcher
{
    private static readonly ThreadLocal<Dispatcher?> _currentDispatcher = new();
    private static Dispatcher? _mainDispatcher;
    private static readonly object _lock = new();

    private readonly Thread _thread;
    private readonly ConcurrentQueue<Action> _queue = new();
    private readonly ManualResetEventSlim _workAvailable = new(false);
    private volatile bool _isShutdown;

    /// <summary>
    /// Gets the <see cref="Dispatcher"/> for the thread currently executing.
    /// </summary>
    public static Dispatcher? CurrentDispatcher => _currentDispatcher.Value;

    /// <summary>
    /// Gets the <see cref="Dispatcher"/> for the main UI thread.
    /// </summary>
    public static Dispatcher? MainDispatcher => _mainDispatcher;

    /// <summary>
    /// Gets the thread this <see cref="Dispatcher"/> is associated with.
    /// </summary>
    public Thread Thread => _thread;

    /// <summary>
    /// Gets a value indicating whether the dispatcher has been shut down.
    /// </summary>
    public bool HasShutdownStarted => _isShutdown;

    /// <summary>
    /// Gets a value indicating whether the dispatcher has finished shutting down.
    /// </summary>
    public bool HasShutdownFinished => _isShutdown && _queue.IsEmpty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Dispatcher"/> class for the current thread.
    /// </summary>
    private Dispatcher()
    {
        _thread = Thread.CurrentThread;
    }

    /// <summary>
    /// Gets or creates a <see cref="Dispatcher"/> for the current thread.
    /// </summary>
    /// <returns>The dispatcher for the current thread.</returns>
    public static Dispatcher GetForCurrentThread()
    {
        if (_currentDispatcher.Value == null)
        {
            var dispatcher = new Dispatcher();
            _currentDispatcher.Value = dispatcher;

            // First dispatcher becomes the main dispatcher
            lock (_lock)
            {
                _mainDispatcher ??= dispatcher;
            }
        }

        return _currentDispatcher.Value;
    }

    /// <summary>
    /// Sets the current thread's dispatcher as the main UI thread dispatcher.
    /// </summary>
    public static void SetAsMainThread()
    {
        var dispatcher = GetForCurrentThread();
        lock (_lock)
        {
            _mainDispatcher = dispatcher;
        }
    }

    /// <summary>
    /// Determines whether the calling thread is the thread associated with this <see cref="Dispatcher"/>.
    /// </summary>
    /// <returns>true if the calling thread is the thread associated with this dispatcher; otherwise, false.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool CheckAccess()
    {
        return Thread.CurrentThread == _thread;
    }

    /// <summary>
    /// Determines whether the calling thread has access to this <see cref="Dispatcher"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">The calling thread does not have access to this <see cref="Dispatcher"/>.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void VerifyAccess()
    {
        if (!CheckAccess())
        {
            throw new InvalidOperationException(
                "The calling thread cannot access this object because a different thread owns it.");
        }
    }

    /// <summary>
    /// Executes the specified <see cref="Action"/> synchronously on the thread the <see cref="Dispatcher"/> is associated with.
    /// </summary>
    /// <param name="callback">The delegate to invoke.</param>
    public void Invoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback();
            return;
        }

        using var completed = new ManualResetEventSlim(false);
        Exception? exception = null;

        _queue.Enqueue(() =>
        {
            try
            {
                callback();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        _workAvailable.Set();
        completed.Wait();

        if (exception != null)
        {
            throw new InvalidOperationException("An error occurred while executing on the dispatcher thread.", exception);
        }
    }

    /// <summary>
    /// Executes the specified <see cref="Func{TResult}"/> synchronously on the thread the <see cref="Dispatcher"/> is associated with.
    /// </summary>
    /// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
    /// <param name="callback">The delegate to invoke.</param>
    /// <returns>The value returned by <paramref name="callback"/>.</returns>
    public TResult Invoke<TResult>(Func<TResult> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            return callback();
        }

        TResult result = default!;
        using var completed = new ManualResetEventSlim(false);
        Exception? exception = null;

        _queue.Enqueue(() =>
        {
            try
            {
                result = callback();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        _workAvailable.Set();
        completed.Wait();

        if (exception != null)
        {
            throw new InvalidOperationException("An error occurred while executing on the dispatcher thread.", exception);
        }

        return result;
    }

    /// <summary>
    /// Executes the specified <see cref="Action"/> asynchronously on the thread the <see cref="Dispatcher"/> is associated with.
    /// </summary>
    /// <param name="callback">The delegate to invoke.</param>
    public void BeginInvoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        _queue.Enqueue(callback);
        _workAvailable.Set();
    }

    /// <summary>
    /// Executes the specified <see cref="Action"/> asynchronously on the thread the <see cref="Dispatcher"/> is associated with.
    /// </summary>
    /// <param name="callback">The delegate to invoke.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task InvokeAsync(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback();
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();

        _queue.Enqueue(() =>
        {
            try
            {
                callback();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        _workAvailable.Set();
        return tcs.Task;
    }

    /// <summary>
    /// Executes the specified <see cref="Func{TResult}"/> asynchronously on the thread the <see cref="Dispatcher"/> is associated with.
    /// </summary>
    /// <typeparam name="TResult">The return value type of the specified delegate.</typeparam>
    /// <param name="callback">The delegate to invoke.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            return Task.FromResult(callback());
        }

        var tcs = new TaskCompletionSource<TResult>();

        _queue.Enqueue(() =>
        {
            try
            {
                tcs.SetResult(callback());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        _workAvailable.Set();
        return tcs.Task;
    }

    /// <summary>
    /// Processes all pending work items in the dispatcher queue.
    /// This should be called from the dispatcher's thread during the message loop.
    /// </summary>
    public void ProcessQueue()
    {
        VerifyAccess();

        while (_queue.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                System.Diagnostics.Debug.WriteLine($"Dispatcher exception: {ex}");
            }
        }

        _workAvailable.Reset();
    }

    /// <summary>
    /// Initiates shutdown of the dispatcher.
    /// </summary>
    public void BeginInvokeShutdown()
    {
        _isShutdown = true;
        _workAvailable.Set();
    }

    /// <summary>
    /// Occurs when the dispatcher begins to shut down.
    /// </summary>
    public event EventHandler? ShutdownStarted;

    /// <summary>
    /// Occurs when the dispatcher finishes shutting down.
    /// </summary>
    public event EventHandler? ShutdownFinished;
}

/// <summary>
/// Specifies the priority at which operations can be invoked via the <see cref="Dispatcher"/>.
/// </summary>
public enum DispatcherPriority
{
    /// <summary>
    /// The operation will not be processed.
    /// </summary>
    Inactive = 0,

    /// <summary>
    /// The operation is processed when the system is idle.
    /// </summary>
    SystemIdle = 1,

    /// <summary>
    /// The operation is processed when the application is idle.
    /// </summary>
    ApplicationIdle = 2,

    /// <summary>
    /// The operation is processed after background operations have completed.
    /// </summary>
    ContextIdle = 3,

    /// <summary>
    /// The operation is processed after all other non-idle operations have completed.
    /// </summary>
    Background = 4,

    /// <summary>
    /// The operation is processed at the same priority as input.
    /// </summary>
    Input = 5,

    /// <summary>
    /// The operation is processed when layout and render operations have completed.
    /// </summary>
    Loaded = 6,

    /// <summary>
    /// The operation is processed at the same priority as rendering.
    /// </summary>
    Render = 7,

    /// <summary>
    /// The operation is processed at the same priority as data binding.
    /// </summary>
    DataBind = 8,

    /// <summary>
    /// The operation is processed at normal priority.
    /// </summary>
    Normal = 9,

    /// <summary>
    /// The operation is processed before other asynchronous operations.
    /// </summary>
    Send = 10
}
