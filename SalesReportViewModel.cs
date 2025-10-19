using System;

namespace EventTicketSystem.Models
{
    public class SalesReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalSales { get; set; }
        public int BookingCount { get; set; }
        public string MonthName
        {
            get
            {
                return new DateTime(Year, Month, 1).ToString("MMMM");
            }
        }
    }
}