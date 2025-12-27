using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.VisualStudio.Threading;

namespace GWTool
{
    internal class ThreadHelper
    {
        private static readonly object _lock = new object();

        private static ThreadHelper _current = null;
        public static ThreadHelper Current
        {
            get
            {
                lock (_lock)
                {
                    if (_current is null)
                        _current = new ThreadHelper();

                    return _current;
                }
            }
        }

        private readonly JoinableTaskContext _joinableTaskContext;
        private readonly JoinableTaskCollection _joinableTaskCollection;
        private readonly JoinableTaskFactory _joinableTaskFactory;

        public JoinableTaskContext JoinableTaskContext => _joinableTaskContext;
        public JoinableTaskCollection JoinableTaskCollection => _joinableTaskCollection;
        public JoinableTaskFactory JoinableTaskFactory => _joinableTaskFactory;

        public ThreadHelper()
            : this(Thread.CurrentThread, SynchronizationContext.Current)
        {
        }

        public ThreadHelper(Thread currentThread = null, SynchronizationContext context = null)
        {
            _joinableTaskContext = new JoinableTaskContext(currentThread,
                context ?? WindowsFormsSynchronizationContext.Current);
            _joinableTaskCollection = _joinableTaskContext.CreateCollection();
            _joinableTaskFactory = _joinableTaskContext.CreateFactory(_joinableTaskCollection);
        }

        // VSTHRD102: Implement internal logic asynchronously
#pragma warning disable VSTHRD102
        public void Run(Func<Task> func) =>
            _joinableTaskFactory.Run(func);

        public T Run<T>(Func<Task<T>> func) =>
            _joinableTaskFactory.Run<T>(func);
#pragma warning restore VSTHRD102

        public JoinableTask RunAsync(Func<Task> func) =>
            _joinableTaskFactory.RunAsync(func);

        public JoinableTask<T> RunAsync<T>(Func<Task<T>> func) =>
            _joinableTaskFactory.RunAsync<T>(func);

        // VSTHRD004: Calls to JoinableTaskFactory.SwitchToMainThreadAsync() must be awaited.
#pragma warning disable VSTHRD004
        public JoinableTaskFactory.MainThreadAwaitable SwitchToMainThreadAsync(CancellationToken token = default) =>
            _joinableTaskFactory.SwitchToMainThreadAsync(token);

        public JoinableTaskFactory.MainThreadAwaitable SwitchToMainThreadAsync(bool alwaysYield, CancellationToken token = default) =>
            _joinableTaskFactory.SwitchToMainThreadAsync(alwaysYield, token);
#pragma warning restore VSTHRD004
    }
}
