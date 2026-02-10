using ThirteenBytes.DDDPatterns.Primitives.Abstractions;

namespace ThirteenBytes.DDDPatterns.Primitives.Extensions
{
    /// <summary>
    /// LINQ query extensions for filtering soft deletable entities.
    /// </summary>
    public static class DeletableQueryExtensions
    {
        /// <summary>
        /// Filters query to exclude soft deleted entities (WHERE DeletedAtUtc IS NULL).
        /// </summary>
        /// <typeparam name="T">Entity type implementing IDeletable.</typeparam>
        /// <param name="query">Source queryable to filter.</param>
        /// <returns>Queryable containing only active entities.</returns>
        public static IQueryable<T> ExcludeDeleted<T>(this IQueryable<T> query)
            where T : class, IDeletable
        {
            return query.Where(e => e.DeletedAtUtc == null);
        }

        /// <summary>
        /// Filters query to include only soft deleted entities (WHERE DeletedAtUtc IS NOT NULL).
        /// Useful for cleanup operations and recovery UI.
        /// </summary>
        /// <typeparam name="T">Entity type implementing IDeletable.</typeparam>
        /// <param name="query">Source queryable to filter.</param>
        /// <returns>Queryable containing only soft deleted entities.</returns>
        public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query)
            where T : class, IDeletable
        {
            return query.Where(e => e.DeletedAtUtc != null);
        }
    }
}
