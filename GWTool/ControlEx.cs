//====================================================================================================
// Copyright (c) 2025 The Aridity Team, all rights reserved.
//
// The Aridity Cereon, an .NET integrated development environment..
//
//====================================================================================================

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GWTool
{
    /// <summary>
    /// Provides extension methods for the <seealso cref="Control"/> class, needed on some parts of Cereon.
    /// </summary>
    public static class ControlEx
    {
        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="action">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>A task that has been completed.</returns>
        public static Task InvokeAsync(this Control control, Action action)
        {
            var tcs = new TaskCompletionSource<object>();

            IAsyncResult result = null;

            try
            {
                result = control.BeginInvoke(new Action(() =>
                {
                    try { action(); tcs.SetResult(null); }
                    catch (Exception ex) { tcs.SetException(ex); }
                }));
            }
            finally
            {
                control.EndInvoke(result);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="action">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>A task that has been completed.</returns>
        public static Task<object> InvokeAsync(this Control control, Func<object> action)
        {
            var tcs = new TaskCompletionSource<object>();

            IAsyncResult result = null;

            try
            {
                result = control.BeginInvoke(new Action(() =>
                {
                    try { tcs.SetResult(action()); }
                    catch (Exception ex) { tcs.SetException(ex); }
                }));
            }
            finally
            {
                control.EndInvoke(result);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="action">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>A task that has been completed.</returns>
        public static Task<T> InvokeAsync<T>(this Control control, Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();

            IAsyncResult result = null;

            try
            {
                result = control.BeginInvoke(new Action(() =>
                {
                    try { tcs.SetResult(action()); }
                    catch (Exception ex) { tcs.SetException(ex); }
                }));
            }
            finally
            {
                control.EndInvoke(result);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="action">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <param name="args">
        /// An array of type System.Object that represents the arguments to pass to the given method. This can be null if
        /// no arguments are needed.
        /// </param>
        /// <returns>A task that has been completed.</returns>
        public static Task<T> InvokeAsync<T>(this Control control, Func<T> action, params object[] args)
        {
            var tcs = new TaskCompletionSource<T>();

            IAsyncResult result = null;

            try
            {
                result = control.BeginInvoke(new Action(() =>
                {
                    try { tcs.SetResult(action()); }
                    catch (Exception ex) { tcs.SetException(ex); }
                }), args);
            }
            finally
            {
                control.EndInvoke(result);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>The object's instance returned by the method.</returns>
        public static object InvokeIfRequired(this Control control, Action method) =>
            InvokeIfRequired(control, method, null);

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <param name="args">
        /// An array of type System.Object that represents the arguments to pass to the given method. This can be null if
        /// no arguments are needed.
        /// </param>
        /// <returns>The object's instance returned by the method.</returns>
        public static object InvokeIfRequired(this Control control, Action method, object[] args)
        {
            return InvokeIfRequired(control, new Func<object>(() =>
            {
                method.Invoke();
                return new object();
            }), args);
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>The object's instance returned by the method.</returns>
        public static object InvokeIfRequired(this Control control, Func<object> method) =>
            InvokeIfRequired(control, method, null);

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <param name="args">
        /// An array of type System.Object that represents the arguments to pass to the given method. This can be null if
        /// no arguments are needed.
        /// </param>
        /// <returns>The object's instance returned by the method.</returns>
        public static object InvokeIfRequired(this Control control, Func<object> method, object[] args)
        {
            if (control.InvokeRequired)
                return control.Invoke(method, args);
                
            return method.Invoke();
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>The <typeparamref name="T"/>'s instance returned by the method.</returns>
        public static T InvokeIfRequired<T>(this Control control, Func<T> method)
            => InvokeIfRequired<T>(control, method, null);

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <param name="args">
        /// An array of type System.Object that represents the arguments to pass to the given method. This can be null if
        /// no arguments are needed.
        /// </param>
        /// <returns>The <typeparamref name="T"/>'s instance returned by the method.</returns>
        public static T InvokeIfRequired<T>(this Control control, Func<T> method, object[] args)
        {
            if (control.InvokeRequired)
                return (T)control.Invoke(method, args);
                
            return method.Invoke();
        }

        // VSTHRD200: Use "Async" suffix in names of methods that return an awaitable type.
#pragma warning disable VSTHRD200
        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>The object's instance returned by the method.</returns>
        public static Task<object> InvokeAsyncIfRequired(this Control control, Action method) =>
            InvokeAsyncIfRequired(control, method, null);

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <param name="args">
        /// An array of type System.Object that represents the arguments to pass to the given method. This can be null if
        /// no arguments are needed.
        /// </param>
        /// <returns>The object's instance returned by the method.</returns>
        public static Task<object> InvokeAsyncIfRequired(this Control control, Action method, object[] args) =>
            InvokeAsyncIfRequired(control, new Func<object>(() =>
            {
                method.Invoke();
                return new object();
            }), args);

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>The object's instance returned by the method.</returns>
        public static Task<object> InvokeAsyncIfRequired(this Control control, Func<object> method) =>
            InvokeAsyncIfRequired(control, method, null);

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <param name="args">
        /// An array of type System.Object that represents the arguments to pass to the given method. This can be null if
        /// no arguments are needed.
        /// </param>
        /// <returns>The object's instance returned by the method.</returns>
        public static Task<object> InvokeAsyncIfRequired(this Control control, Func<object> method, params object[] args)
        {
            if (control.InvokeRequired)
                return InvokeAsync(control, method, args);
                
            return Task.FromResult(method.Invoke());
        }

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <returns>The <typeparamref name="T"/>'s instance returned by the method.</returns>
        public static T InvokeAsyncIfRequired<T>(this Control control, Func<T> method)
            => InvokeIfRequired<T>(control, method, null);

        /// <summary>
        /// Executes the specified delegate on the thread that owns the control's underlying
        /// window handle asynchronously, only if it's required to do so.
        /// </summary>
        /// <param name="control">The value of the <seealso cref="Control"/> instance.</param>
        /// <param name="method">The value of a delegate that contains a method to be called in the control's thread context.</param>
        /// <param name="args">
        /// An array of type System.Object that represents the arguments to pass to the given method. This can be null if
        /// no arguments are needed.
        /// </param>
        /// <returns>The <typeparamref name="T"/>'s instance returned by the method.</returns>
        public static Task<T> InvokeAsyncIfRequired<T>(this Control control, Func<T> method, params object[] args)
        {
            if (control.InvokeRequired)
                return InvokeAsync<T>(control, method, args);

            return Task.FromResult(method.Invoke());
        }
#pragma warning restore VSTHRD200
    }
}
