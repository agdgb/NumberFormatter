using HumanNumbers.AspNetCore;
using HumanNumbers.AspNetCore.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanNumbers.Demo.Models
{
    public class FinancialData
    {
        [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman, IsCurrency = true)]
        public decimal Revenue { get; set; }

        [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman, IsCurrency = true, CurrencyCode = "EUR")]
        public decimal EuroRevenue { get; set; }

        [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman, DecimalPlaces = 1)]
        public decimal GrowthRate { get; set; }

        // No attribute â€“ will use the global converter (plain formatting)
        public long PageViews { get; set; }

        // Dictionary with currency codes as keys â€“ we'll format manually in the controller
        public Dictionary<string, decimal> InternationalRevenue { get; set; } = new();
    }
}
