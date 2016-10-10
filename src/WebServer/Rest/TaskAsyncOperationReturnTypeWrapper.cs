using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Restup.Webserver.Models.Contracts;

namespace Restup.Webserver.Rest
{
    public class TaskAsyncOperationReturnTypeWrapper : IReturnTypeWrapper
    {
        public TypeInfo ReturnType { get; }
        
        public TaskAsyncOperationReturnTypeWrapper(MethodInfo methodInfo)
        {
            ReturnType = methodInfo.ReturnType.GetGenericArguments()[0].GetTypeInfo();
        }

        public async Task<IRestResponse> WrapResponse(object methodInvokeResult)
        {
            return await ConvertToTask((dynamic)methodInvokeResult);
        }

        private static Task<T> ConvertToTask<T>(IAsyncOperation<T> methodInvokeResult)
        {
            return methodInvokeResult.AsTask();
        }
    }
}