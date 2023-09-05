using System.Text;
using Azure;

public class ConsoleHelper 
{
    /// <summary>
    /// Sends a message to the console with a specific color
    /// </summary>
    /// <param name="message">The message to print</param>
    /// <param name="color">The color to print the message in</param>
    public static void Message(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    /// <summary>
    /// Gets a user's input from the console
    /// </summary>
    /// <param name="prompt">The prompt you want to print into console</param>
    /// <param name="skippable">An optional value that will allow a user to input a blank string if true</param>
    /// <returns>A string response, that could be blank</returns>
    public static string GetResponse(string prompt, bool skippable = false) 
    {
        string response = "";

        bool invalid = true;

        do
        {
            Message(prompt, ConsoleColor.DarkCyan);

            response = Console.ReadLine() ?? "";

            if (!skippable && string.IsNullOrEmpty(response))
            {
                Message("Please do not leave this blank", ConsoleColor.DarkYellow);

                invalid = true;
            }
            else 
            {
                response = response.Trim();

                invalid = false;
            }

        } while (invalid);

        return response;
    }

    /// <summary>
    /// Gets a user's input and turns it into an integer
    /// </summary>
    /// <param name="prompt">The prompt you want to print into the console</param>
    /// <param name="min">An optional value to set a minimum range (inclusive)</param>
    /// <param name="max">An optional value to set a maximum range (inclusive)</param>
    /// <param name="skippable">An optional value that will allow a user to input a blank string if true</param>
    /// <returns>A number that will be between any range set, and possibly null if skippable is true</returns>
    public static int? GetIntegerReponse(string prompt, int min = int.MinValue, int max = int.MaxValue, bool skippable = false) 
    {
        string rawResponse = "";

        int response = 0;

        bool invalid = true;

        do
        {
            rawResponse = GetResponse(prompt, skippable);

            if(skippable && string.IsNullOrEmpty(rawResponse)) 
            {
                return null;
            }
            else if(int.TryParse(rawResponse, out response)) 
            {
                if(response < min) 
                {
                    Message($"Please enter a number larger than or equal to {min}", ConsoleColor.DarkYellow);

                    invalid = true;
                }
                else if(response > max)
                {
                    Message($"Please enter a number smaller than or equal to {max}", ConsoleColor.DarkYellow);

                    invalid = true;
                }
                else
                {
                    invalid = false;
                }
            }
            else 
            {
                Message("Please enter a valid number", ConsoleColor.DarkYellow);

                invalid = true;
            }
        } while (invalid);

        return response;
    }

    /// <summary>
    /// Gets a user's selection from a list of choices
    /// </summary>
    /// <param name="options">The list of options to choose from</param>
    /// <returns>A number corresponding with the choice made by a user, starting at 1 | -1 means an issue has occured</returns>
    public static int GetOptionFromList(string[] options, string prompt = "Select an option below:")
    {
        int response = 0;

        StringBuilder sb = new StringBuilder();

        sb.Append(prompt);

        for (int i = 0; i < options.Length; i++)
        {
            sb.Append($"\n{i + 1}. {options[i]}");
        }

        int min = 1;

        int max = options.Length;

        response = GetIntegerReponse(sb.ToString(), min, max) ?? -1;

        return response;
    }

    /// <summary>
    /// Will take in a prompt and get a Yes or No
    /// </summary>
    /// <param name="prompt">The prompt to ask</param>
    /// <param name="skippable">Whether or not this prompt can be skipped</param>
    /// <returns></returns>
    public static bool GetYesNo(string prompt, bool skippable = false) 
    {
        string rawResponse = "";

        char response = 'N';

        bool error = true;

        do
        {
            rawResponse = GetResponse(prompt + " (Y/N)", skippable);

            if(skippable && string.IsNullOrEmpty(rawResponse)) return false;
            else if(char.TryParse(rawResponse, out response))
            {
                response = char.ToUpper(response);

                error = !(response == 'Y' || response == 'N');
            }
            else error = true;

            if(error) Message("Please enter either Y or N", ConsoleColor.DarkYellow);

        } while (error);

        return response == 'Y';
    }
}