using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace ForQab.Extensions
{
    public static class QueryableExtension
    {
        //public static IQueryable<T> IncludeRelations<T>(this IQueryable<T> query) where T : class
        //{
        //    // Monitor tipi için ilişkileri dahil et
        //    if (typeof(T) == typeof(Monitor))
        //    {
        //        query = query
        //            .Include(m => m.Section) // Monitor tipi için Section ilişkisini dahil et
        //            .Include(m => m.DistrictNavigation) // Monitor tipi için District ilişkisini dahil et
        //            .Include(m => m.GenderNavigation) // Monitor tipi için Gender ilişkisini dahil et
        //            .Include(m => m.RoleNavigation); // Monitor tipi için Role ilişkisini dahil et
        //    }
        //    // Diğer varlık türlerine yönelik özel Include işlemleri eklenebilir
        //    return query;
        //}
    }
}
