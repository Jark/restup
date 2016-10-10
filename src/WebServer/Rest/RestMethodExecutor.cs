using System;
using System.Reflection;
using System.Threading.Tasks;
using Restup.Webserver.InstanceCreators;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;

namespace Restup.Webserver.Rest
{
    internal abstract class RestMethodExecutor : IRestMethodExecutor
    {
        public TypeInfo ReturnType => _returnTypeWrapper.ReturnType;

        private readonly RestResponseFactory _responseFactory;
        private readonly IInstanceCreator _instantiator;

        private readonly ConstructorInfo _constructor;
        private readonly Func<object[]> _constructorArgs;
        private readonly MethodInfo _method;
        private readonly IReturnTypeWrapper _returnTypeWrapper;

        protected RestMethodExecutor(ConstructorInfo constructor, Func<object[]> constructorArgs, MethodInfo method, IReturnTypeWrapper returnTypeWrapper)
        {
            _responseFactory = new RestResponseFactory();
            _instantiator = InstanceCreatorCache.Default.GetCreator(method.DeclaringType);

            _constructor = constructor;
            _constructorArgs = constructorArgs;
            _method = method;
            _returnTypeWrapper = returnTypeWrapper;
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

            return await _returnTypeWrapper.WrapResponse(methodInvokeResult);
        }
        
        protected abstract bool TryGetMethodParametersFromRequest(RestControllerMethodInfo contentParameterType, RestServerRequest request, ParsedUri requestUri, out object[] methodParameters);

        private object ExecuteMethod(object[] parameters)
        {
            var constructor = _instantiator.Create(_constructor, _constructorArgs());
            return _method.Invoke(constructor, parameters);
        }
    }
}