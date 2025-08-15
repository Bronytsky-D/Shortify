using Microsoft.EntityFrameworkCore;
using Shortify.Infrastructure.PostgreSQL.ApplicationContext;
using Shortify.Infrastructure.Repository;
using System.Linq.Expressions;

namespace Shortify.Infrastructure.PostgreSQL.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IExecutionResponse> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var response = await _context.Set<T>().Where(predicate).ToListAsync();
            if (response == null)
            {
                return ExecutionResponse.Failure($"Entity not found.");
            }
            return ExecutionResponse.Successful(response);
        }
        public async Task<IExecutionResponse> GetAllAsync()
        {
            var respone = await _context.Set<T>().ToListAsync();
            return  ExecutionResponse.Successful(respone);
        }
        public async Task<IExecutionResponse> AddAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();

            return ExecutionResponse.Successful(entity);
        }
        public async Task<IExecutionResponse> UpdateAsync(T entity) 
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();

            return ExecutionResponse.Successful(entity);
        }
        public async Task<IExecutionResponse> RemoveAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();

            return ExecutionResponse.Successful(entity);
        }
      
    }
}