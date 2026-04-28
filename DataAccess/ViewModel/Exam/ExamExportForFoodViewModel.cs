namespace ForQab.DataAccess.ViewModel.Exam
{
    public class ExamExportForFoodViewModel
    {
        public int Id { get; set; }
        public string ExamBuildingName { get; set; } = string.Empty;
        public DateOnly ExamDate { get; set; }
        public int? Food { get; set; }
        public int? Water { get; set; }
    }
}
