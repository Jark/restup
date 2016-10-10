using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Restup.Webserver.Models.Contracts;

namespace Restup.Webserver.Rest
{
    internal static class ReturnTypeWrapperFactory
    {
        public static IReturnTypeWrapper Create(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (HasRestResponse(returnType))
                return new NoneReturnTypeWrapper(method);
            if (HasAsyncRestResponse(returnType, typeof(Task<>)))
                return new TaskReturnTypeWrapper(method);
            if (HasAsyncRestResponse(returnType, typeof(IAsyncOperation<>)))
                return new TaskAsyncOperationReturnTypeWrapper(method);

            throw new Exception($"Method {method} does not have a response which inherits from IRestResponse");
        }

        private static bool HasRestResponse(Type returnType)
        {
            return returnType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IRestResponse));
        }

        private static bool HasAsyncRestResponse(Type returnType, Type type)
        {
            if (!returnType.IsConstructedGenericType)
                return false;

            var genericTypeDefinition = returnType.GetGenericTypeDefinition();
            var isAsync = genericTypeDefinition == type;
            if (!isAsync)
                return false;

            var genericArgs = returnType.GetGenericArguments();
            if (!genericArgs.Any())
            {
                return false;
            }

            return genericArgs[0].GetTypeInfo().ImplementedInterfaces.Contains(typeof(IRestResponse));
        }
    }
}