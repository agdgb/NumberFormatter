using HumanNumbers.AspNetCore;
using HumanNumbers.AspNetCore.Financial;

namespace HumanNumbers.Demo.Models;

public class FinancialPlaygroundRequest
{
    [BasisPoints] 
    public decimal? Spread { get; set; }

    [FractionPrice(32)] 
    public decimal? TreasuryPrice { get; set; }

    [NoHumanFormat]
    public decimal? RawAmount { get; set; }
}
