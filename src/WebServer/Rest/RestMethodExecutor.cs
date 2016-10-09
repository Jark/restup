using Restup.Webserver.Models;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Restup.Webserver.InstanceCreators;

namespace Restup.Webserver.Rest
{
    internal abstract class RestMethodExecutor : IRestMethodExecutor
    {
        private readonly RestResponseFactory _responseFactory;
        private readonly IInstanceCreator _instantiator;

        private readonly ConstructorInfo _constructor;
        private readonly Func<object[]> _constructorArgs;
        private readonly MethodInfo _method;

        protected RestMethodExecutor(ConstructorInfo constructor, Func<object[]> constructorArgs, MethodInfo method)
        {
            _responseFactory = new RestResponseFactory();
            _instantiator = InstanceCreatorCache.Default.GetCreator(method.DeclaringType);

            _constructor = constructor;
            _constructorArgs = constructorArgs;
            _method = method;
        }

        public async Task<IRestResponse> ExecuteMethodAsync(RestControllerMethodInfo info, RestServerRequest request, ParsedUri requestUri)
        {
            object[] parameters;
            object methodInvokeResult;
            if (TryGetMethodParametersFromRequest(info, request, requestUri, out parameters))
            {
                methodInvokeResult = ExecuteMethod(parameters);
            }
            else
            {
                methodInvokeResult = _responseFactory.CreateBadRequest();
            }

            switch (info.ReturnTypeWrapper)
            {
                case RestControllerMethodInfo.TypeWrapper.None:
                    return await Task.FromResult((IRestResponse)methodInvokeResult);
                case RestControllerMethodInfo.TypeWrapper.AsyncOperation:
                    return await ConvertToTask((dynamic)methodInvokeResult);
                case RestControllerMethodInfo.TypeWrapper.Task:
                    return await (dynamic)methodInvokeResult;
            }

            throw new Exception($"ReturnTypeWrapper of type {info.ReturnTypeWrapper} not known.");
        }

        protected abstract bool TryGetMethodParametersFromRequest(RestControllerMethodInfo contentParameterType, RestServerRequest request, ParsedUri requestUri, out object[] methodParameters);

        private object ExecuteMethod(object[] parameters)
        {
            var constructor = _instantiator.Create(_constructor, _constructorArgs());
            return _method.Invoke(constructor, parameters);
        }

        private static Task<T> ConvertToTask<T>(IAsyncOperation<T> methodInvokeResult)
        {
            return methodInvokeResult.AsTask();
        }
    }
}