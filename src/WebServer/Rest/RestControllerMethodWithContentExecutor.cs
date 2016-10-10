using Newtonsoft.Json;
using Restup.Webserver.Http;
using Restup.Webserver.InstanceCreators;
using Restup.Webserver.Models.Schemas;
using System;
using System.Linq;
using System.Reflection;

namespace Restup.Webserver.Rest
{
    internal class RestControllerMethodWithContentExecutor : RestMethodExecutor
    {
        private readonly ContentSerializer _contentSerializer;
        private readonly Type _contentParameterType;

        public RestControllerMethodWithContentExecutor(
            ConstructorInfo constructor, Func<object[]> constructorArgs,
            MethodInfo method, IReturnTypeWrapper returnTypeWrapper, Type contentParameterType)
            : base(constructor, constructorArgs, method, returnTypeWrapper)
        {
            _contentSerializer = new ContentSerializer();
            _contentParameterType = contentParameterType;
        }

        protected override bool TryGetMethodParametersFromRequest(RestControllerMethodInfo methodInfo, RestServerRequest request, ParsedUri requestUri, out object[] methodParameters)
        {
            object contentObj = null;
            try
            {
                if (request.HttpServerRequest.Content != null)
                {
                    contentObj = _contentSerializer.FromContent(
                        request.ContentEncoding.GetString(request.HttpServerRequest.Content),
                        request.ContentMediaType,
                        _contentParameterType);
                }
            }
            catch (JsonReaderException)
            {
                methodParameters = null;
                return false;
            }
            catch (InvalidOperationException)
            {
                methodParameters = null;
                return false;
            }

            try
            {
                methodParameters = methodInfo.GetParametersFromUri(requestUri).Concat(new[] { contentObj }).ToArray();
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
