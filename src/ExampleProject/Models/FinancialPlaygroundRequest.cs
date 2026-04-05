using NumberFormatter.AspNetCore.Financial;

namespace NumberFormatter.Demo.Models;

public class FinancialPlaygroundRequest
{
    [BasisPoints] 
    public decimal? Spread { get; set; }

    [FractionPrice(32)] 
    public decimal? TreasuryPrice { get; set; }

    public decimal? RawAmount { get; set; }
}
