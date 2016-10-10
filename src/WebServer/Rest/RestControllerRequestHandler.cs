using Restup.HttpMessage.Models.Schemas;
using Restup.Webserver.Attributes;
using Restup.Webserver.InstanceCreators;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Restup.Webserver.Rest
{
    internal class RestControllerRequestHandler
    {
        private ImmutableArray<RestControllerMethodInfo> _restMethodCollection;
        private readonly RestResponseFactory _responseFactory;
        private readonly UriParser _uriParser;
        private readonly RestControllerMethodInfoValidator _restControllerMethodInfoValidator;

        internal RestControllerRequestHandler()
        {
            _restMethodCollection = ImmutableArray<RestControllerMethodInfo>.Empty;
            _responseFactory = new RestResponseFactory();
            _uriParser = new UriParser();
            _restControllerMethodInfoValidator = new RestControllerMethodInfoValidator();
        }

        internal void RegisterController<T>() where T : class
        {
            RegisterController<T>(() => Enumerable.Empty<object>().ToArray());
        }

        internal void RegisterController<T>(Func<object[]> constructorArgs) where T : class
        {
            constructorArgs.GuardNull(nameof(constructorArgs));

            ConstructorInfo constructorInfo;
            if (!ReflectionHelper.TryFindMatchingConstructor<T>(constructorArgs, out constructorInfo))
            {
                throw new Exception($"No constructor found on {typeof (T)} that matches passed in constructor arguments.");
            }

            var restControllerMethodInfos = GetRestMethods<T>(constructorArgs, constructorInfo);
            AddRestMethods<T>(restControllerMethodInfos);
        }

        private void AddRestMethods<T>(IEnumerable<RestControllerMethodInfo> restControllerMethodInfos) where T : class
        {
            var newControllerMethodInfos = restControllerMethodInfos.ToArray();

            _restControllerMethodInfoValidator.Validate<T>(_restMethodCollection, newControllerMethodInfos);

            _restMethodCollection = _restMethodCollection.Concat(newControllerMethodInfos)
                .OrderByDescending(x => x.ParametersCount)
                .ToImmutableArray();

            InstanceCreatorCache.Default.CacheCreator(typeof(T));
        }

        private IEnumerable<RestControllerMethodInfo> GetRestMethods<T>(Func<object[]> constructorArgs, ConstructorInfo constructor) where T : class
        {
            var possibleValidRestMethods = (from m in typeof(T).GetRuntimeMethods()
                                            where m.IsPublic &&
                                                  m.IsDefined(typeof(UriFormatAttribute))
                                            select m).ToList();

            foreach (var restMethod in possibleValidRestMethods)
            {
                var restMethodExecutor = RestMethodExecutorFactory.Create(restMethod, constructor, constructorArgs);
                yield return new RestControllerMethodInfo(restMethod, restMethodExecutor);
            }
        }

        internal async Task<IRestResponse> HandleRequestAsync(RestServerRequest req)
        {
            if (!req.HttpServerRequest.IsComplete ||
                req.HttpServerRequest.Method == HttpMethod.Unsupported)
            {
                return _responseFactory.CreateBadRequest();
            }

            ParsedUri parsedUri;
            var incomingUriAsString = req.HttpServerRequest.Uri.ToRelativeString();
            if (!_uriParser.TryParse(incomingUriAsString, out parsedUri))
            {
                throw new Exception($"Could not parse uri: {incomingUriAsString}");
            }

            var restMethods = _restMethodCollection.Where(r => r.Match(parsedUri)).ToList();
            if (!restMethods.Any())
            {
                return _responseFactory.CreateBadRequest();
            }

            var restMethod = restMethods.FirstOrDefault(r => r.Verb == req.HttpServerRequest.Method);
            if (restMethod == null)
            {
                return new MethodNotAllowedResponse(restMethods.Select(r => r.Verb));
            }

            try
            {
                return await restMethod.ExecuteAsync(req, parsedUri);
            }
            catch
            {
                return _responseFactory.CreateBadRequest();
            }
        }
    }
}
