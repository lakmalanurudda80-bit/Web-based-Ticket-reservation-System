using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventTicketSystem.Models
{
    public class EventSalesViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime EventDate { get; set; }
        public decimal AverageTicketPrice
        {
            get { return TotalTicketsSold > 0 ? TotalRevenue / TotalTicketsSold : 0; }
        }
    }

    public class TicketTypeSalesViewModel
    {
        public string TicketType { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class MonthlySalesViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string MonthName
        {
            get
            {
                return new DateTime(Year, Month, 1).ToString("MMM yyyy");
            }
        }
    }

    public class EventSalesDetailViewModel
    {
        public Event Event { get; set; }
        public List<TicketTypeSalesViewModel> TicketTypeSales { get; set; }
        public List<MonthlySalesViewModel> MonthlySales { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageTicketPrice
        {
            get { return TotalTicketsSold > 0 ? TotalRevenue / TotalTicketsSold : 0; }
        }
    }
}