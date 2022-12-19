using AutoMapper;
using UserManagement.Core.DTOs;

namespace UserManagement.Core.Utilities
{
    public static class Paginator
    {
        public static PaginationResult<IEnumerable<TDestination>> PaginationAsync<TSource, TDestination>(this IQueryable<TSource> queryable,
            int pageSize, int pageNumber, IMapper mapper)
            where TSource : class
            where TDestination : class
        {
            var count = queryable.Count();
            var pageResult = new PaginationResult<IEnumerable<TDestination>>
            {
                PageSize = (pageSize > 10 || pageNumber < 1) ? 10 : pageSize,
                CurrentPage = pageNumber > 1 ? pageNumber : 1,
                PreviousPage = pageNumber > 0 ? pageNumber - 1 : 0,
            };

            pageResult.NumberOfPages = count % pageResult.PageSize != 0
                ? count / pageResult.PageSize + 1
                : count / pageResult.PageSize;

            var sourceList = queryable.Skip((pageResult.CurrentPage - 1) * pageResult.PageSize).Take(pageResult.PageSize).ToList();
            var destinationList = mapper.Map<IEnumerable<TDestination>>(sourceList);
            pageResult.PageItems = destinationList;
            return pageResult;
        }
    }
}
