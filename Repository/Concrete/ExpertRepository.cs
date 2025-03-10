using DocumentFormat.OpenXml.Office2016.Excel;
using ForQab.Data_Access.ViewModel;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository.Concrete
{
    public class ExpertRepository : IExpertRepository
    {
        private readonly MyDbContext _context;

        public ExpertRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Expert>> GetAllAsync()
        {
            return await _context.Experts
                .Include(e => e.Section)
                .Include(e => e.SubProfessions)
                .Include(e => e.DistrictNavigation)
                .Include(e => e.GenderNavigation)
                .Include(e => e.FederationNavigation)
                .ToListAsync();
        }

        public async Task<Expert?> GetByIdAsync(int id)
        {
            return await _context.Experts
                .Include(e => e.Section)
                .Include(e => e.SubProfessions)
                .Include(e => e.DistrictNavigation)
                .Include(e => e.GenderNavigation)
                .Include(e => e.FederationNavigation)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(ExpertViewModel expertViewModel)
        {
            var expert = new Expert
            {
                Name = expertViewModel.Name,
                Surname = expertViewModel.Surname,
                Fname = expertViewModel.Fname,
                SectionId = expertViewModel.SectionId,
                FinCode = expertViewModel.FinCode,
                BankFilial = expertViewModel.BankFilial,
                BankFilialCode = expertViewModel.BankFilialCode,
                BirthDate = expertViewModel.BirthDate,
                HesablashmaH = expertViewModel.HesablashmaH,
                Profession = expertViewModel.Profession,
                SSN = expertViewModel.SSN,
                Rekvizit = expertViewModel.Rekvizit,
                Voen = expertViewModel.Voen,
                Gender = expertViewModel.Gender,
                Federation = expertViewModel.Federation,
                TelIs = expertViewModel.TelIs,
                TelEl = expertViewModel.TelEl,
                Kons = false
            };

            // Link selected SubProfessions
            if (expertViewModel.SelectedSubProfessions != null)
            {
                foreach (var subProfessionId in expertViewModel.SelectedSubProfessions)
                {
                    var subProfession = await _context.SubProfessions.FindAsync(subProfessionId);
                    if (subProfession != null)
                    {
                        expert.SubProfessions.Add(subProfession);
                    }
                }
            }

            await _context.Experts.AddAsync(expert);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExpertEditViewModel expert)
        {
            var existingExpert = await _context.Experts
                .Include(e => e.SubProfessions)
                .Include(e => e.DistrictNavigation)
                .Include(e => e.GenderNavigation)
                .Include(e => e.FederationNavigation)
                .FirstOrDefaultAsync(e => e.Id == expert.Id);

            if (existingExpert != null)
            {
                existingExpert.Id = expert.Id;
                existingExpert.Name = expert.Name;
                existingExpert.Surname = expert.Surname;
                existingExpert.Fname = expert.Fname;
                existingExpert.SectionId = expert.SectionId;
                existingExpert.Profession = expert.Profession;
                existingExpert.SSN = expert.SSN;
                existingExpert.Rekvizit = expert.Rekvizit;
                existingExpert.BankFilial = expert.BankFilial;
                existingExpert.BankFilialCode = expert.BankFilialCode;
                existingExpert.BirthDate = expert.BirthDate;
                existingExpert.FinCode = expert.FinCode;
                existingExpert.Kons = false;
                existingExpert.Voen = expert.Voen;
                existingExpert.Gender = expert.Gender;
                existingExpert.Federation = expert.Federation;
                existingExpert.HesablashmaH = expert.HesablashmaH;
                existingExpert.TelIs = expert.TelIs;
                existingExpert.TelEl = expert.TelEl;

                // SubProfessions yeniləməsi
                if (existingExpert.SubProfessions != null)
                {
                    existingExpert.SubProfessions.Clear();
                }
                if (expert.SelectedSubProfessions != null)
                {
                    foreach (var subProfessionId in expert.SelectedSubProfessions)
                    {
                        var subProfession = await _context.SubProfessions.FindAsync(subProfessionId);
                        if (subProfession != null)
                        {
                            existingExpert.SubProfessions.Add(subProfession);
                        }
                    }
                }

                _context.Experts.Update(existingExpert);
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateExpertAsync(Expert expert)
        {
            _context.Experts.Update(expert);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var expert = await _context.Experts
                .Include(e => e.SubProfessions)
                .Include(e => e.DistrictNavigation)
                .Include(e => e.GenderNavigation)
                .Include(e => e.FederationNavigation)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expert != null)
            {
                _context.Experts.Remove(expert);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SubProfession>> GetSubProfessionsBySectionIdAsync(int? sectionId)
        {
            return await _context.SubProfessions
                                   .Where(e => e.SectionId == sectionId)
                                   .ToListAsync();
        }

        public async Task AddSubProfessionToExpertAsync(int expertId, SubProfession subProfession)
        {
            var expert = await _context.Experts
                .Include(e => e.SubProfessions)
                .FirstOrDefaultAsync(e => e.Id == expertId);

            if (expert != null)
            {
                expert.SubProfessions.Add(subProfession);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveSubProfessionFromExpertAsync(int expertId, int subProfessionId)
        {
            var expert = await _context.Experts
                .Include(e => e.SubProfessions)
                .FirstOrDefaultAsync(e => e.Id == expertId);

            if (expert != null)
            {
                var subProfession = expert.SubProfessions.FirstOrDefault(sp => sp.Id == subProfessionId);
                if (subProfession != null)
                {
                    expert.SubProfessions.Remove(subProfession);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<IEnumerable<Expert>> SearchByNameAsync(string name)
        {
            return await _context.Experts
                .Where(e => EF.Functions.Like(e.Name, $"%{name}%") ||
                            EF.Functions.Like(e.Surname, $"%{name}%") ||
                            EF.Functions.Like(e.Fname, $"%{name}%"))
                .Include(e => e.Section)
                .Include(e => e.SubProfessions)
                .ToListAsync();
        }

        public async Task<List<Section>> GetSectionsAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return _context.Sections.ToList();
            }
            return _context.Sections.Where(s => s.Id == sectionId).ToList();
        }
        public async Task<List<Profession>> GetFederationsAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return _context.Professions.ToList();
            }
            return _context.Professions.Where(s => s.SectionId == sectionId).ToList();
        }

        public async Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return _context.SubProfessions.ToList();
            }
            return _context.SubProfessions.Where(e => e.SectionId == sectionId).ToList();
        }

        public async Task<IEnumerable<Expert>> GetExpertsBySectionIdAsync(int? sectionId)
        {
            return await _context.Experts
                                   .Where(e => e.SectionId == sectionId)
                                   .ToListAsync();
        }

        public async Task<List<Expert>> GetAllAsync(int? sectionId, Expression<Func<Expert, bool>> exp = null, params string[] includes)
        {
            IQueryable<Expert> query = GetQuery(includes);
            return sectionId is null
                ? await query.Where(e => e.Kons == false).Where(e => e.Archive == 0).ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).Where(e => e.Kons == false).Where(e => e.Archive == 0).ToListAsync();
        }
        public async Task<List<Expert>> GetAllArchivedAsync(int? sectionId, Expression<Func<Expert, bool>> exp = null, params string[] includes)
        {
            IQueryable<Expert> query = GetQuery(includes);
            return sectionId is null
                ? await query.Where(e => e.Kons == false).Where(e => e.Archive == 1).ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).Where(e => e.Kons == false).Where(e => e.Archive == 1).ToListAsync();
        }
        private IQueryable<Expert> GetQuery(string[] includes)
        {
            IQueryable<Expert> query = _context.Set<Expert>();
            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    query = query.Include(item);  // Ensure the paths are valid
                }
            }
            return query;
        }
        public async Task BulkAddAsync(IEnumerable<Expert> experts)
        {
            await _context.Set<Expert>().AddRangeAsync(experts);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Expert>> GetExpertLogsAsync()
        {
            return await _context.Experts
                        .Include(xl => xl.ExpertLogs)
                        .Where(xl => xl.ExpertLogs.Any())
                        .ToListAsync();
        }

        public async Task<IEnumerable<Expert>> GetExpertLogsByExpertIdAsync(int expertId)
        {
            return await _context.Experts
                        .Include(xl => xl.ExpertLogs)
                        .Where(xl => xl.Id == expertId)
                        .ToListAsync();
        }

        public async Task DeleteExpertLogs(int? id)
        {
            var log = await _context.ExpertLogs.FindAsync(id);
            if (log == null)
            {
                throw new InvalidOperationException("Log yoxdur");
            }
            else
                _context.ExpertLogs.Remove(log);
            _context.SaveChanges();
        }
        public async Task<List<Expert>> GetExpertsBySectionAndSubProfessionAsync(int sectionId, int subProfessionId, List<int> excludedExpertIds)
        {
            return await _context.Experts
                .Where(e => e.SectionId == sectionId)
                .Where(e => e.SubProfessions.Any(sp => sp.Id == subProfessionId)) // Match the correct SubProfession
                .Where(e => !excludedExpertIds.Contains(e.Id)) // Exclude already selected experts
                .ToListAsync();
        }
    }
}
