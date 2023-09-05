using System.Net;
using System.Net.Mail;
using System.Text;
using OpenPop.Mime;
using OpenPop.Pop3;

public class Email 
{
    public string To { get; set; }
    public string From { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public Email(string To, string From, string Subject, string Body) 
    {
        this.To = To;
        this.From = From;
        this.Subject = Subject;
        this.Body = Body;
    }

    public Email(Message message)
    {
        this.To = message.Headers.To[0].Address;
        this.From = message.Headers.From.Address;
        this.Subject = message.Headers.Subject;
        this.Timestamp = message.Headers.DateSent;
        try
        {
            this.Body = message.FindFirstPlainTextVersion().GetBodyAsText();
        }
        catch (Exception)
        {
            ConsoleHelper.Message($"Failed retrieving email sent from: {From} @ {Timestamp.ToString("dddd, MMMM dd, yyyy h:mm:ss tt")}", ConsoleColor.Red);
            this.Body = "";
        }
    }
    public override string ToString()
    {
        return $"{Timestamp.ToString("dddd, MMMM dd, yyyy h:mm:ss tt")}" +
        $"To: {To}\n" +
        $"From: {From}\n" +
        $"Subject: {Subject}\n" +
        $"---------------\n" +
        $"Body: {Body}";
    }
}

public class EmailHelper
{
    public static void SendEmail(string fromAddress, string fromPassword, string to, string subject, string content)
    {
        using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
        {
            smtpClient.EnableSsl = true;

            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(fromAddress, fromPassword);

            MailMessage message = new MailMessage(fromAddress, to, subject, content);

            smtpClient.Send(message);
        }
    }

    public static IEnumerable<Email> CheckEmail(string address, string password, int rangeStart = 0, int rangeAmount = 10) 
    {
        Pop3Client client = new Pop3Client();

        client.Connect("pop.gmail.com", 995, true);
        client.Authenticate("recent:" + address, password);

        int count = client.GetMessageCount();

        ConsoleHelper.Message($"Total Emails: {count}", ConsoleColor.DarkGreen);

        ConsoleHelper.Message($"Reading Messages {rangeStart + 1} - {int.Min(rangeAmount, count) + rangeStart}", ConsoleColor.DarkGreen);

        Email[] emails = new Email[int.Min(rangeAmount, count)];

        for (int i = 0; i < emails.Length; i++)
        {
            // ConsoleHelper.Message($"Reading message {i + 1}", ConsoleColor.Yellow);
            Message message = client.GetMessage((count - rangeStart) - i);

            Email email = new Email(message);

            emails[i] = email;
        }

        return emails;
    }
}