using System;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace BulletHellTemplate
{
    /// <summary>
    /// Provides extension methods for UnityWebRequestAsyncOperation to enable awaiting.
    /// </summary>
    public static class UnityWebRequestAsyncOperationExtensions
    {
        /// <summary>
        /// Returns an awaiter for UnityWebRequestAsyncOperation.
        /// </summary>
        /// <param name="asyncOp">The UnityWebRequestAsyncOperation instance.</param>
        /// <returns>An awaiter for UnityWebRequestAsyncOperation.</returns>
        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAsyncOperationAwaiter(asyncOp);
        }
    }

    /// <summary>
    /// Awaiter implementation for UnityWebRequestAsyncOperation.
    /// </summary>
    public class UnityWebRequestAsyncOperationAwaiter : INotifyCompletion
    {
        private UnityWebRequestAsyncOperation asyncOp;

        /// <summary>
        /// Initializes a new instance of the UnityWebRequestAsyncOperationAwaiter class.
        /// </summary>
        /// <param name="asyncOp">The UnityWebRequestAsyncOperation instance to await.</param>
        public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation asyncOp)
        {
            this.asyncOp = asyncOp;
        }

        /// <summary>
        /// Indicates whether the operation is completed.
        /// </summary>
        public bool IsCompleted => asyncOp.isDone;

        /// <summary>
        /// Schedules the continuation action to be invoked when the operation completes.
        /// </summary>
        /// <param name="continuation">The action to invoke upon completion.</param>
        public void OnCompleted(Action continuation)
        {
            asyncOp.completed += _ => continuation();
        }

        /// <summary>
        /// Returns the result of the operation.
        /// </summary>
        /// <returns>The UnityWebRequestAsyncOperation instance.</returns>
        public UnityWebRequestAsyncOperation GetResult()
        {
            return asyncOp;
        }
    }
}
