using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Expert;
using ForQab.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository.Concrete
{
    public class KonsRepository : IKonsRepository
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
                    query = query.Include(item);
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
                .Include(e => e.ExpertsProfessions)
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
                existingExpert.Serial = entity.Serial;
                existingExpert.Kons = true;
                existingExpert.Voen = entity.Voen;
                existingExpert.HesablashmaH = entity.HesablashmaH;
                existingExpert.TelIs = entity.TelIs;
                existingExpert.TelEl = entity.TelEl;

                existingExpert.ExpertsProfessions.Clear();

                // Yeni ilişkileri ekle
                if (entity.SelectedSubProfessions != null)
                {
                    foreach (var subProfessionId in entity.SelectedSubProfessions)
                    {
                        existingExpert.ExpertsProfessions.Add(new ExpertsProfession
                        {
                            ExpertId = entity.Id,
                            SubProfessionId = subProfessionId
                        });
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
                .Include(e => e.ExpertsProfessions)
                    .ThenInclude(e => e.SubProfession)
                .Include(e => e.DistrictNavigation)
                .Include(e => e.GenderNavigation)
                .Include(e => e.FederationNavigation)
                .Include(e => e.ExamExpertSubProfessions)
                    .ThenInclude(e => e.Exam)
                .Include(e => e.ExamExpertSubProfessions)
                    .ThenInclude(e => e.SubProfession)
                .Include(e => e.ExamExpertSubProfessions)
                    .ThenInclude(e => e.ExamRoom)
                .Include(e => e.ExpertLogs)
                .Include(e => e.Contracts)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Expert>> GetAllAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return await _dbContext.Experts
                .Include(e => e.Section)
                .Include(e => e.ExpertsProfessions)
                    .ThenInclude(e => e.SubProfession)
                .Include(e => e.DistrictNavigation)
                .Include(e => e.GenderNavigation)
                .Include(e => e.Contracts)
                .Include(e => e.ExamExpertSubProfessions)
                .Where(e => e.Kons == true)
                .ToListAsync();
            }
            return await _dbContext.Experts
                .Include(e => e.Section)
                .Include(e => e.ExpertsProfessions)
                    .ThenInclude(e => e.SubProfession)
                .Include(e => e.DistrictNavigation)
                .Include(e => e.GenderNavigation)
                .Include(e => e.Contracts)
                .Include(e => e.ExamExpertSubProfessions)
                .Where(e => e.SectionId == sectionId)
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
                Kons = true,
                TelIs = entity.TelIs,
                TelEl = entity.TelEl,
                Status = (byte?)entity.Status
            };

            // Link selected SubProfessions
            if (entity.SelectedSubProfessions != null)
            {
                foreach (var subProfessionId in entity.SelectedSubProfessions)
                {
                    var subProfession = await _dbContext.SubProfessions.FindAsync(subProfessionId);
                    if (subProfession != null)
                    {
                        expert.ExpertsProfessions.Add(new Models.ExpertsProfession 
                        {
                            SubProfession = subProfession,
                            Expert = expert
                        });
                    }
                }
            }

            await _dbContext.Experts.AddAsync(expert);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var expert = await _dbContext.Experts
               .Include(e => e.ExpertsProfessions)
                .Include(e => e.DistrictNavigation)
                .Include(e => e.GenderNavigation)
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
                return _dbContext.Sections.ToList();
            }
            return _dbContext.Sections.Where(s => s.Id == sectionId).ToList();
        }
        public async Task BulkAddAsync(IEnumerable<Expert> experts)
        {
            await _dbContext.Set<Expert>().AddRangeAsync(experts);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Expert entity)
        {
            _dbContext.Experts.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Expert>> GetKonsLogsAsync(int? sectionId)
        {
            var query = await _dbContext.Experts
                        .Include(xl => xl.ExpertLogs)
                        .Where(xl => xl.ExpertLogs.Any() && xl.Kons == true)
                        .ToListAsync();
            if (sectionId != null)
            {
                query = query.Where(xl => xl.SectionId == sectionId).ToList();
            }

            return query;
        }
    }
}