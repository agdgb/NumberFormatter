using HumanNumbers.AspNetCore;
using HumanNumbers.AspNetCore.Serialization;
using System;

namespace HumanNumbers.Demo.Models
{
    public class ShowcaseModel
    {
        public string Title { get; set; } = "Portfolio Exposure";

        [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman, IsCurrency = true)]
        public decimal TotalValue { get; set; } = 1550000.50m;

        [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman, DecimalPlaces = 1)]
        public decimal YearlyGrowth { get; set; } = 0.1245m;

        // Demonstration of [NoHumanFormat]
        [NoHumanFormat]
        public long SystemInternalId { get; set; } = 9876543210;

        // Demonstration of a field without any attributes (uses default JSON converter)
        public int ActiveTransactions { get; set; } = 1250;

        // Used for Roman conversion demo
        public int FoundationYear { get; set; } = 2024;

        // Used for Byte conversion demo
        public long CloudStorageBytes { get; set; } = 5368709120; // 5 GB        
    }
}
