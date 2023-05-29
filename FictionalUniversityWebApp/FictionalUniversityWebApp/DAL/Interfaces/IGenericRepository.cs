using System.Linq.Expressions;

namespace FictionalUniversityWebApp.DAL.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        void Delete(object id);

        void Delete(TEntity entityToDelete);

        Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        Task<TEntity> GetByIDAsync(object id);

        void Insert(TEntity entity);

        void Update(TEntity entityToUpdate);
    }
}