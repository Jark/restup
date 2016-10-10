using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Restup.HttpMessage.Models.Schemas;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;

namespace Restup.Webserver.Rest
{
    internal class RestControllerMethodInfo
    {
        private readonly IEnumerable<Type> _validParameterTypes;
        private readonly UriParser _uriParser;
        private readonly IEnumerable<ParameterValueGetter> _parameterGetters;
        private readonly IRestMethodExecutor _restMethodExecutor;

        internal int ParametersCount { get; }

        internal ParsedUri MatchUri { get; }
        internal HttpMethod Verb { get; }

        internal RestControllerMethodInfo(MethodInfo methodInfo, IRestMethodExecutor restMethodExecutor)
        {
            _restMethodExecutor = restMethodExecutor;

            ParametersCount = methodInfo.GetParameters().Count();
            _uriParser = new UriParser();
            MatchUri = GetUriFromMethod(methodInfo);

            _validParameterTypes = GetValidParameterTypes();
            _parameterGetters = GetParameterGetters(methodInfo);
            Verb = GetVerb(restMethodExecutor.ReturnType);           
        }

        private ParsedUri GetUriFromMethod(MethodInfo methodInfo)
        {
            var uriFormatter = methodInfo.GetCustomAttribute<UriFormatAttribute>();
            ParsedUri parsedUri;
            if (!_uriParser.TryParse(uriFormatter.UriFormat, out parsedUri))
                throw new Exception($"Could not parse uri: {uriFormatter.UriFormat}");

            return parsedUri;
        }

        private Type[] GetValidParameterTypes()
        {
            return new[] {
                typeof(IEnumerable<byte>),
                typeof(IEnumerable<sbyte>),
                typeof(IEnumerable<short>),
                typeof(IEnumerable<ushort>),
                typeof(IEnumerable<int>),
                typeof(IEnumerable<uint>),
                typeof(IEnumerable<long>),
                typeof(IEnumerable<ulong>),
                typeof(IEnumerable<decimal>),
                typeof(IEnumerable<double>),
                typeof(IEnumerable<float>),
                typeof(IEnumerable<string>),
                typeof(IEnumerable<bool>),
                typeof(IEnumerable<char>),
                typeof(string),
                typeof(decimal),
                typeof(double),
                typeof(float),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(byte),
                typeof(bool),
                typeof(DateTime),
                typeof(char),
                typeof(sbyte),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
            };
        }

        private ParameterValueGetter[] GetParameterGetters(MethodInfo methodInfo)
        {
            var methodParameters = (from p in methodInfo.GetParameters()
                                    where p.GetCustomAttribute<FromContentAttribute>() == null
                                    select p).ToList();

            if (!ParametersHaveValidType(methodParameters.Select(p => p.ParameterType)))
            {
                throw new InvalidOperationException("Can't use method parameters with a custom type.");
            }

            var parameterValueGetters = methodParameters.Select(x => GetParameterGetter(x, MatchUri)).ToArray();
            if (parameterValueGetters.Length !=
                MatchUri.Parameters.Count + MatchUri.PathParts.Count(x => x.PartType == PathPart.PathPartType.Argument))
            {
                throw new Exception($"Uri format {MatchUri} has got more method parameters defined than the method has got.");
            }

            return parameterValueGetters;
        }

        private static ParameterValueGetter GetParameterGetter(ParameterInfo parameterInfo, ParsedUri matchUri)
        {
            var methodName = parameterInfo.Name;
            var firstPathPartMatch = matchUri.PathParts
                .Select((x, i) => new { Part = x, Index = i })
                .Where(x => x.Part.PartType == PathPart.PathPartType.Argument)
                .FirstOrDefault(x => methodName.Equals(x.Part.Value, StringComparison.OrdinalIgnoreCase));

            var parameterType = parameterInfo.ParameterType;
            if (firstPathPartMatch != null)
                return new PathParameterValueGetter(methodName, parameterType, firstPathPartMatch.Index);

            var firstQueryParameterMatch = matchUri.Parameters
                .FirstOrDefault(x => methodName.Equals(x.Value, StringComparison.OrdinalIgnoreCase));

            if (firstQueryParameterMatch != null)
                return new QueryParameterValueGetter(methodName, parameterType, firstQueryParameterMatch);

            throw new Exception($"Method {methodName} not found in rest controller method uri {matchUri}.");
        }

        private bool ParametersHaveValidType(IEnumerable<Type> parameters)
        {
            return !parameters.Except(_validParameterTypes).Any();
        }

        private HttpMethod GetVerb(TypeInfo returnType)
        {
            if (IsRestResponseOfType<IGetResponse>(returnType))
                return HttpMethod.GET;
            if (IsRestResponseOfType<IPostResponse>(returnType))
                return HttpMethod.POST;
            if (IsRestResponseOfType<IPutResponse>(returnType))
                return HttpMethod.PUT;
            if (IsRestResponseOfType<IDeleteResponse>(returnType))
                return HttpMethod.DELETE;

            throw new ArgumentException($"Verb for return type {returnType} not know. Please use only {typeof(IGetResponse)}, {typeof(IPostResponse)}, {typeof(IPutResponse)}, {typeof(IDeleteResponse)} as return types.");
        }

        private static bool IsRestResponseOfType<T>(TypeInfo returnType)
        {
            return returnType.ImplementedInterfaces.Contains(typeof(T)) || returnType.AsType() == typeof(T);
        }

        internal bool Match(ParsedUri uri)
        {
            if (MatchUri.PathParts.Count != uri.PathParts.Count)
                return false;

            for (var i = 0; i < MatchUri.PathParts.Count; i++)
            {
                var fromPart = MatchUri.PathParts[i];
                var toPart = uri.PathParts[i];
                if (fromPart.PartType == PathPart.PathPartType.Argument)
                    continue;

                if (!fromPart.Value.Equals(toPart.Value, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (uri.Parameters.Count < MatchUri.Parameters.Count)
                return false;

            return MatchUri.Parameters.All(x => uri.Parameters.Any(y => y.Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));
        }

        internal IEnumerable<object> GetParametersFromUri(ParsedUri uri)
        {
            return _parameterGetters.Select(x => x.GetParameterValue(uri)).ToArray();
        }

        public override string ToString()
        {
            return $"Hosting {Verb} method on {MatchUri}";
        }

        public Task<IRestResponse> ExecuteAsync(RestServerRequest req, ParsedUri parsedUri)
        {
            return _restMethodExecutor.ExecuteMethodAsync(this, req, parsedUri);
        }
    }
}
