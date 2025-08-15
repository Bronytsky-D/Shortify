
namespace Shortify.Infrastructure.PostgreSQL
{
    public class ExecutionResponse : IExecutionResponse
    {
        public ExecutionResponse(object data, IEnumerable<string> errors)
        {
            Result = data;
            Errors = errors;
        }

        public bool Success
        {
            get
            {
                return Errors.Count() == 0;
            }
        }

        public object Result
        {
            get; private set;
        }

        public IEnumerable<string> Errors
        {
            get; private set;
        } = new List<string>();

        public static ExecutionResponse Successful(object result)
        {
            return new ExecutionResponse(result, new List<string>());
        }

        public static ExecutionResponse Failure(string error)
        {
            return new ExecutionResponse(default, new List<string> { error });
        }

        public static ExecutionResponse Failure(IEnumerable<string> errors)
        {
            return new ExecutionResponse(default, errors);
        }
    }
}
