using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace PseudoCompiler
{
    class PseudoMain
    {
        private PseudoEventHandler eventHandler;
        private GenUtils utils;

        public string[] correctedText;
        public string[] text;
        public string[] settings;
        public string[] commands = new string[]
        {
            "run | run | Run your pseudo code from the file you provided.",
            "view example | example | View an example I drew up for the class.",
            "reload file | reload | Reload the current file if you made changes.",
            "about | about | What is this program? Who made it?",
            "debug mode | debugs | Show some extra information about your modules.",
            "settings | settings | Change some cool settings like color and manage updates.",
            "open new file | open | Change the file without closing the program.",
            "view root files | root | View the data the compiler stores on your computer.",
            "suggestions (beta) | suggestions | This fixes your parens, but that's about it.",
            "view source code | source | View the awesome source code for this project!",
            "recent changes | changelog | View a poorly documented list of changes.",
            "dev console | dev | For those hidden commands no one cares about.",
            "exit | exit | I wonder what this does?"
        };

        public static Dictionary<string, string> setting = new Dictionary<string, string>();
        public Dictionary<int, int> lineSave = new Dictionary<int, int>();
        private Dictionary<string, int> currentLine = new Dictionary<string, int>();

        private bool debug = false;
        private static bool first = false;
        private static bool allowSettings = false;
        private static bool isInit = false;

        private int globalIndex = 0;
        private int globalTop = 0;

        public string toCompile = "";
        private string user = Environment.UserName;
        private string name = "";
        private string cDir = "";
        private string globalChoice = "";
        private static string latestChangeHardCodedBecauseLazy = "Better Menu, if/else bug fix, var name collision fix";
        private static string suggestions = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/suggestions.pseudo";
        private static string settingsDirectory = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/";
        private static string settingsFile = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/settings.pseudo";
        private static string csFile = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/compile/cs.pseudo";
        private static string version = "2.0.1";

        private Process proc;

        public void modifySetting(string setting, string newValue)
        {
            PseudoMain.setting[setting] = newValue;

            if (setting.Equals("debugs"))
            {
                debug = Boolean.Parse(newValue);
            }

            saveSettings();
        }

        public string getSetting(string setting)
        {
            return PseudoMain.setting[setting];
        }

        public bool containsSetting(string setting)
        {
            return PseudoMain.setting.ContainsKey(setting);
        }

        private string[] getCommandInfo(int i)
        {
            return Regex.Split(commands[i], @" \| ");
        }

        private string read(bool main)
        {
            string input = "";

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(main);

                    if (main)
                    {
                        switch (key.Key)
                        {
                            case ConsoleKey.UpArrow: case ConsoleKey.DownArrow:
                            case ConsoleKey.LeftArrow: case ConsoleKey.RightArrow:

                                globalIndex = (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.RightArrow) ? (globalIndex + 1 < commands.Length ? globalIndex + 1 : 0) : (globalIndex - 1 >= 0 ? globalIndex - 1 : commands.Length - 1);
                                Console.SetCursorPosition(0, globalTop);

                                for (int i = 0; i < commands.Length; i++)
                                {
                                    if (i == globalIndex)
                                    {
                                        Console.BackgroundColor = ConsoleColor.DarkMagenta;
                                        Console.WriteLine("[ " + getCommandInfo(i)[0] + " ]");

                                        try
                                        {
                                            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), setting["bcolor"], true);
                                        }
                                        catch (Exception)
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine(" " + getCommandInfo(i)[0] + "     ");
                                    }
                                }

                                Console.Write("\r\n\r> " + getCommandInfo(globalIndex)[2] + "                              ");

                            break;

                            case ConsoleKey.Enter:

                                globalChoice = getCommandInfo(globalIndex)[1];

                            return "done";

                            case ConsoleKey.Home:

                                Process.Start(new ProcessStartInfo(Application.ExecutablePath, name));
                                Environment.Exit(0);

                            return "exit";
                        }
                    }
                    else
                    {
                        switch (key.Key)
                        {
                            case ConsoleKey.Home:

                                Process.Start(new ProcessStartInfo(Application.ExecutablePath, name));
                                Environment.Exit(0);

                            return "exit";

                            case ConsoleKey.Enter: return input;

                            case ConsoleKey.Backspace:

                                if (input.Length > 0)
                                {
                                    input = input.Substring(0, input.Length - 1);
                                    Console.Write(" \b");
                                }
                                else
                                {
                                    Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                                }

                            break;

                            default:
                                input += key.KeyChar;
                            break;
                        }
                    }
                }
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            PseudoMain.first = false;

            if (!isInit)
            {
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            }

            Console.Title = "PseudoCompiler v" + version + " - READY";
            allowSettings = File.Exists(settingsFile);

            if (!allowSettings)
            {
                try
                {
                    PseudoMain.first = true;
                    Directory.CreateDirectory(settingsDirectory);
                    Directory.CreateDirectory(settingsDirectory + "files");
                    Directory.CreateDirectory(settingsDirectory + "compile");

                    var file = File.Create(settingsFile);
                    file.Close();

                    file = File.Create(csFile);
                    file.Close();

                    allowSettings = true;
                }
                catch (Exception)
                {
                    allowSettings = false;
                    Console.WriteLine("Could not find settings file - using defaults. Did you run as administrator?", "error");
                }

                /* Default Settings */
                setting["debugs"] = "false";
                setting["fcolor"] = "white";
                setting["bcolor"] = "black";
                setting["update"] = "automatic";
                setting["author"] = "David Tossberg";
                setting["source"] = "Click to view!";
                saveSettings();
            }

            if (args.Length == 0)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = settingsDirectory;
                ofd.Title = "Choose text file to run!";
                ofd.Filter = "Text Files|*.txt";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    args = new string[] { ofd.FileName };
                }
                else
                {
                    Console.WriteLine("You must select a file to run. Reboot the program and choose a file.\n[ press enter to exit ]");
                    Console.ReadLine();
                    Application.Exit();
                    return;
                }
            }

            if (args[0].EndsWith(".txt"))
            {
                new PseudoMain().start(args[0]);
            }
            else
            {
                string weirdExtension = args[0].Split('.')[args[0].Split('.').Length - 1];
                Console.WriteLine("I have no idea what to do with a " + weirdExtension + " file.\nPlease drag in a .txt file.");
                Console.ReadLine();
            }

            isInit = true;
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            saveSettings();
        }

        private static void saveSettings()
        {
            if (setting.Keys.Count() > 4)
            {
                string[] toSave = new string[setting.Count()];
                int i = 0;

                foreach (string set in setting.Keys)
                {
                    toSave[i] = set + "~" + setting[set];
                    i++;
                }

                File.WriteAllLines(settingsFile, toSave);
            }
        }

        private string awaitInput()
        {

            if (user.Equals("admin"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }

            Console.Write("\n" + user + "@pseudo-root:~$ ");

            string input = read(false);

            if (debug)
            {
                writeLine(input, "input");
            }

            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), PseudoMain.setting["fcolor"], true);

            return input;
        }

        private void loadSettings()
        {
            foreach (string setting in settings)
            {
                string[] split = setting.Split('~');
                PseudoMain.setting[split[0].ToLower()] = split[1];

                switch (split[0].ToLower())
                {
                    case "debugs":

                        debug = Boolean.Parse(split[1]);

                        break;

                    case "fcolor":

                        Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), split[1], true);

                        break;

                    case "bcolor":

                        Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), split[1], true);

                        break;
                }
            }
        }

        public void start(string fileName)
        {
            lineSave = new Dictionary<int, int>();
            text = File.ReadAllLines(fileName);
            eventHandler = new PseudoEventHandler(this);
            utils = new GenUtils(this);
            cDir = settingsDirectory.Replace('/', '\\');

            if (proc != null)
            {
                proc.Dispose();
            }

            proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = cDir
                }
            };

            if (allowSettings)
            {
                settings = File.ReadAllLines(settingsFile);
                loadSettings();
            }

            if (text.Length < 2)
            {
                Console.WriteLine("There isn't enough code in the file to run.");
                Console.ReadLine();
                return;
            }

            name = fileName;
            correctedText = new string[text.Length];

            List<string> parens = new List<string>() // Things that need complete paren sets ( )
            {
                "module", "function", "method", "call"
            };

            for (int i = 0; i < text.Count(); i++)
            {
                text[i] = text[i].TrimStart(new char[] { ' ', '\t' });
                text[i] = text[i].TrimEnd(new char[] { ' ', '\t' });

                if (parens.Contains(text[i].Split(' ')[0].ToLower()))
                {
                    text[i] = fixParens(text[i]);
                }

                correctedText[i] = text[i];
            }

            File.WriteAllLines(suggestions, correctedText);

            foreach (string s in new string[]
            {
                "using System; using System.IO; using System.Collections.Generic;",

                "namespace DynaCore",
                "{",

                    "public class DynaCore",
                    "{",

                       "private Dictionary<string, int> gindex = new Dictionary<string, int>();",
                       "private Dictionary<string, int> fileSize = new Dictionary<string, int>();",
                       "private List<string> appendModes = new List<string>();",
                       "private string PSEUDO_APP_NAME = @\"" + name + "\";",
                       "private string PSEUDO_APP_PATH = @\"" + Application.ExecutablePath + "\";",

                       "private string read()",
                        "{",
                            "string input = \"\";",

                            "while (true)",
                            "{",
                                "if (Console.KeyAvailable)",
                               " {",
                                    "ConsoleKeyInfo key = Console.ReadKey();",
                                    "switch (key.Key)",
                                    "{",
                                        "case ConsoleKey.Home:",

                                            "System.Diagnostics.Process.Start(PSEUDO_APP_PATH, PSEUDO_APP_NAME);",
                                            "Environment.Exit(0);",

                                            "return \"exit\";",

                                        "case ConsoleKey.Enter: Console.Write(\"                                     \");",
                    
                                        "return input;",

                                        "case ConsoleKey.Backspace:",

                                            "if (input.Length > 0)",
                                            "{",
                                                "input = input.Substring(0, input.Length - 1);",
                                                "Console.Write(\" \\b\");",
                                            "}",
                                            "else",
                                            "{",
                                                "Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);",
                                            "}",

                                            "break;",

                                        "default:",
                                            "input += key.KeyChar;",
                                            "break;",
                                    "}",
                                "}",
                            "}",
                        "}",

                        "public static void Main(string str)",
                        "{",
                            "new DynaCore().main();",
                        "}",

                        "protected int inputAsInteger(){",

                            "Console.Write(\"> \");",
                            "int input = 0;",

                            "try { ",
                                "input = int.Parse(read()); ",
                                "Console.SetCursorPosition(0, Console.CursorTop - 1);Console.Write(new string(' ', Console.WindowWidth));Console.SetCursorPosition(0, Console.CursorTop-1);",
                           " } catch (Exception){ ",
                                "Console.SetCursorPosition(0, Console.CursorTop - 1);Console.Write(new string(' ', Console.WindowWidth));Console.SetCursorPosition(0, Console.CursorTop-1);",
                                "Console.WriteLine(\"Can't convert input to an integer - using 0 instead!\"); ",
                            "}",

                            "return input;",
                        "}",

                        "protected float inputAsReal(){",

                            "Console.Write(\"> \");",
                            "float input = 0;",

                            "try { ",
                                "input = float.Parse(read()); ",
                                "Console.SetCursorPosition(0, Console.CursorTop - 1);Console.Write(new string(' ', Console.WindowWidth));Console.SetCursorPosition(0, Console.CursorTop-1);",
                           " } catch (Exception){ ",
                                "Console.SetCursorPosition(0, Console.CursorTop - 1);Console.Write(new string(' ', Console.WindowWidth));Console.SetCursorPosition(0, Console.CursorTop-1);",
                                "Console.WriteLine(\"Can't convert input to a real - using 0 instead!\"); ",
                            "}",

                            "return input;",
                        "}",

                        "protected bool inputAsBoolean(){",

                            "Console.Write(\"> \");",
                            "bool input = false;",

                            "try { ",
                                "input = Boolean.Parse(read()); ",
                                "Console.SetCursorPosition(0, Console.CursorTop - 1);Console.Write(new string(' ', Console.WindowWidth));Console.SetCursorPosition(0, Console.CursorTop-1);",
                           " } catch (Exception){ ",
                                "Console.SetCursorPosition(0, Console.CursorTop - 1);Console.Write(new string(' ', Console.WindowWidth));Console.SetCursorPosition(0, Console.CursorTop-1);",
                                "Console.WriteLine(\"Can't convert input to a boolean - using false instead!\"); ",
                            "}",

                            "return input;",
                        "}",

                        "protected bool isInteger(string input){",

                            "try { ",
                                "int test = int.Parse(input); ",
                                "return true;",
                           " } catch (Exception){ ",
                                "return false; ",
                            "}",
                        "}",

                        "protected bool isReal(string input){",

                            "try { ",
                                "float test = float.Parse(input); ",
                                "return true;",
                           " } catch (Exception){ ",
                                "return false; ",
                            "}",
                        "}",

                        "protected bool isBoolean(string input){",

                            "try { ",
                                "bool test = Boolean.Parse(input); ",
                                "return true;",
                           " } catch (Exception){ ",
                                "return false; ",
                            "}",
                        "}",

                        "protected bool eof(string inputFile)",
                        "{",
                            "return gindex[inputFile] >= fileSize[inputFile];",
                        "}",

                        "protected bool EOF(string inputFile)",
                        "{",
                            "return eof(inputFile);",
                        "}",

                        "protected int length(string input){",
                            "return input.Length;",
                        "}",

                        "protected string toLower(string input){",
                            "return input.ToLower();",
                        "}",

                        "protected string toUpper(string input){",
                            "return input.ToUpper();",
                        "}",

                        "%after%", // fill in converted code
                    "}",
                "}"
            })
            {
                toCompile += s;
            }

            for (int i = 0; i < text.Count(); i++)
            {
                text[i] = text[i].Replace("False", "false");
                text[i] = text[i].Replace("FALSE", "false");
                text[i] = text[i].Replace("True", "true");
                text[i] = text[i].Replace("TRUE", "true");
                text[i] = text[i].Replace(" Integer ", " int ");
                text[i] = text[i].Replace("Integer ", "int ");
                text[i] = text[i].Replace(" Integer", " int");
                text[i] = text[i].Replace(" integer ", " int ");
                text[i] = text[i].Replace(" Integer[] ", " int[] ");
                text[i] = text[i].Replace("Integer[] ", "int[] ");
                text[i] = text[i].Replace(" Integer[]", " int[]");
                text[i] = text[i].Replace(" integer[] ", " int[] ");
                text[i] = text[i].Replace(" String ", " string ");
                text[i] = text[i].Replace("String ", "string ");
                text[i] = text[i].Replace(" String", " string");
                text[i] = text[i].Replace(" String[] ", " string[] ");
                text[i] = text[i].Replace(" String[]", " string[]");
                text[i] = text[i].Replace("String[] ", "string[] ");
                text[i] = text[i].Replace("String[] ", "string[] ");
                text[i] = text[i].Replace("Main()", "main()");
                text[i] = text[i].Replace(" boolean ", " bool ");
                text[i] = text[i].Replace(" boolean", " bool");
                text[i] = text[i].Replace("boolean ", "bool ");
                text[i] = text[i].Replace(" Boolean", " bool");
                text[i] = text[i].Replace("boolean ", "bool ");
                text[i] = text[i].Replace("Boolean ", "bool ");
                text[i] = text[i].Replace(" Boolean ", " bool ");

                string[] args = text[i].Split(' ');

                switch (args[0].ToLower())
                {
                    case "module":
                    case "begin": // Module changeName() --> private void changeName(){

                        text[i] = text[i].Substring(args[0].Length);
                        text[i] = "public void " + text[i] + "{";

                        if (args[1].ToLower().Equals("main"))
                        {
                            text[i] = "public void main() {";
                        }

                        break;

                    case "function": // Function String changeName() --> private String changeName(){

                        text[i] = text[i].Substring(args[0].Length);
                        string toLower = text[i].Split(' ')[0];
                        toLower = toLower.ToLower().Replace("boolean", "bool");
                        text[i] = "public " + toLower.ToLower() + text[i].Substring(toLower.Length) + "{";

                        break;

                    case "end": // End Module --> }

                        text[i] = "}";

                        break;

                    case "call":

                        text[i] = text[i].Substring(args[0].Length) + ";";

                        break;

                    case "sleep":

                        text[i] = "System.Threading.Thread.Sleep(1000 *" + int.Parse(args[1]) + ");";

                        break;

                    case "else":


                        if ((args.Length >= 2 && args[1].ToLower().Equals("if")) || text[i].ToLower().Contains("else if"))
                        {
                            text[i] = "} " + text[i];

                            if (!text[i].Contains('('))
                            {
                                text[i] = text[i].Substring(9);
                                text[i] = "} else if (" + text[i] + ")";
                            }

                            text[i] += "{";
                            text[i] = replaceOperators(text[i]);
                            text[i] = text[i].Replace(" then", "");
                            text[i] = text[i].Replace(" Then", "");
                        }
                        else
                        {
                            text[i] = "} else {";
                        }

                        break;

                    case "if":

                        if (!text[i].ToLower().StartsWith("if ("))
                        {
                            text[i] = text[i].Substring(args[0].Length);
                            text[i] = "if (" + text[i] + ")";
                        }

                        text[i] += "{";
                        text[i] = replaceOperators(text[i]);
                        text[i] = text[i].Replace(" then", "");
                        text[i] = text[i].Replace(" Then", "");

                        break;

                    case "set":
                    case "declare":
                    case "constant":

                        if (args[0].ToLower().Equals("declare") || args[0].ToLower().Equals("constant"))
                        {
                            text[i] = args[1].ToLower();
                        }
                        else
                        {
                            text[i] = args[1];
                        }

                        for (int x = 2; x < args.Length; x++)
                        {
                            if (!args[x].ToLower().Equals("reference") && !args[x].ToLower().Equals("ref"))
                            {
                                text[i] += " " + args[x];
                            }
                        }

                        text[i] = text[i].Replace("boolean", "bool");
                        text[i] = text[i].Replace("boolean[]", "bool[]");
                        text[i] = text[i].Replace("integer", "int");
                        text[i] = text[i].Replace("real", "float");
                        text[i] = text[i].Replace("real[]", "float[]");
                        text[i] = text[i].Replace("True", "true");
                        text[i] = text[i].Replace("False", "false");
                        text[i] = text[i].Replace("InputFile", "string");
                        text[i] = text[i].Replace("inputfile", "string");
                        text[i] = text[i].Replace("inputFile", "string");
                        text[i] = text[i].Replace("Inputfile", "string");
                        text[i] = text[i].Replace("OutputFile", "string");
                        text[i] = text[i].Replace("outputfile", "string");
                        text[i] = text[i].Replace("outputFile", "string");
                        text[i] = text[i].Replace("Outputfile", "string");

                        if (args[2].ToLower().Contains("appendmode") && args.Length >= 4)
                        {
                            text[i] = "appendModes.Add(\"" + args[3] + "\");" + text[i];
                            text[i] = text[i].Replace("AppendMode", "");
                            text[i] = text[i].Replace("Appendmode", "");
                            text[i] = text[i].Replace("appendmode", "");
                        }

                        args = text[i].Split(' ');

                        if (args[1].Contains("]") && args[1].Contains('['))
                        {
                            text[i] = args[0] + "[] " + args[1].Substring(0, args[1].IndexOf('[')) + " = new " + args[0] + "[" + args[1].Substring(args[1].IndexOf('['), args[1].Length - args[1].IndexOf('[')).Replace("[", "").Replace("]", "") + "]";
                        }

                        text[i] += ";";

                        break;

                    case "display":

                        text[i] = text[i].Substring(args[0].Length);
                        text[i] = "Console.WriteLine((" + (debug ? "new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name + ' ' + '>' + ' ' + " : "\"> \" + ") + text[i] + ").ToString());";
                        text[i] = replaceCommas(text[i]);

                        break;

                    case "input":

                        string header = "> ";
                        text[i] = "Console.Write(" + '"' + header + '"' + ");" + args[1] + " = read();";
                        text[i] += "Console.SetCursorPosition(0, Console.CursorTop - 1);Console.Write(new string(' ', Console.WindowWidth));Console.SetCursorPosition(0, Console.CursorTop-1);";

                    break;

                    case "while":

                        if (!text[i].ToLower().StartsWith("while ("))
                        {
                            text[i] = text[i].Replace(args[0], "while (") + ")";
                        }

                        text[i] += "{";
                        text[i] = replaceOperators(text[i]);

                        break;

                    case "do-while": // for people who are too lazy to condition their while loops correctly.

                        if (!text[i].StartsWith("do-while ("))
                        {
                            text[i] = text[i].Substring(args[0].Length);
                            text[i] = "while (!doWhileBool" + i + " || " + text[i] + ")";
                        }
                        else
                        {
                            text[i] = text[i].Replace(")", " || !doWhileBool" + i + ")");
                        }

                        text[i] += "{ doWhileBool" + i + " = true;";
                        text[i] = "bool doWhileBool" + i + " = false;" + text[i];
                        text[i] = replaceOperators(text[i]);

                        break;

                    case "for": // So, the pseudo syntax is actually 87% more confusing than the normal syntax.

                        if (text[i].ToLower().Contains(" to ")) // only change if pseudo
                        {
                            string firstVar = text[i].Split(' ')[1];
                            text[i] = text[i].Substring(args[0].Length);
                            text[i] = "for (int " + text[i];
                            text[i] = text[i].Replace(" to ", "; " + firstVar + " <= ");
                            text[i] = text[i].Replace(" To ", "; " + firstVar + " <= ");
                            if (text[i].ToLower().Contains(" step "))
                            {
                                text[i] = text[i].Replace(" Step ", "; " + firstVar + " += (");
                                text[i] = text[i].Replace(" step ", "; " + firstVar + " += (");
                                text[i] += ")){";
                            }
                            else
                            {
                                text[i] += "; " + firstVar + "++";
                                text[i] += "){";
                            }
                        }

                        break;

                    case "break":
                    case "return":

                        if (!text[i].EndsWith(";"))
                        {
                            text[i] += ";";
                        }

                        text[i] = replaceCommas(text[i]);

                        break;

                    case "select":

                        text[i] = text[i].Substring(args[0].Length);
                        text[i] = "switch (" + text[i] + "){";

                        break;

                    case "case":

                        text[i] = text[i].Substring(args[0].Length);
                        text[i] = "case" + text[i];

                        break;

                    case "//":

                        text[i] = "/*" + text[i] + "*/";

                        break;

                    case "open":

                        text[i] = args[1] + " = " + args[2] + ";";
                        text[i] += "if (!File.Exists(" + args[2] + ")){ var f = File.Create(" + args[2] + "); f.Close(); }";
                        text[i] += "string[] str" + args[1] + " = File.ReadAllLines(" + args[2] + ");";
                        text[i] += "int index" + args[1] + " = 0;";
                        text[i] += "index" + args[1] + " = index" + args[1] + " + 0;"; // workaround for compiler error
                        text[i] += "gindex[" + args[1] + "] = 0;";
                        text[i] += "fileSize[" + args[1] + "] = str" + args[1] + ".Length;";

                        break;

                    case "read":

                        text[i] = args[2] + " = str" + args[1] + "[index" + args[1] + "];";
                        text[i] += "gindex[" + args[1] + "] = gindex[" + args[1] + "] + 1;";
                        text[i] += "index" + args[1] + "++;";

                        break;

                    case "clear":

                        text[i] = "Console.Clear();";

                        break;

                    case "close":

                        text[i] = "gindex.Remove(" + args[1] + ");";
                        text[i] += "fileSize.Remove(" + args[1] + ");";

                        break;

                    case "write":

                        string toWrite = args[1];
                        string notArray = "";

                        for (int w = 2; w < args.Length; w++)
                        {
                            notArray += args[w] + (w + 1 < args.Length ? " " : "");
                        }

                        notArray = notArray.TrimEnd(new char[] { ' ' });


                        text[i] = "if (!appendModes.Contains(\"" + toWrite + "\")) {" +
                                    "File.WriteAllText(" + toWrite + ", string.Empty ); appendModes.Add(\"" + toWrite + "\");}" +
                                  "using (StreamWriter w = File.AppendText(" + toWrite + ")) {" +
                                        "w.WriteLine(" + notArray + ");" +
                                  "}";
                        break;

                    case "default:": break;

                    default:

                        if (text[i].TrimStart(new char[] { ' ' }).Length > 1 && !text[i].Contains(';') && !text[i].StartsWith("//") && !text[i].StartsWith("*/"))
                        {
                            text[i] += ";";
                        }

                        if (text[i].StartsWith("//"))
                        {
                            text[i] = "/* " + text[i] + " */";
                        }

                        break;
                }

                text[i] = text[i].Replace("string ref ", "ref string ");
                text[i] = text[i].Replace("int ref ", "ref int ");
                text[i] = text[i].Replace("float ref ", "ref float ");
                text[i] = text[i].Replace("bool ref ", "ref bool ");

                text[i] = text[i].Replace("string[] ref ", "ref string[] ");
                text[i] = text[i].Replace("int[] ref ", "ref int[] ");
                text[i] = text[i].Replace("float[] ref ", "ref float[] ");
                text[i] = text[i].Replace("bool[] ref ", "ref bool[] ");

                text[i] = text[i].Replace("string[] Ref ", "ref string[] ");
                text[i] = text[i].Replace("int[] Ref ", "ref int[] ");
                text[i] = text[i].Replace("float[] Ref ", "ref float[] ");
                text[i] = text[i].Replace("bool[] Ref ", "ref bool[] ");

                text[i] = text[i].Replace("string Ref ", "ref string ");
                text[i] = text[i].Replace("int Ref ", "ref int ");
                text[i] = text[i].Replace("float Ref ", "ref float ");
                text[i] = text[i].Replace("bool Ref ", "ref bool ");

                text[i] = text[i].Replace("string reference ", "ref string ");
                text[i] = text[i].Replace("int reference ", "ref int ");
                text[i] = text[i].Replace("float reference ", "ref float ");
                text[i] = text[i].Replace("bool reference ", "ref bool ");

                text[i] = text[i].Replace("string Reference ", "ref string ");
                text[i] = text[i].Replace("int Reference ", "ref int ");
                text[i] = text[i].Replace("float Reference ", "ref float ");
                text[i] = text[i].Replace("bool Reference ", "ref bool ");

                text[i] = text[i].Replace("If", "if");
                text[i] = text[i].Replace("Else", "else");

                text[i] = " %line_start: " + i + "%" + text[i] + " %line_end: " + i + "% ";
            }

            string newText = "";

            foreach (string s in text)
            {
                newText += s;
            }

            toCompile = toCompile.Replace("%after%", newText);
            //toCompile = toCompile.Replace("\n", "");
            toCompile = toCompile.Replace(";", ";\n");
            toCompile = toCompile.Replace("}", "}\n");
            toCompile = toCompile.Replace("{", "{\n");
            toCompile = toCompile.Replace("*/", "*/\n");

            string[] theStrings = toCompile.Split('\n');

            for (int qq = 0; qq < theStrings.Length; qq++)
            {
                string checking = theStrings[qq];
                for (int q = 0; q < theStrings.Length; q++)
                {
                    if (checking.Contains("%line_start: " + q + "%"))
                    {
                        lineSave.Add(qq, q);
                        break;
                    }
                }
            }

            for (int q = 0; q < theStrings.Length; q++)
            {
                toCompile = toCompile.Replace("%line_start: " + q + "%", "");
                toCompile = toCompile.Replace("%line_end: " + q + "%", "");
            }

            File.WriteAllLines(csFile, new string[] { toCompile });
            string phrase = ("Welcome " + Environment.UserName + "! " + fileName.Split('\\')[fileName.Split('\\').Length - 1] + " is ready").ToUpper();
            string arrows = "";

           // writeLine("Hi " + Environment.UserName + ", welcome " + (!PseudoMain.first ? "back " : "") + "to the Pseudo Compiler.", "system");

            for (int i = 0; i < phrase.Length; i++)
            {
                arrows += "=";
            }

            Console.WriteLine(arrows);
            Console.WriteLine(phrase);
            Console.WriteLine(arrows + "\n");

            try
            {
                if (getSetting("update").ToLower().Equals("automatic"))
                {
                    writeLine("Checking for updates...", "system");

                    WebClient client = new WebClient();
                    Stream stream = client.OpenRead("https://github.com/lyokofirelyte/PseudoCompiler/blob/master/README.md");
                    StreamReader reader = new StreamReader(stream);
                    string content = reader.ReadToEnd();
                    string version = "";

                    int location = content.IndexOf("<p>Version: ") + 11;
                    int z = 1;

                    while ((location + z) < content.Length)
                    {
                        char c = content[location + z];
                        if (c.Equals('<'))
                        {
                            break;
                        }
                        else
                        {
                            version += c;
                        }
                        z++;
                    }

                    if (!version.Equals(PseudoMain.version))
                    {
                        writeLine("A new version (" + version + ") is released.", "system");
                        writeLine("You're only running version " + PseudoMain.version + ".", "system");
                        writeLine("Automatically updating...", "system");
                        WebClient wc = new WebClient();

                        wc.DownloadProgressChanged += (a, b) =>
                        {
                            Console.Write("\r" + b.ProgressPercentage.ToString() + " ");
                        };

                        wc.DownloadFileCompleted += (a, b) =>
                        {
                            writeLine("Download Complete. Check the current directory for the new file.\n\n[ press enter to exit ]", "system");
                            Console.Read();
                            Application.Exit();
                        };

                        wc.DownloadFileAsync(new Uri("https://github.com/lyokofirelyte/PseudoCompiler/blob/master/PseudoCompiler.exe?raw=true"), "PseudoCompiler v" + version + ".exe");
                        Console.Read();
                        return;
                    }
                    else
                    {
                        writeLine("You're up to date! Running version " + PseudoMain.version, "system");
                        writeLine("The latest change was: " + latestChangeHardCodedBecauseLazy, "system");

                        foreach (string f in Directory.GetFiles(Directory.GetCurrentDirectory()))
                        {
                            if (f.Contains("PseudoCompiler v") && f.EndsWith(".exe") && !f.Contains("PseudoCompiler v" + PseudoMain.version + ".exe"))
                            {
                                File.Delete(f);
                                writeLine("Deleted " + f.Split('\\')[f.Split('\\').Length - 1] + " (old version)", "system");
                            }
                        }
                    }
                }
                else
                {
                    writeLine("You've disabled automatic updating.\n> You could be behind on important fixes!", "system");
                }
            }
            catch (Exception)
            {
                writeLine("Update check failed. No internet or github is down.", "error");
            }

            Console.WriteLine("\n> You can press the Home key at any time to reload (even while your code is running)");
            Console.WriteLine("> Use your arrow keys to select an option. Press enter to confirm.");
            /* Console.Write("> Type ");
             cWrite("run", ConsoleColor.Magenta);
             Console.WriteLine(" to run your pseudo code.");

             Console.Write("> Type ");
             cWrite("debug", ConsoleColor.Magenta);
             Console.WriteLine(" to toggle module names on/off.");

             Console.Write("> Type ");
             cWrite("settings", ConsoleColor.Magenta);
             Console.WriteLine(" to change options such as text and background color.");

             Console.Write("> Type ");
             cWrite("help", ConsoleColor.Magenta);
             Console.WriteLine(" to see all of the commands you can type.");
             Console.WriteLine();

             writeLine("There is special syntax for reference variables and input.", "system");

             Console.Write("> Please download the new example.txt for v1.6 by typing ");
             cWrite("example", ConsoleColor.Magenta);

             Console.WriteLine();*/

            outputMenu();

            while (!read(true).Equals("done")){}

            string str = "";

            if (globalChoice.Equals("dev"))
            {
                str = awaitInput();
            }
            else
            {
                str = globalChoice;
            }

            manageInputs(str, fileName);
        }

        private void outputMenu()
        {
            Console.WriteLine("\n= - = - = - = - = - = - =\nPseudo Compiler Main Menu\n= - = - = - = - = - = - =\n");
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            globalTop = Console.CursorTop;
            globalIndex = 0;
            Console.WriteLine("[ " + getCommandInfo(0)[0] + " ]");

            try
            {
                Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), setting["bcolor"], true);
            }
            catch (Exception)
            {
                Console.BackgroundColor = ConsoleColor.Black;
            }

            for (int i = 1; i < commands.Length; i++)
            {
                Console.WriteLine(" " + getCommandInfo(i)[0]);
            }

            Console.Write("\r\n\r> " + getCommandInfo(globalIndex)[2] + "                              \n");
        }

        private void manageInputs(string str, string fileName)
        {
            if (str.Equals("exit"))
            {
                System.Environment.Exit(0);
            }
            else
            {
                switch (str.Split(' ')[0].Trim(new char[] { ' ' }))
                {
                    case "open":

                        OpenFileDialog ofd = new OpenFileDialog();

                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            Console.Clear();
                            Main(new string[1] { ofd.FileName });
                            return;
                        }

                        break;

                    case "root":

                        try
                        {
                            OpenFileDialog fbd = new OpenFileDialog();
                            fbd.Title = "View Pseudo Files";
                            fbd.InitialDirectory = settingsDirectory.Replace('/', '\\');
                            fbd.ShowDialog();
                        }
                        catch (Exception)
                        {
                            writeLine("Error opening file browser!", "error");
                        }

                        break;

                    case "update":

                        modifySetting("update", getSetting("update").Equals("automatic") ? "disabled" : "automatic");
                        writeLine("Update setting changed to: " + getSetting("update"), "system");

                        break;

                    case "settings": // unnecessary fancy settings menu, must have in every program!

                        Form settingsForm = new Form();

                        Panel panel = new Panel();
                        panel.BackColor = Color.LightSlateGray;
                        panel.Dock = DockStyle.Fill;
                        panel.AutoSize = false;
                        panel.Size = new Size(300, 330);

                        settingsForm.FormBorderStyle = FormBorderStyle.FixedSingle;
                        settingsForm.Text = "Global Pseudo Settings";
                        settingsForm.AutoScaleMode = AutoScaleMode.Font;
                        settingsForm.AutoSize = false;
                        settingsForm.Size = panel.Size;
                        settingsForm.MaximizeBox = false;
                        settingsForm.MinimizeBox = false;

                        int yAmt = 0;

                        foreach (string set in setting.Keys)
                        {
                            GenUtils.FancyLabel l = new GenUtils.FancyLabel()
                            {
                                Name = set,
                                Location = new Point(5, settingsForm.Location.Y + yAmt + 5),
                                AutoSize = true,
                                Text = set.ToLower(),
                                Font = new Font("Courier New", 20),
                                BackColor = Color.Purple,
                                ForeColor = Color.White,
                                TextAlign = ContentAlignment.MiddleCenter,
                                Parent = panel
                            };

                            Control cont = (Control)l; // A little cheaty method to get it to accept a mouse over
                            cont.MouseDown += new MouseEventHandler(eventHandler.onMouseDown);
                            cont.MouseEnter += new EventHandler(eventHandler.focusGained);
                            cont.MouseLeave += new EventHandler(eventHandler.focusLost);

                            panel.Controls.Add(l);
                            l.Visible = true;
                            l.BringToFront();
                            yAmt += 10 + l.Height;
                        }

                        settingsForm.Controls.Add(panel);
                        settingsForm.Visible = true;

                        Application.Run(settingsForm);
                        Console.Clear();
                        saveSettings();
                        Main(new string[1] { fileName });

                        return;

                    case "reload":

                        Console.Clear();
                        Main(new string[1] { fileName });

                        return;

                    case "changelog":

                        Process.Start("https://github.com/lyokofirelyte/PseudoCompiler/commits/master");

                        break;

                    case "suggestions":

                        if (File.Exists(suggestions))
                        {
                            string[] txt = File.ReadAllLines(suggestions);
                            int i = 1; // I was tempted to leave this at 0, but alas...
                            bool found = false;

                            foreach (string t in txt)
                            {
                                if (t.Contains("[system]"))
                                {
                                    writeLine("Suggestion @ line " + i, "system");
                                    writeLine(t.Replace("[system]", ""), "read");
                                    writeLine("... ... ...", "system");
                                    found = true;
                                }
                                i++;
                            }

                            if (!found)
                            {
                                writeLine("No suggestions found! Good job!", "system");
                            }
                            else
                            {
                                proc.StartInfo.Arguments = "/c notepad " + suggestions;
                                proc.Start();
                            }
                        }
                        else
                        {
                            writeLine("The suggestions file appears to be missing.", "error");
                        }

                        break;

                    case "run":

                        Console.Clear();
                        Console.Title = "PseudoCompiler - RUNNING";

                        utils.CompileAndRun(new string[] { toCompile });

                        Console.Title = "PseudoCompiler - READY";
                        writeLine("----", "system");
                        writeLine("Code finished. Type 'help' for a list of commands.", "system");
                        writeLine("Remember to view the suggestions if your code didn't work out well.", "system");

                        break;

                    case "debug":

                        Console.Clear();
                        debug = !debug;
                        setting["debugs"] = debug + "";
                        saveSettings();
                        Main(new string[1] { fileName });

                        return;

                    case "source":

                        Process.Start("https://github.com/lyokofirelyte/PseudoCompiler");

                        break;

                    case "example":


                        WebClient wc = new WebClient();

                        wc.DownloadProgressChanged += (a, b) =>
                        {
                            Console.Write("\r" + b.ProgressPercentage.ToString() + " ");
                        };

                        wc.DownloadFileCompleted += (a, b) =>
                        {
                            writeLine("Download Complete. Type 'open' to find and run the example.txt", "system");
                            Process.Start("example.txt");
                            Console.WriteLine("[ press enter to continue ]");
                        };

                        wc.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/lyokofirelyte/PseudoCompiler/master/example.txt"), "example.txt");

                        break;

                    case "help":

                        Console.Clear();

                        foreach (string s in new string[]
                        {
                                "> Pseudo Compiler Commands <",
                                "--- --- --- --- --- --- ---",
                                "",
                                "run (runs the program)",
                                "example (displays example pseudo code)",
                                "about (what even is this?)",
                                "reload (reloads the text file for a fresh start)",
                                "debug (toggles debug mode on / off)",
                                "settings (modify settings)",
                                "open (open a new file to run)",
                                "root (view program files)",
                                "suggestions (shows corrected code)",
                                "newinstance (open a new instance in another window)",
                                "admin (login to the super secret account)",
                                "source (view source code on github)",
                                "changelog (view a poorly described list of changes)",
                                "exit (exits the program)",
                                "",
                                "--- --- --- --- --- --- ---",
                                "Most Windows CMD commands will also work, such as dir, ipconfig, ping, etc."
                        })
                        {
                            Console.WriteLine(s);
                        }

                        break;

                    case "about":

                        Console.Clear();

                        foreach (string s in new string[]
                        {
                                 "> Pseudo Compiler Information <",
                                 "--- --- --- --- --- --- ---",
                                 "",
                                 "Greetings! I'm David. I wrote this thing.",
                                 "The purpose of this program is to compile & run pseudo code.",
                                 "Specifically, the code standards taught in ITS 140 @ Purdue.",
                                 "I feel that if students can actually see the code run, it will help a lot.",
                                 "They can fix syntax mistakes, get a better feel for how code operates, etc.",
                                 "And, of course, it's always cool to see what you wrote actually do something.",
                                 "--- --- ---",
                                 "This program converts your pseudo code into C# code, compiles, and runs.",
                                 "That means you can actually type RAW C# code into your pseudo code.",
                                 "You'll need to call the entire path if you need a system library.",
                                 "(Example: System.Collections.Generic.Dictionary)",
                                 "I only import System for basic pseudo stuff.",
                                 "You have access to all the functions that come with the data types too!",
                                 "You can do anything with this compiler. And I hope that you enjoy."
                        })
                        {
                            Console.WriteLine(s);
                        }

                        break;

                    case "newinstance":

                        Process.Start(new ProcessStartInfo(Application.ExecutablePath));

                        break;

                    case "admin":

                        Console.Clear();
                        Console.Write("Top secret admin account password: ");

                        if (Console.ReadLine().Equals("guest"))
                        {
                            user = user.Equals("admin") ? Environment.UserName : "admin";
                        }
                        else
                        {
                            writeLine("Invalid password. I wonder where you could find it?", "system");
                        }

                        break;

                    default: // Allowed default windows commands, just for extra... oomph.

                        if (str.Equals("ls")) // [mild laughter]
                        {
                            str = "dir";
                        }

                        proc.StartInfo.Arguments = "/c " + str;
                        proc.Start();

                        while (!proc.StandardOutput.EndOfStream)
                        {
                            Console.WriteLine(proc.StandardOutput.ReadLine());
                        }

                        break;
                }
            }

            Console.WriteLine("\n\n[ press enter to return to main menu ]");
            Console.ReadLine();
            Console.Clear();

            if (globalChoice.Equals("dev"))
            {
                manageInputs(awaitInput(), fileName);
            }
            else
            {
                outputMenu();
                while (!read(true).Equals("done")) { }
                if (globalChoice.Equals("dev"))
                {
                    manageInputs(awaitInput(), fileName);
                }
                else
                {
                    manageInputs(globalChoice, fileName);
                }
            }
        }

        private void cWrite(string toWrite, ConsoleColor color)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(toWrite);
            Console.ForegroundColor = old;
        }

        private string replaceOperators(string item)
        {
            item = item.Replace("NOT eof", "!eof");
            item = item.Replace("NOT EOF", "!eof");
            item = item.Replace(" NOT ", " != ");
            item = item.Replace(" OR ", " || ");
            item = item.Replace(" AND ", " && ");

            item = item.Replace("not eof", "!eof");
            item = item.Replace("not EOF", "!eof");
            item = item.Replace("Not EOF", "!eof");
            item = item.Replace(" not ", " != ");
            item = item.Replace(" or ", " || ");
            item = item.Replace(" and ", " && ");

            return item;
        }

        private string replaceCommas(string item)
        {
            string newString = "";
            bool isInQuotes = false;

            for (int x = 0; x < item.Length; x++)
            {
                char c = item[x];

                if (!isInQuotes)
                {
                    if (c.Equals(','))
                    {
                        c = '+';
                    }
                    else if (c.Equals('"'))
                    {
                        isInQuotes = true;
                    }
                }
                else
                {
                    if (c.Equals('"'))
                    {
                        isInQuotes = false;
                    }
                }

                newString += c;
            }

            return newString;
        }

        private string fixParens(string line) // assumes there needs to be a method of some sort
        {
            string newLine = "";
            bool foundStart = false;
            bool foundEnd = false;

            if (!line.Contains('(') && !line.Contains(')'))
            {
                return line + "() // Added ending parens." + " [system]"; // easy quick check before the rough stuff.
            }

            if (line.IndexOf(')') != line.LastIndexOf(')') || line.IndexOf('(') != line.LastIndexOf('('))
            {
                return "// Invalid module name // " + line + " [system]"; // That'll teach em'
            }

            foreach (char c in line.ToCharArray())
            {
                if (!foundStart && !c.Equals('('))
                {
                    newLine += c.Equals(')') ? "() // Added start param & trimmed out the rest [system]" : c.ToString(); // All good so far, add crap to the line.
                    if (c.Equals(')'))
                    {
                        break;
                    }
                }
                else
                {
                    if (c.Equals('('))
                    {
                        foundStart = true; // Ok, we need params or a end ) now.
                        newLine += c.ToString();
                    }
                    else if (c.Equals(')'))
                    {
                        newLine += (foundStart ? ")" : "() // Added start paren ( to match closing ) [system]"); // Close it off.
                        foundEnd = true;
                    }
                    else
                    {
                        newLine += c.ToString();
                    }
                }
            }

            if (!foundEnd && foundStart)
            {
                newLine += ") // Added ending paren" + @" [system]"; // They're lazy so we fix it for them.
            }

            return newLine;
        }

        private void writeLine(string message, string module)
        {
            Console.WriteLine((debug ? module + " > " : "> ") + message);
        }
    }
}