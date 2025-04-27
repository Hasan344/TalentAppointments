namespace ForQab.DataAccess.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }

        public int? ExpertId { get; set; }
        public Expert? Expert { get; set; }

        public int? MonitorId { get; set; }
        public Monitor? Monitor { get; set; }
    }
}
