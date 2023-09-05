using System.Text.RegularExpressions;

AccountHelper db = new AccountHelper("Server=127.0.0.1,1433;Password=Password1!;User Id=SA;TrustServerCertificate=true;Initial Catalog=EmailConsoleApp");

Account? user = null;

int choice = 0;

bool exit = false;

bool error = false;

ConsoleHelper.Message("Welcome to Dante's Email Client", ConsoleColor.Blue);

do
{
    if (user is null)
    {
        choice = ConsoleHelper.GetOptionFromList(new string[] {"Log in", "Sign up", "Exit"});

        if(choice == 1)
        {
            string email = ConsoleHelper.GetResponse("Please enter your email:");

            string password = ConsoleHelper.GetResponse("Please enter your password:");

            string key = EncryptionHelper.GenerateKeyFromPassword(password);

            user = db.SignIn(email, key);

            if(user is null) ConsoleHelper.Message("Sign in failed", ConsoleColor.Red);

            else ConsoleHelper.Message("Sign in was successful", ConsoleColor.Green);
        }

        if(choice == 2)
        {
            string email;
            do
            {
                email = ConsoleHelper.GetResponse("Please provide an email (that is 2fa enabled):");

                error = db.DoesEmailExist(email);

                if(error) ConsoleHelper.Message("This email already exists", ConsoleColor.Red);

            } while (error);

            string password = ConsoleHelper.GetResponse("Please provide the 2fa password:");

            string realPW = ConsoleHelper.GetResponse("To secure this password, provide an additional passphrase:");

            string key = EncryptionHelper.GenerateKeyFromPassword(realPW);

            string encryptedPW = EncryptionHelper.AESEncrypt(password, key);

            string publicKeyFileName = ConsoleHelper.GetResponse("Please provide the filepath to your public key:");

            string publicKey = File.ReadAllText(publicKeyFileName).Trim();

            Account account = new Account(email, key, encryptedPW, publicKey);

            error = !db.AddAccount(account);

            if(error) ConsoleHelper.Message("There was an issue creating your account", ConsoleColor.DarkYellow);

            else ConsoleHelper.Message("Account created successfully", ConsoleColor.Green);
        }

        if (choice == 3) exit = true;
    }
    else
    {
        choice = ConsoleHelper.GetOptionFromList(new string[] { "Read Emails", "Send Email", "Exit" });

        if(choice == 1)
        {
            List<Email> emails = EmailHelper.CheckEmail(user.Email, EncryptionHelper.AESDecrypt(user.EmailPassword, user.Password), 0, 25).ToList();

            List<string> emailHeadLines = emails.Select(x => x.Subject + " | " + x.From + " | " + x.Timestamp.ToString("dddd, MMMM dd, yyyy h:mm:ss tt")).ToList();

            int emailChoice = ConsoleHelper.GetOptionFromList(emailHeadLines.ToArray(), "Please select an email to read:");

            Email selectedEmail = emails[emailChoice - 1];

            bool signature = selectedEmail.Subject[0] == '+';

            bool encrypted = selectedEmail.Subject[0] == '*' || selectedEmail.Subject[1] == '*';

            string actualContent = selectedEmail.Body;

            Regex BodyWithSignatureCapturing = new Regex(@"(.*)(?:\nSIGNATURE: ?)(?:[^\s]*)", RegexOptions.Singleline);
            Regex SignatureCapturing = new Regex(@"(?:SIGNATURE: ?)([^\s]*)", RegexOptions.Singleline);
            Regex EncryptedBodyCapturing = new Regex(@"^(?:BODY: ?)([^\s]*)", RegexOptions.Singleline);

            if(encrypted) 
            {
                string privateKeyFileName = ConsoleHelper.GetResponse("Please provide the file path for your private key:");

                string base64encryptedContent = EncryptedBodyCapturing.Match(selectedEmail.Body).Groups[1].Value;

                actualContent = EncryptionHelper.RSADecryptFromBase64(base64encryptedContent, privateKeyFileName);
            }

            if(signature)
            {
                string base64Signature = SignatureCapturing.Match(selectedEmail.Body).Groups[1].Value;

                if(!encrypted)
                {
                    actualContent = BodyWithSignatureCapturing.Match(selectedEmail.Body).Groups[1].Value;
                }

                string publicKey = db.GetPublicKeyFromEmail(selectedEmail.From);

                bool verified = EncryptionHelper.VerifyData(actualContent, publicKey, base64Signature);

                ConsoleHelper.Message(verified ? "Message integrity was verified" : "Message integrity was not verified", verified ? ConsoleColor.DarkMagenta : ConsoleColor.Red);
            }

            ConsoleHelper.Message("Message:", ConsoleColor.DarkCyan);

            ConsoleHelper.Message(actualContent, ConsoleColor.Yellow);
        }

        if(choice == 2)
        {
            bool encrypt = ConsoleHelper.GetYesNo("Do you want to encrypt this message?:");

            bool sign = ConsoleHelper.GetYesNo("Do you want to sign this message?:");

            string email;

            string recipientPublicKey = "";

            do
            {
                email = ConsoleHelper.GetResponse("What email would you like to send a message to?:");

                // Since we only care about whether or not a public key is accessible, we check to see if
                // we have the email in our db.
                error = !db.DoesEmailExist(email);

                if(error && encrypt) ConsoleHelper.Message("That email does not have a public key", ConsoleColor.DarkYellow);
                else if(encrypt) recipientPublicKey = db.GetPublicKeyFromEmail(email);
            } while (error && encrypt);

            string subject = ConsoleHelper.GetResponse("What would you like the subject line to say?:");

            string content = ConsoleHelper.GetResponse("What would you like to say?:");

            string signature = "";

            string encryptedContent = "";

            if(sign)
            {
                string privateKeyFileName = ConsoleHelper.GetResponse("Please provide the file path for your private key:");
                signature = EncryptionHelper.SignData(content, privateKeyFileName);
            }

            if(encrypt)
            {
                encryptedContent = EncryptionHelper.RSAEncryptToBase64(content, recipientPublicKey);
            }

            subject = (sign ? "+" : "") + (encrypt ? "*" : "") + subject;

            content = (encrypt ? $"BODY: {encryptedContent}" : content) + (sign ? $"\nSIGNATURE: {signature}" : "");

            EmailHelper.SendEmail(user.Email, EncryptionHelper.AESDecrypt(user.EmailPassword, user.Password), email, subject, content);
        }

        if(choice == 3) exit = true;
    }
} while (!exit);

ConsoleHelper.Message("Farewell", ConsoleColor.Blue);