using NumberFormatter.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberFormatter.Demo.Models
{
    public class FinancialData
    {
        [ShortNumberFormat(isCurrency: true)]
        public decimal Revenue { get; set; }

        [ShortNumberFormat(isCurrency: true, currencyCode: "EUR")]
        public decimal EuroRevenue { get; set; }

        [ShortNumberFormat(decimalPlaces: 1)]
        public decimal GrowthRate { get; set; }

        // No attribute – will use the global converter (plain formatting)
        public long PageViews { get; set; }

        // Dictionary with currency codes as keys – we'll format manually in the controller
        public Dictionary<string, decimal> InternationalRevenue { get; set; } = new();
    }
}
