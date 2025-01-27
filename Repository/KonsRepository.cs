using DocumentFormat.OpenXml.InkML;
using ForQab.Data_Access.ViewModel;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Expert;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository
{
    public class KonsRepository :  IKonsRepository
    {
        private readonly MyDbContext _dbContext;
        public KonsRepository(MyDbContext dbContext) 
        {
            _dbContext = dbContext;
        }
        public async Task<List<Expert>> GetAllAsync(int? sectionId, int? role, Expression<Func<Expert, bool>> exp = null, params string[] includes)
        {
            IQueryable<Expert> query = GetQuery(includes);
            return sectionId is null
                ? await query.ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).ToListAsync();
        }
        private IQueryable<Expert> GetQuery(string[] includes)
        {
            IQueryable<Expert> query = _dbContext.Set<Expert>();
            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    query = query.Include(item);  // Ensure the paths are valid
                }
            }
            return query;
        }

        public async Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return _dbContext.SubProfessions.ToList();
            }
            return _dbContext.SubProfessions.Where(e => e.SectionId == sectionId).ToList();
        }

        public async Task UpdateAsync(KonsEditViewModel entity)
        {
            var existingExpert = await _dbContext.Experts
                .Include(e => e.SubProfessions)
                .FirstOrDefaultAsync(e => e.Id == entity.Id);

            if (existingExpert != null)
            {
                existingExpert.Id = entity.Id;
                existingExpert.Name = entity.Name;
                existingExpert.Surname = entity.Surname;
                existingExpert.Fname = entity.Fname;
                existingExpert.SectionId = entity.SectionId;
                existingExpert.Profession = entity.Profession;
                existingExpert.SSN = entity.SSN;
                existingExpert.Rekvizit = entity.Rekvizit;
                existingExpert.BankFilial = entity.BankFilial;
                existingExpert.BankFilialCode = entity.BankFilialCode;
                existingExpert.BirthDate = entity.BirthDate;
                existingExpert.FinCode = entity.FinCode;
                existingExpert.Kons = true;
                existingExpert.Voen = entity.Voen;
                existingExpert.HesablashmaH = entity.HesablashmaH;

                // SubProfessions yeniləməsi
                if (entity.SelectedSubProfessions != null)
                {
                    foreach (var subProfessionId in entity.SelectedSubProfessions)
                    {
                        var subProfession = await _dbContext.SubProfessions.FindAsync(subProfessionId);
                        if (subProfession != null)
                        {
                            existingExpert.SubProfessions.Add(subProfession);
                        }
                    }
                }

                _dbContext.Experts.Update(existingExpert);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Expert> GetByIdAsync(int id)
        {
            return await _dbContext.Experts
                 .Include(e => e.Section)
                 .Include(e => e.SubProfessions)
                 .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Expert>> GetAllAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return await _dbContext.Experts
                .Include(e => e.Section)
                .Include(e => e.SubProfessions)
                .Where(e=>e.Kons==true)
                .ToListAsync();
            }
            return await _dbContext.Experts
                .Include(e => e.Section)
                .Include(e => e.SubProfessions)
                .Where(e=>e.SectionId == sectionId)
                .Where(e => e.Kons == true)
                .ToListAsync();
        }

        public async Task AddAsync(KonsViewModel entity)
        {
            var expert = new Expert
            {
                Name = entity.Name,
                Surname = entity.Surname,
                Fname = entity.Fname,
                SectionId = entity.SectionId,
                FinCode = entity.FinCode,
                BankFilial = entity.BankFilial,
                BankFilialCode = entity.BankFilialCode,
                BirthDate = entity.BirthDate,
                HesablashmaH = entity.HesablashmaH,
                Profession = entity.Profession,
                SSN = entity.SSN,
                Rekvizit = entity.Rekvizit,
                Voen = entity.Voen,
                Kons = true
            };

            // Link selected SubProfessions
            if (entity.SelectedSubProfessions != null)
            {
                foreach (var subProfessionId in entity.SelectedSubProfessions)
                {
                    var subProfession = await _dbContext.SubProfessions.FindAsync(subProfessionId);
                    if (subProfession != null)
                    {
                        expert.SubProfessions.Add(subProfession);
                    }
                }
            }

            await _dbContext.Experts.AddAsync(expert);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var expert = await _dbContext.Experts
               .Include(e => e.SubProfessions)
               .FirstOrDefaultAsync(e => e.Id == id);

            if (expert != null)
            {
                _dbContext.Experts.Remove(expert);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Section>> GetSectionsAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return  _dbContext.Sections.ToList();
            }
            return  _dbContext.Sections.Where(s => s.Id == sectionId).ToList();
        }
        public async Task BulkAddAsync(IEnumerable<Expert> experts)
        {
            await _dbContext.Set<Expert>().AddRangeAsync(experts);
            await _dbContext.SaveChangesAsync();
        }
    }
}
