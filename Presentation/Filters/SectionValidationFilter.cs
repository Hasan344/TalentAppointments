
using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Presentation.Filters
{

    public class SectionValidationFilter<T> : IAsyncActionFilter where T : class
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public SectionValidationFilter(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.ContainsKey("id"))
            {
                var id = (int)context.ActionArguments["id"];

                // Get the current user and their SectionId
                var user = await _userManager.GetUserAsync(context.HttpContext.User);
                var sectionId = user?.SectionId;

                // Resolve DbContext dynamically
                var dbContext = (DbContext)_serviceProvider.GetService(typeof(MyDbContext));
                var dbSet = dbContext.Set<T>();

                // Find the entity by ID
                var entity = await dbSet.FindAsync(id);
                if (entity == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                // Check if the SectionId matches
                var sectionIdProperty = typeof(T).GetProperty("SectionId");
                if (sectionIdProperty == null)
                {
                    context.Result = new ForbidResult();
                    return;
                }

                var entitySectionId = sectionIdProperty.GetValue(entity) as int?;
                if (entitySectionId != sectionId)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }


}
