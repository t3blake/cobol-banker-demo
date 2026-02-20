using Microsoft.Data.Sqlite;
using CobolBanker.Models;

namespace CobolBanker.Data;

/// <summary>
/// Database connection, schema management, and seed data.
/// Uses SQLite with a single .db file next to the exe.
/// </summary>
public class Database : IDisposable
{
    private const int CURRENT_SCHEMA_VERSION = 1;
    private readonly SqliteConnection _conn;

    public Database(string? dbPath = null)
    {
        dbPath ??= Path.Combine(AppContext.BaseDirectory, "cobol-banker.db");
        _conn = new SqliteConnection($"Data Source={dbPath}");
        _conn.Open();
        EnsureSchema();
    }

    public void Dispose() => _conn.Dispose();

    // ── Schema ──────────────────────────────────────────────────────

    private void EnsureSchema()
    {
        using var cmd = _conn.CreateCommand();

        // Check if schema_version table exists
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='schema_version'";
        var exists = cmd.ExecuteScalar() != null;

        if (!exists)
        {
            CreateSchema();
            SeedData();
        }
        else
        {
            // Check version and run migrations if needed
            cmd.CommandText = "SELECT MAX(version) FROM schema_version";
            var version = Convert.ToInt32(cmd.ExecuteScalar());
            if (version < CURRENT_SCHEMA_VERSION)
                RunMigrations(version);
        }
    }

