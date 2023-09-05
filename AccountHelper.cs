using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.SqlClient;

public class Account
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string EmailPassword { get; set; }
    public string PublicKey { get; set; }
    public Account(int Id, string Email, string Password, string EmailPassword, string PublicKey)
    {
        this.Id = Id;
        this.Email = Email;
        this.Password = Password;
        this.EmailPassword = EmailPassword;
        this.PublicKey = PublicKey;
    }
    public Account(string Email, string Password, string EmailPassword, string PublicKey) 
    {
        this.Email = Email;
        this.Password = Password;
        this.EmailPassword = EmailPassword;
        this.PublicKey = PublicKey;
    }
}
public class AccountHelper
{
    private string ConnectionString { get; set; }

    private SqlConnection db { get; set; }

    public AccountHelper(string ConnectionString) 
    {
        this.ConnectionString = ConnectionString;

        db = new SqlConnection(ConnectionString);
    }

    public bool DoesEmailExist(string email) 
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("SELECT Email FROM Accounts WHERE ");
        sb.Append($"Email LIKE '{email}';");

        db.Open();

        SqlCommand command = new SqlCommand(sb.ToString(), db);

        Object exists = command.ExecuteScalar();

        db.Close();

        if(exists is null) return false;
        return true;
    }

    public bool AddAccount(Account account) 
    {
        if(DoesEmailExist(account.Email)) return false;

        StringBuilder sb = new StringBuilder();

        sb.Append("INSERT INTO Accounts (Email, Password, EmailPassword, PublicKey) VALUES");
        sb.Append($"('{account.Email}','{account.Password}','{account.EmailPassword}', '{account.PublicKey}');");

        db.Open();

        SqlCommand command = new SqlCommand(sb.ToString(), db);

        int rows = command.ExecuteNonQuery();

        db.Close();

        return rows == 1;
    }

    public Account? SignIn(string email, string password) 
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("SELECT Id, Email, Password, EmailPassword, PublicKey FROM Accounts WHERE ");
        sb.Append($"Email LIKE '{email}' AND Password LIKE '{password}';");

        db.Open();

        SqlCommand command = new SqlCommand(sb.ToString(), db);

        SqlDataReader reader = command.ExecuteReader();

        Account? account = null;

        if(reader.Read())
        {
            int accId = (int)reader[0];
            string accEmail = (string)reader[1];
            string accPassword = (string)reader[2];
            string accEmailPassword = (string)reader[3];
            string accPublicKey = (string)reader[4];

            account = new Account(accId, accEmail, accPassword, accEmailPassword, accPublicKey);
        }
        
        db.Close();

        return account;
    }

    public string GetPublicKeyFromEmail(string email) 
    {
        if(!DoesEmailExist(email)) return "";

        StringBuilder sb = new StringBuilder();

        sb.Append("SELECT PublicKey FROM Accounts WHERE ");
        sb.Append($"Email LIKE '{email}';");

        db.Open();

        SqlCommand command = new SqlCommand(sb.ToString(), db);

        string publicKey = command.ExecuteScalar().ToString() ?? "";

        db.Close();

        return publicKey;
    }
}