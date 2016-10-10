using System;
using System.Linq;
using System.Reflection;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;

namespace Restup.Webserver.Rest
{
    internal static class RestMethodExecutorFactory
    {
        public static IRestMethodExecutor Create(MethodInfo methodInfo, ConstructorInfo constructor, Func<object[]> constructorArgs)
        {
            var returnTypeWrapper = ReturnTypeWrapperFactory.Create(methodInfo);

            Type contentParameterType;
            if (TryGetContentParameterType(methodInfo, out contentParameterType))
            {
                return new RestControllerMethodWithContentExecutor(constructor, constructorArgs, methodInfo, returnTypeWrapper, contentParameterType);
            }

            return  new RestControllerMethodExecutor(constructor, constructorArgs, methodInfo, returnTypeWrapper);
        }

        private static bool TryGetContentParameterType(MethodInfo methodInfo, out Type content)
        {
            var fromContentParameter = methodInfo.GetParameters().FirstOrDefault(p => CustomAttributeExtensions.GetCustomAttribute<FromContentAttribute>((ParameterInfo) p) != null);
            if (fromContentParameter != null)
            {
                content = fromContentParameter.ParameterType;
                return true;
            }

            content = null;
            return false;
        }
    }
}