    private void CreateSchema()
    {
        var sql = @"
            CREATE TABLE schema_version (
                version       INTEGER NOT NULL,
                applied_date  TEXT NOT NULL
            );

            CREATE TABLE tellers (
                username      TEXT PRIMARY KEY,
                password      TEXT NOT NULL,
                display_name  TEXT NOT NULL,
                branch        TEXT NOT NULL
            );

            CREATE TABLE customers (
                customer_id   TEXT PRIMARY KEY,
                first_name    TEXT NOT NULL,
                last_name     TEXT NOT NULL,
                phone         TEXT NOT NULL,
                address       TEXT NOT NULL,
                created_date  TEXT NOT NULL
            );

            CREATE TABLE accounts (
                account_number TEXT PRIMARY KEY,
                customer_id   TEXT NOT NULL REFERENCES customers(customer_id),
                account_type  TEXT NOT NULL,
                balance       REAL NOT NULL,
                status        TEXT NOT NULL DEFAULT 'Active',
                opened_date   TEXT NOT NULL
            );

            CREATE TABLE transactions (
                transaction_id  INTEGER PRIMARY KEY AUTOINCREMENT,
                account_number  TEXT NOT NULL REFERENCES accounts(account_number),
                date            TEXT NOT NULL,
                description     TEXT NOT NULL,
                amount          REAL NOT NULL,
                running_balance REAL NOT NULL
            );

            CREATE TABLE account_notes (
                note_id         INTEGER PRIMARY KEY AUTOINCREMENT,
                account_number  TEXT NOT NULL REFERENCES accounts(account_number),
                created_by      TEXT NOT NULL,
                created_date    TEXT NOT NULL,
                note_text       TEXT NOT NULL
            );

            INSERT INTO schema_version (version, applied_date)
            VALUES (1, datetime('now'));
        ";

        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private void RunMigrations(int fromVersion)
    {
        // Future migrations go here
        // if (fromVersion < 2) { ... }
    }

    // ── Seed Data ───────────────────────────────────────────────────

    private void SeedData()
    {
        // Tellers
        InsertTeller("teller1", "pass123", "J. Smith", "Downtown");
        InsertTeller("teller2", "pass123", "M. Johnson", "Westside");
        InsertTeller("admin", "admin", "Admin User", "HQ");

        // Customers — obviously fake names, 555 numbers, fictional addresses
        InsertCustomer("CUST001", "John", "Smith", "555-0101", "123 Main St, Anytown, USA 00001", "2020-03-15");
        InsertCustomer("CUST002", "Jane", "Doe", "555-0102", "456 Oak Ave, Anytown, USA 00002", "2019-07-22");
        InsertCustomer("CUST003", "Robert", "Jones", "555-0103", "789 Pine Rd, Springfield, USA 00003", "2021-01-10");
        InsertCustomer("CUST004", "Mary", "Williams", "555-0104", "321 Elm St, Shelbyville, USA 00004", "2018-11-05");
        InsertCustomer("CUST005", "James", "Brown", "555-0105", "654 Maple Dr, Anytown, USA 00005", "2022-06-18");
        InsertCustomer("CUST006", "Patricia", "Davis", "555-0106", "987 Cedar Ln, Springfield, USA 00006", "2020-09-30");
        InsertCustomer("CUST007", "Michael", "Miller", "555-0107", "147 Birch Ct, Shelbyville, USA 00007", "2017-04-12");
        InsertCustomer("CUST008", "Linda", "Wilson", "555-0108", "258 Walnut Way, Anytown, USA 00008", "2023-02-28");

        // Accounts — each customer has 1-3 accounts with varied states
        // John Smith — regular customer, everything normal
        InsertAccount("1000100001", "CUST001", "Checking", 4523.67m, "Active", "2020-03-15");
        InsertAccount("1000100002", "CUST001", "Savings", 12750.00m, "Active", "2020-03-15");

        // Jane Doe — has a frozen savings account (good for Scenario 1)
        InsertAccount("1000200001", "CUST002", "Checking", 1893.45m, "Active", "2019-07-22");
        InsertAccount("1000200002", "CUST002", "Savings", 45200.00m, "Active", "2019-08-01");

        // Robert Jones — has a large balance, money market account
        InsertAccount("1000300001", "CUST003", "Checking", 8734.12m, "Active", "2021-01-10");
        InsertAccount("1000300002", "CUST003", "Savings", 25000.00m, "Active", "2021-01-10");
        InsertAccount("1000300003", "CUST003", "Money Market", 150000.00m, "Active", "2021-06-01");

        // Mary Williams — older account, some suspicious activity in transactions
        InsertAccount("1000400001", "CUST004", "Checking", 2341.89m, "Active", "2018-11-05");
        InsertAccount("1000400002", "CUST004", "Savings", 8900.00m, "Active", "2018-11-05");

        // James Brown — newer customer, checking only
        InsertAccount("1000500001", "CUST005", "Checking", 15678.34m, "Active", "2022-06-18");

        // Patricia Davis — has checking and savings
        InsertAccount("1000600001", "CUST006", "Checking", 3421.56m, "Active", "2020-09-30");
        InsertAccount("1000600002", "CUST006", "Savings", 67500.00m, "Active", "2020-10-15");

        // Michael Miller — long-time customer, has a closed account
        InsertAccount("1000700001", "CUST007", "Checking", 11234.78m, "Active", "2017-04-12");
        InsertAccount("1000700002", "CUST007", "Savings", 5600.00m, "Closed", "2017-04-12");

        // Linda Wilson — newest customer
        InsertAccount("1000800001", "CUST008", "Checking", 920.15m, "Active", "2023-02-28");
        InsertAccount("1000800002", "CUST008", "Savings", 3200.00m, "Active", "2023-03-01");

        // Transactions — seed realistic history
        // John Smith checking - normal activity
        SeedTransactions("1000100001", new[]
        {
            ("2026-02-01", "DIRECT DEPOSIT - EMPLOYER", 3200.00m),
            ("2026-02-03", "DEBIT CARD - GROCERY MART", -87.43m),
            ("2026-02-05", "ONLINE BILL PAY - ELECTRIC CO", -142.50m),
            ("2026-02-07", "ATM WITHDRAWAL", -200.00m),
            ("2026-02-10", "DEBIT CARD - GAS STATION", -45.67m),
            ("2026-02-12", "CHECK #1042", -500.00m),
            ("2026-02-14", "DEBIT CARD - RESTAURANT", -62.30m),
            ("2026-02-15", "DIRECT DEPOSIT - EMPLOYER", 3200.00m),
        }, 1761.57m); // starting balance before these transactions

        // Jane Doe checking - normal
        SeedTransactions("1000200001", new[]
        {
            ("2026-02-01", "DIRECT DEPOSIT - EMPLOYER", 2800.00m),
            ("2026-02-04", "DEBIT CARD - PHARMACY", -23.99m),
            ("2026-02-06", "ONLINE TRANSFER TO SAVINGS", -1000.00m),
            ("2026-02-10", "DEBIT CARD - COFFEE SHOP", -8.75m),
            ("2026-02-13", "ONLINE BILL PAY - INSURANCE", -425.00m),
            ("2026-02-15", "DIRECT DEPOSIT - EMPLOYER", 2800.00m),
        }, -2248.81m); // calculated to end at 1893.45

        // Jane Doe savings — some suspicious large deposits (for fraud demo)
        SeedTransactions("1000200002", new[]
        {
            ("2026-01-15", "TRANSFER FROM CHECKING", 500.00m),
            ("2026-01-22", "WIRE TRANSFER - UNKNOWN ORIGIN", 9800.00m),
            ("2026-01-28", "WIRE TRANSFER - UNKNOWN ORIGIN", 9500.00m),
            ("2026-02-02", "WIRE TRANSFER - UNKNOWN ORIGIN", 9700.00m),
            ("2026-02-06", "TRANSFER FROM CHECKING", 1000.00m),
            ("2026-02-11", "WIRE TRANSFER - UNKNOWN ORIGIN", 9900.00m),
            ("2026-02-18", "WIRE TRANSFER - OFFSHORE ACCT", 4800.00m),
        }, -0.00m); // ends at 45200.00

        // Mary Williams checking - suspicious pattern
        SeedTransactions("1000400001", new[]
        {
            ("2026-01-20", "DIRECT DEPOSIT - EMPLOYER", 1800.00m),
            ("2026-01-22", "CASH DEPOSIT", 4900.00m),
            ("2026-01-25", "WIRE TRANSFER OUT", -4850.00m),
            ("2026-01-28", "CASH DEPOSIT", 4800.00m),
            ("2026-01-30", "WIRE TRANSFER OUT", -4750.00m),
            ("2026-02-03", "DIRECT DEPOSIT - EMPLOYER", 1800.00m),
            ("2026-02-05", "CASH DEPOSIT", 4950.00m),
            ("2026-02-08", "WIRE TRANSFER OUT", -4900.00m),
            ("2026-02-12", "DEBIT CARD - GROCERY MART", -56.22m),
            ("2026-02-15", "ONLINE BILL PAY - RENT", -1200.00m),
        }, 748.11m);

        // Robert Jones checking - high earner, normal
        SeedTransactions("1000300001", new[]
        {
            ("2026-02-01", "DIRECT DEPOSIT - EMPLOYER", 8500.00m),
            ("2026-02-03", "ONLINE BILL PAY - MORTGAGE", -2450.00m),
            ("2026-02-05", "DEBIT CARD - COUNTRY CLUB", -350.00m),
            ("2026-02-08", "TRANSFER TO MONEY MARKET", -5000.00m),
            ("2026-02-10", "DEBIT CARD - RESTAURANT", -125.88m),
            ("2026-02-15", "DIRECT DEPOSIT - EMPLOYER", 8500.00m),
        }, -340.00m);

        // Some existing notes for demo purposes
        InsertNote("1000200002", "teller2", "2026-01-23", "Customer called - verified wire transfer from family overseas. No flag needed at this time.");
        InsertNote("1000700002", "teller1", "2025-12-01", "Account closed per customer request. Remaining balance transferred to checking.");
        InsertNote("1000400001", "teller2", "2026-02-10", "Unusual cash deposit pattern noted. Flagged for compliance review.");
    }

    private void SeedTransactions(string accountNumber, (string date, string desc, decimal amount)[] txns, decimal startingBalance)
    {
        var balance = startingBalance;
        foreach (var (date, desc, amount) in txns)
        {
            balance += amount;
            InsertTransaction(accountNumber, date, desc, amount, balance);
        }
    }

    // ── Insert helpers ──────────────────────────────────────────────

    private void InsertTeller(string username, string password, string displayName, string branch)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO tellers (username, password, display_name, branch) VALUES ($u, $p, $d, $b)";
        cmd.Parameters.AddWithValue("$u", username);
        cmd.Parameters.AddWithValue("$p", password);
        cmd.Parameters.AddWithValue("$d", displayName);
        cmd.Parameters.AddWithValue("$b", branch);
        cmd.ExecuteNonQuery();
    }

