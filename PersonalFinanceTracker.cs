using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace PersonalFinanceTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Personal Finance Tracker!");

            Console.Write("Enter your username: ");
            string? inputUsername = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(inputUsername))
            {
                Console.WriteLine("Username cannot be empty. Exiting.");
                return;
            }
            string username = inputUsername.Trim();
            string userFile = $"{username}_transactions.txt";

            FinanceTracker tracker = new FinanceTracker(userFile, username);
            tracker.Run();
        }
    }

    class FinanceTracker
    {
        private readonly List<Transaction> transactions;
        private readonly string saveFilePath;
        private readonly string username;

        public FinanceTracker(string filePath, string user)
        {
            transactions = new List<Transaction>();
            saveFilePath = filePath;
            username = user;
            LoadTransactions();
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1. Add Transaction");
                Console.WriteLine("2. View Transactions");
                Console.WriteLine("3. Export Transactions to CSV");
                Console.WriteLine("4. View Summary (Weekly or Monthly)");
                Console.WriteLine("5. Save and Exit");
                Console.Write("Enter your choice: ");

                string? choice = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(choice))
                {
                    Console.WriteLine("Invalid input.");
                    continue;
                }

                switch (choice)
                {
                    case "1":
                        AddTransaction();
                        break;
                    case "2":
                        DisplayTransactions();
                        break;
                    case "3":
                        ExportToCSV();
                        break;
                    case "4":
                        ViewSummary();
                        break;
                    case "5":
                        SaveTransactions();
                        Console.WriteLine("Exiting... Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private void AddTransaction()
        {
            Console.Write("Enter description: ");
            string? descInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(descInput))
            {
                Console.WriteLine("Description cannot be empty.");
                return;
            }
            string description = descInput.Trim();

            Console.Write("Enter amount: ");
            string? amountInput = Console.ReadLine();
            if (!decimal.TryParse(amountInput, out decimal amount))
            {
                Console.WriteLine("Invalid amount. Transaction not added.");
                return;
            }

            transactions.Add(new Transaction(description, amount, DateTime.Now));
            Console.WriteLine("Transaction added successfully.");
        }

        private void DisplayTransactions()
        {
            Console.WriteLine("\nTransaction History:");
            if (transactions.Count == 0)
            {
                Console.WriteLine("No transactions recorded.");
                return;
            }

            foreach (var transaction in transactions)
            {
                Console.WriteLine($"- {transaction.Date:yyyy-MM-dd}: {transaction.Description} - ${transaction.Amount:F2}");
            }
        }

        private void ExportToCSV()
        {
            string filePath = $"{username}_transactions.csv";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Date,Description,Amount");
                foreach (var t in transactions)
                {
                    writer.WriteLine($"{t.Date:yyyy-MM-dd},{t.Description},{t.Amount:F2}");
                }
            }
            Console.WriteLine($"Transactions exported to {filePath}");
        }

        private void ViewSummary()
        {
            Console.WriteLine("\nChoose Summary Type:");
            Console.WriteLine("1. Weekly Summary");
            Console.WriteLine("2. Monthly Summary");
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Invalid input.");
                return;
            }

            if (input == "1")
            {
                var weekly = new Dictionary<string, decimal>();
                foreach (var t in transactions)
                {
                    var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        t.Date,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday
                    );
                    string key = $"{t.Date.Year}-W{week}";
                    weekly[key] = weekly.TryGetValue(key, out var val) ? val + t.Amount : t.Amount;
                }

                Console.WriteLine("\nWeekly Summary:");
                foreach (var entry in weekly)
                {
                    Console.WriteLine($"- {entry.Key}: ${entry.Value:F2}");
                }
            }
            else if (input == "2")
            {
                var monthly = new Dictionary<string, decimal>();
                foreach (var t in transactions)
                {
                    string key = t.Date.ToString("yyyy-MM");
                    monthly[key] = monthly.TryGetValue(key, out var val) ? val + t.Amount : t.Amount;
                }

                Console.WriteLine("\nMonthly Summary:");
                foreach (var entry in monthly)
                {
                    Console.WriteLine($"- {entry.Key}: ${entry.Value:F2}");
                }
            }
            else
            {
                Console.WriteLine("Invalid option.");
            }
        }

        private void SaveTransactions()
        {
            using (StreamWriter writer = new StreamWriter(saveFilePath))
            {
                foreach (var t in transactions)
                {
                    writer.WriteLine($"{t.Description}|{t.Amount}|{t.Date:o}");
                }
            }
        }

        private void LoadTransactions()
        {
            if (File.Exists(saveFilePath))
            {
                var lines = File.ReadAllLines(saveFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 3 &&
                        decimal.TryParse(parts[1], out decimal amount) &&
                        DateTime.TryParse(parts[2], null, DateTimeStyles.RoundtripKind, out DateTime date))
                    {
                        transactions.Add(new Transaction(parts[0], amount, date));
                    }
                }
            }
        }
    }

    class Transaction
    {
        public string Description { get; }
        public decimal Amount { get; }
        public DateTime Date { get; }

        public Transaction(string description, decimal amount, DateTime date)
        {
            Description = description;
            Amount = amount;
            Date = date;
        }
    }
}
