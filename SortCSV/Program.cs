using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
    {
        Console.WriteLine("CSV Cleaner");
        Console.WriteLine("============\n");

        Console.WriteLine("Select your first CSV file.");

        string? file1 = SelectFile();

        if (file1 == null)
        {
            Console.WriteLine("No file selected.");
            return;
        }

        CleanCsv(file1);

        Console.WriteLine("\nSelect your second CSV file.");

        string? file2 = SelectFile();

        if (file2 == null)
        {
            Console.WriteLine("No file selected.");
            return;
        }

        CleanCsv(file2);

        Console.WriteLine("\nDone!");
        Console.WriteLine("Press any key to close.");
        Console.ReadKey();
    }

    static string? SelectFile()
    {
        using OpenFileDialog dialog = new OpenFileDialog();

        dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
        dialog.Title = "Select CSV file";

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return dialog.FileName;
        }

        return null;
    }

    static void CleanCsv(string inputFile)
    {
        Console.WriteLine("\nCleaning:");
        Console.WriteLine(inputFile);

        string[] lines = File.ReadAllLines(
            inputFile,
            Encoding.UTF8
        );

        if (lines.Length == 0)
        {
            Console.WriteLine("File is empty.");
            return;
        }

        List<string> cleanedLines = new List<string>();

        // Keep the header
        cleanedLines.Add(lines[0]);

        // Clean all other rows
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            List<string> fields = ParseCsvLine(lines[i]);

            for (int j = 0; j < fields.Count; j++)
            {
                string value = fields[j].Trim();

                // Remove null values
                if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    fields[j] = "";
                }
                else
                {
                    fields[j] = value;
                }
            }

            cleanedLines.Add(CreateCsvLine(fields));
        }

        // Create the new filename
        string directory = Path.GetDirectoryName(inputFile)!;
        string filename = Path.GetFileNameWithoutExtension(inputFile);

        string outputFile = Path.Combine(
            directory,
            filename + "_Clean.csv"
        );

        File.WriteAllLines(
            outputFile,
            cleanedLines,
            new UTF8Encoding(true)
        );

        Console.WriteLine("Cleaned file created:");
        Console.WriteLine(outputFile);
    }

    static List<string> ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();

        StringBuilder currentField = new StringBuilder();

        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char character = line[i];

            if (character == '"')
            {
                // Double quotes inside a quoted field
                if (insideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (character == ',' && !insideQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(character);
            }
        }

        fields.Add(currentField.ToString());

        return fields;
    }

    static string CreateCsvLine(List<string> fields)
    {
        List<string> result = new List<string>();

        foreach (string field in fields)
        {
            string cleanedField = field.Replace("\"", "\"\"");

            result.Add("\"" + cleanedField + "\"");
        }

        return string.Join(",", result);
    }
}