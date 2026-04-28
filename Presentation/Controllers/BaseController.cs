
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ForQab.DataAccess.Models;

namespace ForQab.Presentation.Controllers
{

    public abstract class BaseController : Controller
    {
        protected readonly MyDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;

        protected BaseController(MyDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Verilen model ve id için Section doğrulaması yapar.
        /// </summary>
        /// <typeparam name="TEntity">DbContext içindeki model türü</typeparam>
        /// <param name="id">Modelin ID değeri</param>
        /// <returns>Section doğrulaması başarılıysa true, aksi halde false</returns>
        protected async Task<bool> IsSectionValidAsync<TEntity>(int id) where TEntity : class
        {
            var user = await _userManager.GetUserAsync(User);
            var sectionId = user?.SectionId;

            if (sectionId == null)
                return true;

            // Modeli al ve SectionId kontrolü yap
            var entity = await _context.Set<TEntity>().FindAsync(id);

            if (entity == null)
                return false;
             
            // SectionId'nin varlığını kontrol et
            var sectionProperty = typeof(TEntity).GetProperty("SectionId");
            if (sectionProperty == null)
                return false;

            var entitySectionId = (int?)sectionProperty.GetValue(entity);
            return entitySectionId == sectionId;
        }
        protected async Task<bool> IsAdminValidAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user?.IsAdmin;


            if(isAdmin == 0)
                return false;
            else
                return true;
        }
    }

}
