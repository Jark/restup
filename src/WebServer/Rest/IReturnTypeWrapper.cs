using System.Reflection;
using System.Threading.Tasks;
using Restup.Webserver.Models.Contracts;

namespace Restup.Webserver.Rest
{
    internal interface IReturnTypeWrapper
    {
        TypeInfo ReturnType { get; }
        Task<IRestResponse> WrapResponse(object methodExecutionResult);
    }
}