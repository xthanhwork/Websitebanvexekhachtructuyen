namespace DOANCOSO26.Models
{
    public class RevenueReportViewModel
    {
        public string? Company { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double TotalRevenue { get; set; }
        public int TotalTickets { get; set; }
        public List<RevenueReportRow> Rows { get; set; } = new();
    }

    public class RevenueReportRow
    {
        public string Company { get; set; } = string.Empty;
        public int BusTripId { get; set; }
        public string TripName { get; set; } = string.Empty;
        public string BusNumber { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public DateTime DepartureDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public int TicketCount { get; set; }
        public double Revenue { get; set; }
    }
}
