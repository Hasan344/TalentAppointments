using ForQab.DataAccess.Models;
using System;
using System.Collections.Generic;

namespace ForQab.Models.ViewModels
{
    public class ExamDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SectionViewModel Section { get; set; } = new();
        public BuildingViewModel ExamBulding { get; set; } = new();
        public DateOnly ExamDate { get; set; }
        public decimal Duration { get; set; }
        public double Water { get; set; }
        public int Food { get; set; }
        public int? StudentCount { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public TimeSpan? AdmissionTime { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string InventoryTransport { get; set; } = string.Empty;
        public List<ExamCommissionViewModel> ExamCommissions { get; set; } = new();
        public List<ExpertViewModelForExam> Experts { get; set; } = new();
        public List<MonitorViewModel> Monitors { get; set; } = new();
        public List<int> ExpertsWithLogs { get; set; } = new();
        public List<Profession> Federations { get; set; } = new();
    }

    public class SectionViewModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public class BuildingViewModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public class ExamCommissionViewModel
    {
        public CommissionViewModel Commission { get; set; } = new();
    }

    public class CommissionViewModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public class MonitorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Fname { get; set; } = string.Empty;
        public string FinCode { get; set; } = string.Empty;
        public byte? Role { get; set; }
    }
    public class ExpertViewModelForExam
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Fname { get; set; } = string.Empty;
        public string FinCode { get; set; } = string.Empty;
        public List<SubProfessionViewModelForExam> ExamExpertSubProfessions { get; set; } = new();
    }

    public class SubProfessionViewModelForExam
    {
        public string Name { get; set; } = string.Empty;
        public string FederationName { get; set; } = string.Empty;
    }
}