    private void InsertCustomer(string id, string first, string last, string phone, string address, string created)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO customers (customer_id, first_name, last_name, phone, address, created_date) VALUES ($id, $f, $l, $p, $a, $c)";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.Parameters.AddWithValue("$f", first);
        cmd.Parameters.AddWithValue("$l", last);
        cmd.Parameters.AddWithValue("$p", phone);
        cmd.Parameters.AddWithValue("$a", address);
        cmd.Parameters.AddWithValue("$c", created);
        cmd.ExecuteNonQuery();
    }

    private void InsertAccount(string accountNumber, string customerId, string type, decimal balance, string status, string opened)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO accounts (account_number, customer_id, account_type, balance, status, opened_date) VALUES ($an, $ci, $t, $b, $s, $o)";
        cmd.Parameters.AddWithValue("$an", accountNumber);
        cmd.Parameters.AddWithValue("$ci", customerId);
        cmd.Parameters.AddWithValue("$t", type);
        cmd.Parameters.AddWithValue("$b", (double)balance);
        cmd.Parameters.AddWithValue("$s", status);
        cmd.Parameters.AddWithValue("$o", opened);
        cmd.ExecuteNonQuery();
    }

    private void InsertTransaction(string accountNumber, string date, string desc, decimal amount, decimal runningBalance)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO transactions (account_number, date, description, amount, running_balance) VALUES ($an, $d, $desc, $a, $rb)";
        cmd.Parameters.AddWithValue("$an", accountNumber);
        cmd.Parameters.AddWithValue("$d", date);
        cmd.Parameters.AddWithValue("$desc", desc);
        cmd.Parameters.AddWithValue("$a", (double)amount);
        cmd.Parameters.AddWithValue("$rb", (double)runningBalance);
        cmd.ExecuteNonQuery();
    }

    private void InsertNote(string accountNumber, string createdBy, string date, string text)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO account_notes (account_number, created_by, created_date, note_text) VALUES ($an, $cb, $cd, $nt)";
        cmd.Parameters.AddWithValue("$an", accountNumber);
        cmd.Parameters.AddWithValue("$cb", createdBy);
        cmd.Parameters.AddWithValue("$cd", date);
        cmd.Parameters.AddWithValue("$nt", text);
        cmd.ExecuteNonQuery();
    }

    // ── Query Methods ───────────────────────────────────────────────

    public Teller? AuthenticateTeller(string username, string password)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT username, password, display_name, branch FROM tellers WHERE username = $u AND password = $p";
        cmd.Parameters.AddWithValue("$u", username);
        cmd.Parameters.AddWithValue("$p", password);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Teller
            {
                Username = reader.GetString(0),
                Password = reader.GetString(1),
                DisplayName = reader.GetString(2),
                Branch = reader.GetString(3)
            };
        }
        return null;
    }

    public List<Customer> SearchCustomers(string query)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = @"
            SELECT c.customer_id, c.first_name, c.last_name, c.phone, c.address, c.created_date
            FROM customers c
            LEFT JOIN accounts a ON c.customer_id = a.customer_id
            WHERE c.first_name LIKE $q
               OR c.last_name LIKE $q
               OR (c.first_name || ' ' || c.last_name) LIKE $q
               OR c.phone LIKE $q
               OR a.account_number LIKE $q
               OR c.customer_id LIKE $q
            GROUP BY c.customer_id
            ORDER BY c.last_name, c.first_name";
        cmd.Parameters.AddWithValue("$q", $"%{query}%");

        var customers = new List<Customer>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            customers.Add(ReadCustomer(reader));
        }
        return customers;
    }

    public Customer? GetCustomer(string customerId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT customer_id, first_name, last_name, phone, address, created_date FROM customers WHERE customer_id = $id";
        cmd.Parameters.AddWithValue("$id", customerId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? ReadCustomer(reader) : null;
    }

    public List<Account> GetAccountsForCustomer(string customerId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT account_number, customer_id, account_type, balance, status, opened_date FROM accounts WHERE customer_id = $ci ORDER BY account_type";
        cmd.Parameters.AddWithValue("$ci", customerId);

        var accounts = new List<Account>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            accounts.Add(ReadAccount(reader));
        }
        return accounts;
    }

    public Account? GetAccount(string accountNumber)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT account_number, customer_id, account_type, balance, status, opened_date FROM accounts WHERE account_number = $an";
        cmd.Parameters.AddWithValue("$an", accountNumber);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? ReadAccount(reader) : null;
    }

    public List<Transaction> GetTransactions(string accountNumber, int limit = 20)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT transaction_id, account_number, date, description, amount, running_balance FROM transactions WHERE account_number = $an ORDER BY date DESC, transaction_id DESC LIMIT $lim";
        cmd.Parameters.AddWithValue("$an", accountNumber);
        cmd.Parameters.AddWithValue("$lim", limit);

        var txns = new List<Transaction>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            txns.Add(new Transaction
            {
                TransactionId = reader.GetInt64(0),
                AccountNumber = reader.GetString(1),
                Date = reader.GetString(2),
                Description = reader.GetString(3),
                Amount = (decimal)reader.GetDouble(4),
                RunningBalance = (decimal)reader.GetDouble(5)
            });
        }
        return txns;
    }

    public List<AccountNote> GetNotes(string accountNumber)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT note_id, account_number, created_by, created_date, note_text FROM account_notes WHERE account_number = $an ORDER BY created_date DESC";
        cmd.Parameters.AddWithValue("$an", accountNumber);

        var notes = new List<AccountNote>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            notes.Add(new AccountNote
            {
                NoteId = reader.GetInt64(0),
                AccountNumber = reader.GetString(1),
                CreatedBy = reader.GetString(2),
                CreatedDate = reader.GetString(3),
                NoteText = reader.GetString(4)
            });
        }
        return notes;
    }

    // ── Update Methods ──────────────────────────────────────────────

    public void UpdateAccountStatus(string accountNumber, string newStatus)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "UPDATE accounts SET status = $s WHERE account_number = $an";
        cmd.Parameters.AddWithValue("$s", newStatus);
        cmd.Parameters.AddWithValue("$an", accountNumber);
        cmd.ExecuteNonQuery();
    }

    public void UpdateCustomerContact(string customerId, string phone, string address)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "UPDATE customers SET phone = $p, address = $a WHERE customer_id = $id";
        cmd.Parameters.AddWithValue("$p", phone);
        cmd.Parameters.AddWithValue("$a", address);
        cmd.Parameters.AddWithValue("$id", customerId);
        cmd.ExecuteNonQuery();
    }

    public void AddNote(string accountNumber, string createdBy, string noteText)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        InsertNote(accountNumber, createdBy, date, noteText);
    }

    public void TransferFunds(string sourceAcct, string destAcct, decimal amount, string tellerUsername)
    {
        using var transaction = _conn.BeginTransaction();
        try
        {
            // Debit source
            ExecuteNonQuery("UPDATE accounts SET balance = balance - $a WHERE account_number = $an",
                ("$a", (double)amount), ("$an", sourceAcct));

            // Credit destination
            ExecuteNonQuery("UPDATE accounts SET balance = balance + $a WHERE account_number = $an",
                ("$a", (double)amount), ("$an", destAcct));

            // Get new balances
            var srcBalance = GetAccount(sourceAcct)!.Balance;
            var dstBalance = GetAccount(destAcct)!.Balance;

            // Record transactions
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            InsertTransaction(sourceAcct, date, $"TRANSFER TO {destAcct}", -amount, srcBalance);
            InsertTransaction(destAcct, date, $"TRANSFER FROM {sourceAcct}", amount, dstBalance);

            // Add notes
            InsertNote(sourceAcct, tellerUsername, date, $"Transfer of ${amount:N2} to account {destAcct}");
            InsertNote(destAcct, tellerUsername, date, $"Transfer of ${amount:N2} from account {sourceAcct}");

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // ── Reset ───────────────────────────────────────────────────────

    public void ResetDatabase()
    {
        var tables = new[] { "account_notes", "transactions", "accounts", "customers", "tellers", "schema_version" };
        foreach (var table in tables)
        {
            ExecuteNonQuery($"DELETE FROM {table}");
        }
        ExecuteNonQuery("INSERT INTO schema_version (version, applied_date) VALUES (1, datetime('now'))");
        SeedData();
    }

    public int GetSchemaVersion()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(version) FROM schema_version";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    // ── Private helpers ─────────────────────────────────────────────

    private void ExecuteNonQuery(string sql, params (string name, object value)[] parameters)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);
        cmd.ExecuteNonQuery();
    }

    private static Customer ReadCustomer(SqliteDataReader reader)
    {
        return new Customer
        {
            CustomerId = reader.GetString(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            Phone = reader.GetString(3),
            Address = reader.GetString(4),
            CreatedDate = reader.GetString(5)
        };
    }

    private static Account ReadAccount(SqliteDataReader reader)
    {
        return new Account
        {
            AccountNumber = reader.GetString(0),
            CustomerId = reader.GetString(1),
            AccountType = reader.GetString(2),
            Balance = (decimal)reader.GetDouble(3),
            Status = reader.GetString(4),
            OpenedDate = reader.GetString(5)
        };
    }
}
