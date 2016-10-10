using System.Reflection;
using System.Threading.Tasks;
using Restup.Webserver.Models.Contracts;

namespace Restup.Webserver.Rest
{
    public class TaskReturnTypeWrapper : IReturnTypeWrapper
    {
        public TypeInfo ReturnType { get; }

        public TaskReturnTypeWrapper(MethodInfo methodInfo)
        {
            ReturnType = methodInfo.ReturnType.GetGenericArguments()[0].GetTypeInfo();
        }

        public async Task<IRestResponse> WrapResponse(object methodInvokeResult)
        {
            return await (dynamic)methodInvokeResult;
        }
    }
}