

// read the input CSV files

using Invest;

//var investments = ReadInvestmentsCSV("investments.csv");
//var transactions = ReadTransactionsCSV("transactions.csv");
//var quotes = ReadQuotesCSV("quotes.csv");

// Fake data for Investments
var investments = new List<Investment>
{
    new Investment { InvestorId = "A", InvestmentId = "A1", InvestmentType = "Stock", ISIN = "ABC", City = "New York", FondsInvestor = "D" },
    new Investment { InvestorId = "A", InvestmentId = "A2", InvestmentType = "RealEstate", ISIN = "DEF", City = "Los Angeles", FondsInvestor = null },
    new Investment { InvestorId = "B", InvestmentId = "B1", InvestmentType = "Funds", ISIN = "GHI", City = null, FondsInvestor = null },
    new Investment { InvestorId = "B", InvestmentId = "B2", InvestmentType = "Stock", ISIN = "JKL", City = "Chicago", FondsInvestor = "E" }
};

// Fake data for Transactions
var transactions = new List<Transaction>
{
    new Transaction { InvestmentId = "A1", Type = "Shares", Date = new DateTime(2022, 2, 1), Value = 100 },
    new Transaction { InvestmentId = "A1", Type = "Shares", Date = new DateTime(2022, 2, 2), Value = -50 },
    new Transaction { InvestmentId = "A2", Type = "Estate", Date = new DateTime(2022, 2, 1), Value = 1000000 },
    new Transaction { InvestmentId = "A2", Type = "Building", Date = new DateTime(2022, 2, 1), Value = 500000 },
    new Transaction { InvestmentId = "B1", Type = "Percentage", Date = new DateTime(2022, 2, 1), Value = 0.5M },
    new Transaction { InvestmentId = "B1", Type = "Percentage", Date = new DateTime(2022, 2, 2), Value = 0.2M },
    new Transaction { InvestmentId = "B2", Type = "Shares", Date = new DateTime(2022, 2, 1), Value = 200 }
};

// Fake data for Quotes
var quotes = new List<Quote>
{
    new Quote { ISIN = "ABC", Date = new DateTime(2022, 2, 1), PricePerShare = 10 },
    new Quote { ISIN = "ABC", Date = new DateTime(2022, 2, 2), PricePerShare = 11 },
    new Quote { ISIN = "JKL", Date = new DateTime(2022, 2, 1), PricePerShare = 5 }
};



// read input parameters from console
var line = Console.ReadLine();
while (!string.IsNullOrWhiteSpace(line))
{
    var input = line.Split(";");
    var date = DateTime.Parse(input[0]);
    var investorId = input[1];

    // calculate the value of the portfolio
    var portfolioValue = CalculatePortfolioValue(investorId, date, investments, transactions, quotes);

    // print the result
    Console.WriteLine($"Portfolio value for investor {investorId} on {date.ToShortDateString()}: {portfolioValue}");

    line = Console.ReadLine();
}


static List<Investment> ReadInvestmentsCSV(string filePath)
{
    var investments = new List<Investment>();

    using (var reader = new StreamReader(filePath))
    {
        reader.ReadLine(); // skip header row
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var fields = line.Split(",");
            var investment = new Investment
            {
                InvestorId = fields[0],
                InvestmentId = fields[1],
                InvestmentType = fields[2],
                ISIN = fields[3],
                City = fields[4],
                FondsInvestor = fields[5]
            };
            investments.Add(investment);
        }
    }

    return investments;
}

static List<Transaction> ReadTransactionsCSV(string filePath)
{
    var transactions = new List<Transaction>();

    using (var reader = new StreamReader(filePath))
    {
        reader.ReadLine(); // skip header row
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var fields = line.Split(",");
            var transaction = new Transaction
            {
                InvestmentId = fields[0],
                Type = fields[1],
                Date = DateTime.Parse(fields[2]),
                Value = decimal.Parse(fields[3])
            };
            transactions.Add(transaction);
        }
    }

    return transactions;
}

static List<Quote> ReadQuotesCSV(string filePath)
{
    var quotes = new List<Quote>();

    using (var reader = new StreamReader(filePath))
    {
        reader.ReadLine(); // skip header row
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var fields = line.Split(",");
            var quote = new Quote
            {
                ISIN = fields[0],
                Date = DateTime.Parse(fields[1]),
                PricePerShare = decimal.Parse(fields[2])
            };
            quotes.Add(quote);
        }
    }

    return quotes;
}
static decimal CalculatePortfolioValue(string investorId, DateTime cutoffDate, IEnumerable<Investment> investments, IEnumerable<Transaction> transactions, IEnumerable<Quote> quotes)
{
    decimal portfolioValue = 0;

    foreach (Investment inv in investments.Where(i => i.InvestorId == investorId))
    {
        if (inv.InvestmentType == "Stock")
        {
            Quote quote = quotes.FirstOrDefault(q => q.ISIN == inv.ISIN && q.Date == cutoffDate);
            if (quote != null)
            {
                decimal units = GetTotalUnits(transactions.Where(t => t.InvestmentId == inv.InvestmentId));
                decimal shareValue = units * quote.PricePerShare;
                portfolioValue += shareValue;
            }
        }
        else if (inv.InvestmentType == "RealEstate")
        {
            IEnumerable<Transaction> estateTransactions = transactions.Where(t => t.InvestmentId == inv.InvestmentId && t.Type == "Estate");
            decimal estateValue = estateTransactions.FirstOrDefault(t => t.Date <= cutoffDate)?.Value ?? 0;
            IEnumerable<Transaction> buildingTransactions = transactions.Where(t => t.InvestmentId == inv.InvestmentId && t.Type == "Building");
            decimal buildingValue = buildingTransactions.FirstOrDefault(t => t.Date <= cutoffDate)?.Value ?? 0;
            decimal propertyValue = estateValue + buildingValue;
            portfolioValue += propertyValue;
        }
        else if (inv.InvestmentType == "Funds")
        {
            decimal percentage = GetTotalUnits(transactions.Where(t => t.InvestmentId == inv.InvestmentId && t.Type == "Percentage"));
            IEnumerable<Investment> fundInvestments = investments.Where(i => i.FondsInvestor == inv.InvestmentId);
            decimal fundValue = 0;
            foreach (Investment fundInv in fundInvestments)
            {
                Quote quote = quotes.FirstOrDefault(q => q.ISIN == fundInv.ISIN && q.Date == cutoffDate);
                if (quote != null)
                {
                    decimal units = GetTotalUnits(transactions.Where(t => t.InvestmentId == fundInv.InvestmentId));
                    decimal shareValue = units * quote.PricePerShare;
                    fundValue += shareValue;
                }
            }
            decimal investmentValue = percentage * fundValue;
            portfolioValue += investmentValue;
        }
    }

    return portfolioValue;
}
static decimal GetTotalUnits(IEnumerable<Transaction> transactions)
{
    decimal totalUnits = 0;

    foreach (Transaction transaction in transactions)
    {
        decimal transactionUnits = 0;
        if (transaction.Type == "Shares")
        {
            transactionUnits = transaction.Value;
        }
        else if (transaction.Type == "Percentage")
        {
            transactionUnits = transaction.Value / 100; // Value is a percentage
        }
        else if (transaction.Type == "Estate" || transaction.Type == "Building")
        {
            // Do nothing, as property values are not measured in units
        }

        totalUnits += transactionUnits;
    }

    return totalUnits;
}
