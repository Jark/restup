using System.Reflection;
using System.Threading.Tasks;
using Restup.Webserver.Models.Contracts;

namespace Restup.Webserver.Rest
{
    public class NoneReturnTypeWrapper : IReturnTypeWrapper
    {
        public TypeInfo ReturnType { get; }

        public NoneReturnTypeWrapper(MethodInfo methodInfo)
        {
            ReturnType = methodInfo.ReturnType.GetTypeInfo();
        }

        public Task<IRestResponse> WrapResponse(object methodInvokeResult)
        {
            return Task.FromResult((IRestResponse)methodInvokeResult);
        }
    }
}