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

        private string[] text;
        private string[] correctedText;
        private string[] settings;

        private Dictionary<string, Module> mods = new Dictionary<string, Module>();
        private static Dictionary<string, string> setting = new Dictionary<string, string>();

        private List<string> currentOutput = new List<string>();
        private List<string> starters = new List<string>();
        private List<Variable> vars = new List<Variable>();

        private bool debug = false;
        private static bool allowSettings = false;
        private static bool isInit = false;

        private string name = "";
        private string suggestions = "";
        private static string settingsDirectory = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/";
        private static string settingsFile = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/settings.pseudo";

        private void clearVariablesForReload()
        {
            currentOutput = new List<string>();
            starters = new List<string>();
            vars = new List<Variable>();
            mods = new Dictionary<string, Module>();
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
                    var file = File.Create(settingsFile);
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
            Console.Write("\n" + Environment.UserName + "@pseudo-root:~$ ");
            string input = Console.ReadLine();
            if (debug)
            {
                writeLine(input, "input");
            }
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
                    case "debug":

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

            fixEnds();
            File.WriteAllLines(suggestions, correctedText);

            for (int i = 0; i < text.Count(); i++)
            {
                if (text[i].ToLower().StartsWith("function")) // convert to module because there's no difference. Why do extra work? //TODO rant about pseudocode
                {
                    string[] textSplit = text[i].Split(' '); // [ Function ] [ String ] [ WhoCares ]
                    string newText = "module";

                    for (int index = 2; index < textSplit.Length; index++)
                    {
                        newText += " " + textSplit[index]; // Skip [ String ]
                    }
                    text[i] = newText;
                }

                text[i] = parseAll(text[i]);
            }

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
                    switch (str)
                    {
                        default:

                            writeLine("Unknown Command.", "system");

                        break;

                        case "open":

                            OpenFileDialog ofd = new OpenFileDialog();

                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
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
                            writeLine("Settings menu closed.", "system");

                        break;

                        case "reload":

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
                            }
                            else
                            {
                                writeLine("The suggestions file appears to be missing.", "error");
                            }

                        break;

                        case "run":

                            currentOutput = new List<string>();
                            List<String> mainModule = findModule("main");

                            if (mainModule != null && mainModule.Count() > 0)
                            {
                                Console.Title = "PseudoCompiler - RUNNING";
                                parseModule(mainModule, "main", 0, "main");
                                Console.Title = "PseudoCompiler - READY";
                                writeLine("----", "system");
                                writeLine("Code finished. Type 'help' for a list of commands.", "system");
                                writeLine("Remember to view the suggestions if your code didn't work out well.", "system");
                            }
                            else
                            {
                                writeLine("No main module found! Add a main module and type 'reload'.", "error");
                            }

                        break;

                        case "debug":

                            debug = !debug;
                            writeLine("Debug mode toggled.", "system");
                            setting["debugs"] = debug + "";

                        break;

                        case "source":

                            Process.Start("https://github.com/lyokofirelyte/PseudoCompiler");

                        break;

                        case "help":

                            Console.Clear();

                            foreach (string s in new string[] 
                            {
                                "run (runs the program)",
                                "reload (reloads the text file for a fresh start)",
                                "debug (toggles debug mode on / off)",
                                "settings (modify settings)",
                                "open (open a new file to run)",
                                "suggestions (shows corrected code)",
                                "exit (exits the program)"
                            }){
                                Console.WriteLine(s);
                            }
                                    
                        break;
                    }
                }
            }
        }

        private string parseAll(string line) // Pre-parser
        {
            string[] args = line.Split(' ');

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "module": case "function": // Module [ main(String args0, String args1) ]

                        args = line.Split(' ');
                       // line = fixParens(line);

                        Module module = new Module(args[(args[0].ToLower().Equals("module") ? 1 : 2)].Substring(0, args[(args[0].ToLower().Equals("module") ? 1 : 2)].IndexOf('('))); // [ main ]
                        module.setLines(findModule(module.Name));
                        mods.Add(module.Name, module);

                    break;
                }
            }

            return line;
        }

        private void fixEnds() // Makes sure all modules start and end properly.
        {
            string currentStart = "";

            for (int i = 0; i < correctedText.Length; i++)
            {
                string firstWord = correctedText[i].Split(' ')[0].ToLower();
                string secondWord = correctedText[i].Split(' ').Length > 1 ? correctedText[i].Split(' ')[1] : "";

                if (starters.Contains(firstWord)) // Ok we found a start module. Let's see what we have found already.
                {
                   if (!currentStart.Equals("")) // This is our first new starter
                   {
                       correctedText[i] += " // Out of place start (previous module not ended)! [system]";
                   }

                   currentStart = firstWord;
                }
                else if (!currentStart.Equals(""))// So this means we're looking for an ending segment.
                {
                    if (firstWord.ToLower().Equals("end") && secondWord.ToLower().Equals(currentStart))
                    {
                        currentStart = ""; // All good, reset everything.
                    }
                    else if (firstWord.ToLower().Equals("end"))
                    {
                        correctedText[i] += " // Out of place end! [system]"; // Yeah.. but not the right ending.
                    }
                }
                else if (firstWord.ToLower().Equals("end"))
                {
                    correctedText[i] += " // Out of place end! [system]";
                }
            }

            if (!currentStart.Equals(""))
            {
                correctedText[correctedText.Length - 1] += " // Module never ends! [system]";
            }
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

        public Variable findVar(string module, string varName)
        {
            foreach (Variable v in vars)
            {
                if (v.isExactMatch(module, varName))
                {
                    return v;
                }
            }

            return null;
        }

        private string parseModule(List<String> content, string moduleName, int startingLine, string cameFrom)
        {
            for (int x = 0; x < content.Count(); x++)
            {
                content[x] = content[x].TrimStart(new char[] { ' ', '\t' });
                content[x] = content[x].TrimEnd(new char[] { ' ', '\t' });
                string currentLine = content[x];

                if (currentLine.Split(' ').Length > 1)
                {
                    string[] args = currentLine.Split(' ');

                    switch (args[0].ToLower())
                    {
                        case "set": // Set someVar = 10

                            Variable var = findVar(moduleName, args[1]);

                            if (var != null)
                            {
                                string toSet = "";

                                for (int i = 3; i < args.Length; i++)
                                {
                                    Variable possiblyAVariable = findVar(moduleName, args[i]);
                                    toSet += possiblyAVariable != null ? possiblyAVariable.Value : (toSet.Equals("") ? "" : " ") + args[i]; // Account for setting a var to another var's value.
                                }

                                toSet = toSet.Replace("\"", "");

                                Variable checker = var;
                                var.Value = toSet;

                                while (true) // Check down (up?) the rabbit hole for reference updates
                                {
                                    if (checker.hasReference())
                                    {
                                        checker.getReference().Value = toSet;
                                        checker = checker.getReference();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                writeLine("No variable found for " + args[1] + "!", "error");
                                return "null";
                            }

                        break;

                        case "input":

                            if (args.Length == 2)
                            {
                                Variable toInput = findVar(moduleName, args[1]);

                                if (toInput == null)
                                {
                                    writeLine("The variable " + args[1] + " has not been declared yet.", "error");
                                }
                                else
                                {
                                    toInput.Value = awaitInput();
                                }
                            }
                            else
                            {
                                writeLine("No variable found to input!", "error");
                            }

                        break;

                        case "while":



                        break;

                        case "declare":

                            bool found = false;

                            if (args.Length == 3 || (args.Length >= 5 && args[3].Equals("=")))
                            {
                                foreach (Variable v in vars)
                                {
                                    if (v.isExactMatch(moduleName, args[2]))
                                    {
                                        writeLine("Unnecessary decleration - " + args[2] + " is already declared.", "warning");
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    string toAdd = "";

                                    if (args.Length >= 5)
                                    {
                                        for (int i = 4; i < args.Length; i++)
                                        {
                                            toAdd += (toAdd.Equals("") ? "" : " ") + args[i];
                                        }
                                    }

                                    toAdd = toAdd.TrimStart(new char[] { '"' });
                                    toAdd = toAdd.TrimEnd(new char[] { '"' });

                                    Variable variable = new Variable(args[2], toAdd.Equals("") ? "null" : toAdd, moduleName);
                                    vars.Add(variable);
                                }
                            }
                            else
                            {
                                writeLine("Incorrect declaration syntax!", "error");
                                return "null";
                            }

                        break;

                        case "display": // Display args

                            string toDisplay = "";
                            string finalDisplay = "";
                            bool isInQuotes = false;

                            for (int i = 1; i < args.Length; i++)
                            {
                                toDisplay += (toDisplay.Equals("") ? "" : " ") + args[i];
                            }

                            for (int i = 0; i < toDisplay.Length; i++)
                            {
                                char c = toDisplay.ToCharArray()[i];

                                if (i == 0 && !c.Equals('\"'))
                                {
                                    isInQuotes = true; // sneaky sneaky
                                }

                                if (c.Equals('\"') || i == 0) // begin or end quote
                                {
                                    if (isInQuotes) // exiting quotes, expecting start of variable  "txt", var, "txt" or method call someMethod()
                                    {
                                        string tempVar = "";

                                        for (i = (i > 0 ? i + 3 : 0); i < toDisplay.Length; i++) // parse the variables. "text", var, var2, var3, "text2"
                                        {
                                            char tempChar = toDisplay.ToCharArray()[i];

                                            if (!tempChar.Equals('\"'))
                                            {
                                                tempVar += tempChar;
                                            }
                                            else
                                            {
                                                i--;
                                                break;
                                            }
                                        }

                                        if (!tempVar.Equals(""))
                                        {
                                            string[] vars = tempVar.Split(',');

                                            for (int d = 0; d < vars.Length; d++)
                                            {
                                                vars[d] = vars[d].Replace(" ", "");

                                                if (findVar(moduleName, vars[d]) != null)
                                                {
                                                    finalDisplay += findVar(moduleName, vars[d]).Value;
                                                }
                                                else
                                                {
                                                    if (vars[d].Contains(')') && vars[d].Contains('('))
                                                    {
                                                        string checkModule = vars[d].Substring(0, vars[d].IndexOf('('));

                                                        if (mods.ContainsKey(checkModule))
                                                        {
                                                            Module mod = mods[checkModule];
                                                            string[] modInputParamNames = null;

                                                            string modParams = "";

                                                            try
                                                            {
                                                                modParams = vars[d].IndexOf('(') != vars[d].IndexOf(')') - 1 ? vars[d].Substring(vars[d].IndexOf('('), vars[d].Length - vars[d].IndexOf('(')).Replace(")", "") : "";
                                                                string bleh = mod.getLines()[0].Substring(mod.getLines()[0].IndexOf('('), mod.getLines()[0].Length - mod.getLines()[0].IndexOf('(')).Replace(")", "");
                                                                bleh = bleh.TrimStart(new char[] { '(' });
                                                                modParams = modParams.TrimStart(new char[] { '(' });
                                                                modInputParamNames = Regex.Split(bleh, @", ");
                                                            }
                                                            catch (Exception botchedParams) { }

                                                            string[] modParamArgs = Regex.Split(modParams, @", ");

                                                            for (int xx = 0; xx < modParamArgs.Length; xx++)
                                                            {
                                                                Variable currParamArgVar = findVar(moduleName, modParamArgs[xx]);

                                                                if (currParamArgVar != null)
                                                                {
                                                                    modParamArgs[xx] = currParamArgVar.Value;
                                                                }
                                                                else
                                                                {
                                                                    modParamArgs[xx] = modParamArgs[xx].TrimStart('"');
                                                                    modParamArgs[xx] = modParamArgs[xx].TrimEnd('"');
                                                                }

                                                                Variable newVar = new Variable(modInputParamNames[xx], modParamArgs[xx], mod.Name);
                                                                Variable oldVar = findVar(mod.Name, newVar.Name);

                                                                if (oldVar != null)
                                                                {
                                                                    oldVar.Value = newVar.Value;
                                                                }
                                                                else
                                                                {
                                                                    this.vars.Add(newVar);
                                                                }
                                                            }

                                                            string moduleCallResult = parseModule(mod.getLines(), mod.Name, startingLine, moduleName);
                                                            finalDisplay += moduleCallResult;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    isInQuotes = !isInQuotes;
                                }
                                else if (isInQuotes)
                                {
                                    finalDisplay += c;
                                }
                            }

                            writeLine(finalDisplay, moduleName);

                        break;

                        case "if":

                            string ifArgSingle = "";

                            for (int i = 1; i < args.Length; i++)
                            {
                                ifArgSingle += (ifArgSingle.Equals("") ? "" : " ") + args[i];
                            }

                            string[] ifArgs = ifArgSingle.Split(' ');



                        break;

                        case "call":

                            string callingModule = args[1].Substring(0, args[1].IndexOf("("));
                            
                            if (currentLine.IndexOf("(") != currentLine.IndexOf(")") - 1)
                            {
                                string modString = currentLine.Substring(currentLine.IndexOf("("), (currentLine.IndexOf(")") + 1 - currentLine.IndexOf("(")));
                                modString = modString.TrimStart(new char[] { '(', '"', ')' });
                                modString = modString.TrimEnd(new char[] { '(', '"', ')' });
                                string[] paramz = Regex.Split(modString, @", "); // someVarValue, someOtherVarValue

                                if (paramz.Length == 0)
                                {
                                    paramz = new string[] { modString };
                                }

                                for (int y = 0; y < paramz.Length; y++)
                                {
                                    string param = paramz[y];
                                    string paramLine = findModule(callingModule)[0];
                                    string[] searchForParams = paramLine.Split(' '); // [Module test(String test, String test2)]
                                    string paramSegment = paramLine.Substring(paramLine.IndexOf('(') + 1, paramLine.IndexOf(')') - paramLine.IndexOf('(') - 1); // [String Reference something, String otherThing]
                                    string[] acceptingParams = Regex.Split(paramSegment, @", "); // [String Reference something] [String otherThing]

                                    if (acceptingParams.Length == 0)
                                    {
                                        acceptingParams = new string[] { paramSegment };
                                    }

                                    string[] paramNames = new string[acceptingParams.Length]; // [something] [otherThing]

                                    for (int i = 0; i < acceptingParams.Length; i++)
                                    {
                                        paramNames[i] = acceptingParams[i].Split(' ')[acceptingParams[i].Split(' ').Length == 3 ? 2 : 1]; // chop off 'reference' if necessary
                                    }

                                    /* BEHOLD! THE MIGHTY REFERENCE CODE. SO SIMPLE... YET ELEGANT. */

                                    bool isRef = false;

                                    for (int i = 0; i < acceptingParams.Length; i++)
                                    {
                                        if (acceptingParams[i].Split(' ').Length == 3)
                                        {
                                            isRef = acceptingParams[i].Split(' ')[2].Equals(paramNames[y]);
                                        }
                                    }

                                    Variable currVar = findVar(moduleName, param);

                                    param = param.TrimStart(new char[] { '"' });
                                    param = param.TrimEnd(new char[] { '"' });

                                    string value = currVar != null ? currVar.Value : param;

                                    Variable newVarPassing = new Variable(paramNames[y], value, callingModule);

                                    if (isRef && currVar != null)
                                    {
                                        newVarPassing.addReference(currVar);
                                    }

                                    vars.Add(newVarPassing);
                                }
                            }

                            if (!callingModule.Equals(moduleName))
                            {
                                parseModule(mods[callingModule].getLines(), callingModule, (x + startingLine), moduleName);
                            }
                            else
                            {
                                writeLine("A module can't call itself right now. Use a loop instead!", "error");
                            }

                        break;

                        case "return":

                            string[] toCalc = new string[args.Length - 1];

                            for (int i = 1; i < args.Length; i++)
                            {
                                toCalc[i-1] = args[i];
                            }

                        return calculate(moduleName, toCalc);
                    }
                }
            }

            return "null";
        }

        private string calculate(string module, string[] args)
        {
            return "test";
        }

        private bool isVarInModule(string var, string module)
        {
            return findVar(module, var) != null;
        }

        private void writeLine(string message, string module)
        {
            Console.Clear();

            string msg = "";
            message = message.TrimEnd(new char[] { '\n' });
            message = message.TrimStart(new char[] { '\n' });
            module = module.TrimEnd(new char[] { '\n', ' ' });
            module = module.TrimStart(new char[] { '\n', ' ' });

            foreach (Variable v in vars)
            {
                msg += msg.Equals("") ? "" : ", ";
                msg += v.Name + " = " + v.Value;
            }

            currentOutput.Add(module + " " + message);

            foreach (string thing in currentOutput)
            {
                string spaces = "";
                string mod = thing.Split(' ')[0];
                int max = mod.Length;

                foreach (string t in currentOutput)
                {
                    if (t.Split(' ')[0].Length > max)
                    {
                        max = t.Split(' ')[0].Length;
                    }
                }

                for (int i = 0; i < (max - mod.Length); i++)
                {
                    spaces += " ";
                }

                Console.WriteLine((debug ? (mod + spaces) : "") + " > " + thing.Replace(mod + " ", ""));
            }

            Console.WriteLine("\n----------\n" + (debug ? (msg.Equals("") ? "no variables defined" : msg) + "\n" : "") + "----------\n");
        }

        private List<String> findModule(string moduleName)
        {

            List<String> toReturn = new List<String>();

            for (int i = 0; i < text.Length; i++)
            {
                string line = text[i];
                if (line.ToLower().StartsWith("module") || line.ToLower().StartsWith("function"))
                {
                    if (line.Split(' ').Length > 1 && line.Split(' ')[line.ToLower().StartsWith("module") ? 1 : 2].ToLower().StartsWith(moduleName.ToLower()))
                    {
                        while (!line.ToLower().StartsWith("end module") && !line.ToLower().StartsWith("end function"))
                        {

                            toReturn.Add(line);

                            if (i >= 250)
                            {
                                writeLine("Too many lines without module end!", "error");
                                break;
                            }

                            if (i >= text.Length - 1)
                            {
                                writeLine("No module end found for " + moduleName + "!", "error");
                                break;
                            }

                            i++;
                            line = text[i];
                        }
                        break;
                    }
                }
            }

            if (toReturn.Count() <= 0)
            {
                writeLine("Module " + moduleName + " is undefined or empty!", "warning");
            }

            return toReturn;
        }
    }
}