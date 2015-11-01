using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private static string[] text;
        private string[] correctedText;
        private string[] settings;

        private static Dictionary<string, string> setting = new Dictionary<string, string>();

        private List<string> starters = new List<string>();

        private bool debug = false;
        private static bool allowSettings = false;
        private static bool isInit = false;

        private string user = Environment.UserName;
        private string toCompile = "";
        private string name = "";
        private string suggestions = "";
        private string cDir = "";
        private static string settingsDirectory = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/";
        private static string settingsFile = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/settings.pseudo";
        private static string csFile = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/compile/cs.pseudo";

        private Process proc;

        private void clearVariablesForReload()
        {
            starters = new List<string>();
        }

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

        [STAThread]
        static void Main(string[] args)
        {

            if (!isInit)
            {
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            }
            
            Console.Title = "PseudoCompiler - READY";
            allowSettings = File.Exists(settingsFile);

            if (!allowSettings)
            {
                try
                {
                    Directory.CreateDirectory(settingsDirectory);
                    Directory.CreateDirectory(settingsDirectory + "files");
                    Directory.CreateDirectory(settingsDirectory + "compile");

                    var file = File.Create(settingsFile);
                    file.Close();

                    file = File.Create(csFile);
                    file.Close();

                    allowSettings = true;
                }
                catch (Exception e)
                {
                    allowSettings = false;
                    Console.WriteLine("Could not find settings file - using defaults.", "error");
                }

                /* Default Settings */
                setting["debugs"] = "false";
                setting["fcolor"] = "white";
                setting["bcolor"] = "black";
                setting["author"] = "David Tossberg";
                setting["source"] = "Click to view!";
                saveSettings();
            }

            if (args.Length == 0)
            {
                /*Console.WriteLine("You need to drag a file onto the .exe to run the compiler.");
                Console.ReadLine();*/
                
                while (true)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.InitialDirectory = settingsDirectory;
                    ofd.Title = "Choose text file to run!";
                 
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        args = new string[]{ ofd.FileName };
                        break;
                    }
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
            string[] toSave = new string[setting.Count()];
            int i = 0;

            foreach (string set in setting.Keys)
            {
                toSave[i] = set + "~" + setting[set];
                i++;
            }

            File.WriteAllLines(settingsFile, toSave);
        }

        private string awaitInput()
        {

            if (user.Equals("admin"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
            }

            Console.Write("\n" + user + "@pseudo-root:~$ ");

            string input = Console.ReadLine();

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

                        Console.ForegroundColor = (ConsoleColor) Enum.Parse(typeof(ConsoleColor), split[1], true);

                    break;

                    case "bcolor":

                        Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), split[1], true);

                    break;
                }
            }
        }

        public void start(string fileName)
        {
            clearVariablesForReload();
            text = File.ReadAllLines(fileName);
            eventHandler = new PseudoEventHandler(this);
            cDir = settingsDirectory.Replace('/', '\\');

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
            suggestions = fileName.Replace(fileName.Split('\\')[fileName.Split('\\').Length - 1], "corrected.txt");
            correctedText = new string[text.Length];

            List<string> parens = new List<string>() // Things that need complete paren sets ( )
            {
                "module", "function", "method", "call"
            };

            starters = new List<string>() // Things that need an end
            {
                "module", "function", "for", "while", "method", "do-while", "do-until", "if"
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
                "using System;",

                "namespace DynaCore",
                "{",

                    "public class DynaCore",
                    "{",

                       // "%global%", // global variables

                        "public static void Main(string str)",
                        "{",
                            "new DynaCore().main();",
                        "}",

                        "private void writeLine(string str, string module)",
                        "{",
                            "Console.Clear();",
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

                string[] args = text[i].Split(' ');

                switch (args[0].ToLower())
                {
                    case "module": // Module changeName() --> private void changeName(){

                        text[i] = text[i].Substring(args[0].Length);
                        text[i] = "public void " + text[i] + "{";
                      
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

                    case "if":

                        if (!text[i].Contains('('))
                        {
                            text[i] = text[i].Substring(args[0].Length);
                            text[i] = "if (" + text[i] + ")";
                        }

                        text[i] += "{";
                        text[i] = text[i].Replace("NOT", "!=");
                        text[i] = text[i].Replace("OR", "||");
                        text[i] = text[i].Replace("AND", "&&");

                        text[i] = text[i].Replace("not", "!=");
                        text[i] = text[i].Replace("or", "||");
                        text[i] = text[i].Replace("and", "&&");

                    break;

                    case "set": case "declare":

                        text[i] = text[i].Replace(args[1], args[1].ToLower());
                        text[i] = text[i].Replace("boolean", "bool");
                        text[i] = text[i].Replace("integer", "int");
                        text[i] = text[i].Replace("real", "float");
                        text[i] = text[i].Substring(args[0].Length);
                        text[i] += ";";

                    break;

                    case "display":

                        text[i] = text[i].Substring(args[0].Length);
                        text[i] = "Console.WriteLine(" + (debug ? "new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name + ' ' + '>' + ' ' + " : "\"> \" + ") + text[i] + ");";
                        text[i] = replaceCommas(text[i]);

                    break;

                    case "input":

                        string header = "> ";
                        text[i] = "Console.Write(" + '"' + header + '"' + ");" + args[1] + " = Console.ReadLine();Console.SetCursorPosition(0, Console.CursorTop - 1);Console.Write(new string(' ', Console.WindowWidth));Console.SetCursorPosition(0, Console.CursorTop-1);";
                        
                    break;

                    case "while":

                        if (!text[i].Contains(')'))
                        {
                            text[i] = text[i].Replace(args[0], "while (") + ")";
                        }

                        text[i] += "{";

                    break;

                    case "do-while": // for people who are too lazy to condition their while loops correctly.

                        if (!text[i].Contains(')'))
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

                    break;

                    case "do-until": // for people who... ARE YOU KIDDING ME? WHY WOULD YOU USE THIS? WHO WOULD USE THIS?!?!?!



                    break;

                    case "for": // So, the pseudo syntax is actually 87% more confusing than the normal syntax.



                    break;

                    case "break": case "return":

                        text[i] += ";";
                        text[i] = replaceCommas(text[i]);

                    break;

                    case "//":

                        text[i] = "/*" + text[i] + "*/";

                    break;

                    default:

                        if (text[i].TrimStart(new char[] { ' ' }).Length > 1 && !text[i].Contains(';') && !text[i].StartsWith("//") && !text[i].StartsWith("*/"))
                        {
                            text[i] += ";";
                        }

                    break;
                }
            }

            string newText = "";

            foreach (string s in text)
            {
                newText += s;
            }

            toCompile = toCompile.Replace("%after%", newText);
            toCompile = toCompile.Replace("\n", "");
            File.WriteAllLines(csFile, new string[] { toCompile });
           
            writeLine("Hey " + Environment.UserName + ", welcome to the Pseudo Compiler.", "system");
            writeLine("A list of suggestions has been saved to corrected.txt.", "system");
            writeLine("You can open it or type 'suggestions' to see it now.", "system");
            writeLine("Written in C#!\n\n=====\nLoaded " + fileName.Split('\\')[fileName.Split('\\').Length - 1] + ". Type 'run' to start or 'help' to see all commands.\n=====", "system");

            while (true)
            {
                string str = awaitInput();

                if (str.Equals("exit"))
                {
                    System.Environment.Exit(0);
                }
                else
                {
                    switch (str.Split(' ')[0])
                    {
                        case "open":

                            OpenFileDialog ofd = new OpenFileDialog();

                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                Console.Clear();
                                Main(new string[1]{ ofd.FileName });
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
                            catch (Exception botchedFile)
                            {
                                writeLine("Error opening file browser!", "error");
                            }

                        break;

                        case "settings": // unnecessary fancy settings menu, must have in every program!

                            Form settingsForm = new Form();

                            Panel panel = new Panel();
                            panel.BackColor = Color.LightSlateGray;
                            panel.Dock = DockStyle.Fill;
                            panel.AutoSize = false;
                            panel.Size = new Size(300, 300);

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

                            GenUtils.CompileAndRun(new string[] { toCompile });

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
                                "suggestions (shows corrected code)",
                                "newinstance (open a new instance in another window)",
                                "admin (login to the super secret account)",
                                "source (view source code on github)",
                                "exit (exits the program)",
                                "",
                                "--- --- --- --- --- --- ---",
                                "Most Windows CMD commands will also work, such as dir, ipconfig, ping, etc."
                            }){
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

                            while (!proc.StandardOutput.EndOfStream){
                                Console.WriteLine(proc.StandardOutput.ReadLine());
                            }

                        break;
                    }
                }
            }
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