using Microsoft.EntityFrameworkCore;

namespace BookMS.Models.ViewModels
{
    /// <summary>
    /// 分页类
    /// </summary>
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }//页码
        public int TotalPages { get; private set; }//页数

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;//页码大于1有前一页

        public bool HasNextPage => PageIndex < TotalPages;//页码小于页数有后一页

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();//数据条数
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();//跳过（页码-1）*页大小 条数据
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }        
    }
}
