using System;
using System.Linq;
using System.Reflection;
using Restup.Webserver.Models.Schemas;

namespace Restup.Webserver.Rest
{
    internal class RestControllerMethodExecutor : RestMethodExecutor
    {
        public RestControllerMethodExecutor(
            ConstructorInfo constructor, Func<object[]> constructorArgs,
            MethodInfo method, IReturnTypeWrapper returnTypeWrapper)
            : base(constructor, constructorArgs, method, returnTypeWrapper)
        {
        }

        protected override bool TryGetMethodParametersFromRequest(RestControllerMethodInfo methodInfo,
            RestServerRequest request, ParsedUri requestUri, out object[] methodParameters)
        {
            try
            {
                methodParameters = methodInfo.GetParametersFromUri(requestUri).ToArray();
                return true;
            }
            catch (FormatException)
            {
                methodParameters = null;
                return false;
            }
        }
    }
}
