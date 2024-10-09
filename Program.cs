//opt boundle --language --output --note --sort --remove-empty-lines --author
using System.CommandLine;   
using System;
using System.IO;
using System.Collections.Generic;


var languageOption =new Option<string>("--language", "List of programming languages"){IsRequired = true};//רשימת שפות התכנות
var outputOption = new Option<FileInfo>("--output", "The name of the exported bundle file");//שם קובץ ה boundle המיוצא
var noteOption = new Option<bool>("--note", "Write the source code as a comment in the bundle file");//לרשום את מקור הקוד כהערה בבאנדל
var sortOption = new Option<string>("--sort", "The order of copying the code files");//סדר העתקת קבצי הקוד
var removeOption = new Option<bool>("--remove-empty-lines", "Delete empty lines");//האם למחוק שורות ריקות
var authorOption = new Option<string>("--author", "Registering the name of the creator of the file");//רשום שם יוצר הקובץ

languageOption.AddAlias("-l");
outputOption.AddAlias("-o");
noteOption.AddAlias("-n");
sortOption.AddAlias("-s");
removeOption.AddAlias("-r");
authorOption.AddAlias("-a");

var boundleCommand = new Command("boundle", "Boundle code files to a single file");

boundleCommand.AddOption(languageOption);
boundleCommand.AddOption(outputOption);
boundleCommand.AddOption(noteOption);
boundleCommand.AddOption(sortOption);
boundleCommand.AddOption(removeOption);
boundleCommand.AddOption(authorOption);


boundleCommand.SetHandler((language,output, note, sort, remove, author) =>
{
    try
    {
        List<string> codeFiles = new List<string>();

        if (language.Equals("all"))   //כל קבצי הקוד שבתיקייה
        {
            Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.TopDirectoryOnly);

            Console.WriteLine("The code files were included in the folder");
        }
        else // רשימת שפות נבחרות
        {
            var languages = File.ReadAllLines(language);
            foreach (var lang in languages)
            {
                codeFiles.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), $"*.{lang.Trim()}", SearchOption.TopDirectoryOnly));
            }
        }
        List<string> combinedCode = new List<string>();

        if (note)
        {
            string comment = $" {Path.GetFileName(output.FullName)} - {Path.GetFullPath(output.FullName)}";
            combinedCode.Insert(0, comment); // מוסיף את ההערה כאלמנט הראשון ברשימה
        }
        if (sort != null)
        {
            // סידור קבצים לפי שם או סוג
            if (sort.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                combinedCode.Sort(); // סידור לפי שם (א"ב)
            }
            else if (sort.Equals("type", StringComparison.OrdinalIgnoreCase))
            {
                combinedCode.Sort((x, y) => Path.GetExtension(x).CompareTo(Path.GetExtension(y))); // סידור לפי סוג
            }
            else
            {
                Console.WriteLine("Invalid sort option. Use 'name' or 'type'.");
            }
        }
        // הוספת שם היוצר אם סופק
        if (!string.IsNullOrEmpty(author))
        {
            combinedCode.Insert(0, $"// Author: {author}"); // הוספת הערת יוצר לקובץ
        }

       
        
        foreach (var file in codeFiles)
        {
            var lines = File.ReadAllLines(file);
            if (remove)
            {
                  // הסרת שורות ריקות
                lines = Array.FindAll(lines, line => !string.IsNullOrWhiteSpace(line));
            }
            combinedCode.AddRange(lines);
        }
        
        File.WriteAllLines(output.FullName, combinedCode); // כתיבת הקבצים לקובץ הפלט
        File.Create(output.FullName);
        Console.WriteLine("File was created");

    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("error: file path unvalid");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }
},languageOption, outputOption, noteOption, sortOption,removeOption,authorOption);

var createRspCommand = new Command("create-rsp", "Create a response file for the boundle command.");

createRspCommand.SetHandler(() =>
{
    Console.Write("Enter the value for --language: ");
    var language = Console.ReadLine();

    Console.Write("Enter the output file name (with .txt extension): ");
    var outputFileName = Console.ReadLine();

    Console.Write("Should it include note? (true/false): ");
    var note = Console.ReadLine();

    Console.Write("Enter the sort order (name/type): ");
    var sort = Console.ReadLine();

    Console.Write("Should it remove empty lines? (true/false): ");
    var remove = Console.ReadLine();

    Console.Write("Enter the author name: ");
    var author = Console.ReadLine();

    var responseFileName = $"{Path.GetFileNameWithoutExtension(outputFileName)}.rsp";
    var responseCommand = $"dotnet boundle --language {language} --output {outputFileName} --note {note} --sort {sort} --remove-empty-lines {remove} --author {author}";

    File.WriteAllText(responseFileName, responseCommand);
    Console.WriteLine($"Response file '{responseFileName}' created. You can run it using: dotnet @{responseFileName}");
});

var rootCommand = new RootCommand("Root command for File Boundle CLI");
rootCommand.AddCommand(boundleCommand);
rootCommand.AddCommand(createRspCommand);
await rootCommand.InvokeAsync(args);