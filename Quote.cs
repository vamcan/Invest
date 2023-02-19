

namespace Invest
{
    public class Quote
    {
        public string ISIN { get; set; }
        public DateTime Date { get; set; }
        public decimal PricePerShare { get; set; }
    }
}
