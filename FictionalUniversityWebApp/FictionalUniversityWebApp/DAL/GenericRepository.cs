using System.Linq.Expressions;
using FictionalUniversityWebApp.Data;
using Microsoft.EntityFrameworkCore;
using FictionalUniversityWebApp.DAL.Interfaces;

namespace TestWebApplication.DAL
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        internal EducationDBContext context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(EducationDBContext context)
        {
            this.context = context;
            dbSet = context.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public virtual async Task<TEntity> GetByIDAsync(object id)
        {
            IQueryable<TEntity> query = dbSet;
            return await dbSet.FindAsync(id);
        }

        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}