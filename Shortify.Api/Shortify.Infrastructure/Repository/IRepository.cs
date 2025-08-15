using System.Linq.Expressions;

namespace Shortify.Infrastructure.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<IExecutionResponse> AddAsync(T entity);
        Task<IExecutionResponse> UpdateAsync(T entity);
        Task<IExecutionResponse> RemoveAsync(T entity);
    }
}
