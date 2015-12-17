using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;

namespace PCGUI
{
    class PseudoMain
    {
        private GenUtils utils;
        public Form1 form;

        public string[] correctedText;
        public string[] text;
        public string[] commands = new string[]
        {
            "run | run | Run your pseudo code from the file you provided.",
            "reload file | reload | Reload the current file if you made changes.",
            "view example | example | View an example I drew up for the class.",
            "about | about | What is this program? Who made it?",
            "debug mode | debug | Show some extra information about your modules.",
            "settings | settings | Change some cool settings like color and manage updates.",
            "open new file | open | Change the file without closing the program.",
            "view root files | root | View the data the compiler stores on your computer.",
            "view source code & changes | source | View the awesome source code for this project!",
            "dev console | dev | For those hidden commands no one cares about.",
            "exit | exit | I wonder what this does?"
        };

        public Dictionary<int, int> lineSave = new Dictionary<int, int>();
        public Dictionary<string, int> currentLine = new Dictionary<string, int>();

        public bool debug = false;
        public static bool allowSettings = false;
        public static bool first = false;
        public static bool isInit = false;

        public int globalIndex = 0;
        public int globalTop = 0;

        public string toCompile = "";
        public string user = Environment.UserName;
        public string name = "";
        public string cDir = "";
        public string globalChoice = "";
        public static string latestChangeHardCodedBecauseLazy = "(12/16/15) Removed gift code event (completed)";
        private static string settingsDirectory = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/";
        private static string settingsFile = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/settings.pseudo";
        private static string csFile = "C:/Users/" + Environment.UserName + "/AppData/Roaming/PseudoCompiler/compile/cs.pseudo";
        public static string version = "3.1.2";

        public Process proc;

        private string[] getCommandInfo(int i)
        {
            return Regex.Split(commands[i], @" \| ");
        }

        public PseudoMain(Form1 form)
        {
            this.form = form;
        }

        delegate void codeDone(Form1 form);

        public void doneCode()
        {
            form.Invoke(new codeDone(actuallyDone), form);
        }

        public void actuallyDone(Form1 form)
        {
            form.writeToConsole("--------------------", Color.Magenta);
            form.writeToConsole("Your program has finished.", Color.Magenta);
            form.finish();
        }

        public void init()
        {
            PseudoMain.first = false;
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
                }
            }

            isInit = true;
        }

        public bool start(string fileName)
        {
            lineSave = new Dictionary<int, int>();
            text = File.ReadAllLines(fileName);
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

            if (text.Length < 2)
            {
                MessageBox.Show("There isn't enough code in the file to run.");
                Environment.Exit(0);
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

            foreach (string s in new string[]
            {
                "using System; using System.IO; using System.Collections.Generic; using System.Windows.Forms; using System.Drawing;",

                "namespace PCGUI",
                "{",

                    "public class DynaCore",
                    "{",

                       "private Dictionary<string, int> gindex = new Dictionary<string, int>();",
                       "private Dictionary<string, int> fileSize = new Dictionary<string, int>();",
                       "private List<string> appendModes = new List<string>();",
                       "private string PSEUDO_APP_NAME = @\"" + name + "\";",
                       "private string PSEUDO_APP_PATH = @\"" + Application.ExecutablePath + "\";",
                       "private static bool go = false;",
                       "public RichTextBox form;",
                       "public RichTextBox input;",

                        "public static void Main(string str, RichTextBox form, RichTextBox input)",
                        "{",
                            "DynaCore c = new DynaCore();",
                            "input.KeyDown += (a, b) =>",
                            "{",
                                "if (b.KeyCode == Keys.Enter)",
                                "{",
                                    "b.SuppressKeyPress = true;",
                                    "go = true;",
                                    "b.Handled = true;",
                                "}",
                            "};",
                            "c.form = form;",
                            "c.input = input;",
                            "c.main();",
                        "}",

                        "public void writeToConsole(string text)",
                        "{",
                            "writeToConsole(text, System.Drawing.Color.White);",
                        "}",

                        "delegate void SetTextCallback(string text, System.Drawing.Color color);",
                        "delegate string AccessInputCallback();",
                        "delegate void ClearFormInputCallback();",

                        "public void clearFormInput(){ form.Parent.Parent.Invoke(new ClearFormInputCallback(cfinput)); }",
                        "public void cfinput(){ input.Clear(); }",

                        "private void SetText(string text)",
                        "{",
                            "writeToConsole(text);",
                        "}",

                        "public void writeToConsole(string text, System.Drawing.Color color)",
                        "{",
                            "if (form.InvokeRequired)",
                            "{",
                                "SetTextCallback d = new SetTextCallback(writeToConsole);",
                                "form.Parent.Parent.Invoke(d, new object[] { text, color });",
                            "}",
                            "else",
                            "{",
                                "RichTextBox c = form;",
                                "c.SelectionStart = c.TextLength;",
                                "c.SelectionLength = 0;",

                                "c.SelectionColor = color;",
                                "c.AppendText(text + \"\\n\");",
                                "c.SelectionColor = c.ForeColor;",
                                "c.ScrollToCaret();",
                            "}",
                        "}",

                        "public void cWrite(string msg)",
                        "{",
                        "    SetText(msg);",
                        "}",

                        "public string accessInput(){ string i = input.Text; input.Clear(); return i; }",

                        "public string cRead()",
                        "{",
                            "while (!go){};",
                            "go = false;",
                            "string i = (string) form.Parent.Parent.Invoke(new AccessInputCallback(accessInput));",
                        "    return i;",
                        "}",

                        "protected int inputAsInteger(){",

                            "cWrite(\"> \");",
                            "int input = 0;",

                            "try { ",
                                "input = int.Parse(cRead()); ",
                           " } catch (Exception){ ",
                                "cWrite(\"Can't convert input to an integer - using 0 instead!\"); ",
                            "}",

                            "return input;",
                        "}",

                        "protected float inputAsReal(){",

                            "cWrite(\"> \");",
                            "float input = 0;",

                            "try { ",
                                "input = float.Parse(cRead()); ",
                           " } catch (Exception){ ",
                                "cWrite(\"Can't convert input to a real - using 0 instead!\"); ",
                            "}",

                            "return input;",
                        "}",

                        "protected bool inputAsBoolean(){",

                            "cWrite(\"> \");",
                            "bool input = false;",

                            "try { ",
                                "input = Boolean.Parse(cRead()); ",
                           " } catch (Exception){ ",
                                "cWrite(\"Can't convert input to a boolean - using false instead!\"); ",
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

                        "protected float squareRoot(object input){",
                            "return float.Parse(Math.Sqrt(double.Parse(input.ToString())).ToString());",
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
                text[i] = text[i].Replace(" Real ", " float ");
                text[i] = text[i].Replace(" real ", " float ");
                text[i] = text[i].Replace("Real ", "float ");
                text[i] = text[i].Replace("real ", "float ");
                text[i] = text[i].Replace(" Real", " float");
                text[i] = text[i].Replace(" real", " float");
                text[i] = text[i].Replace("Console.ReadLine()", "cRead()");
                text[i] = text[i].Replace("Console.WriteLine(", "cWrite(");

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
                        text[i] = "cWrite((" + (debug ? "new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name + ' ' + '>' + ' ' + " : "\"> \" + ") + text[i] + ").ToString());";
                        text[i] = replaceCommas(text[i]);

                        break;

                    case "input":

                        string header = "> ";
                        text[i] = "cWrite(" + '"' + header + '"' + ");" + args[1] + " = cRead();";
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

                        text[i] = "clearFormInput();";

                        break;

                    case "close":

                        text[i] = "try {";
                        text[i] += "gindex.Remove(" + args[1] + ");";
                        text[i] += "fileSize.Remove(" + args[1] + ");";
                        text[i] += "} catch (Exception){}";

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

            form.writeToConsole(arrows, Color.White);
            form.writeToConsole(phrase);
            form.writeToConsole(arrows + "\n");

            try
            {
               // if (getSetting("update").ToLower().Equals("automatic"))
               // {
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
                    form.clearConsole();
                    writeLine("A new version (" + version + ") is released.", "system");
                    writeLine("You're only running version " + PseudoMain.version + ".", "system");
                    writeLine("Automatically updating...", "system");
                    WebClient wc = new WebClient();

                    wc.DownloadProgressChanged += (a, b) =>
                    {
                       form.writeToConsole("\r" + b.ProgressPercentage.ToString() + "%");
                    };

                    wc.DownloadFileCompleted += (a, b) =>
                    {
                        writeLine("Download Complete. Check the current directory for the new file.", "system");
                        form.writeToConsole("Please exit and open the new version.", Color.Red);
                    };

                    wc.DownloadFileAsync(new Uri("https://github.com/lyokofirelyte/PseudoCompiler/blob/master/PseudoCompiler.exe?raw=true"), "PseudoCompiler v" + version + ".exe");
                    return false;
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

                    writeLine("", "system");
                    form.writeToConsole("Please report bugs to dtossber@purduecal.edu - thanks!", Color.Magenta);
                }
            }
            catch (Exception)
            {
                writeLine("Update check failed. No internet or github is down.", "error");
            }

            return true;
        }

        public void manageInputs(string str, string fileName)
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

                        form.writeToConsole("There's a button to do this, are you trying to be cool & hip with your command lines?");

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
                            writeLine("Error opening file browser! (How did this fail? Are you on XP or something?)", "error");
                        }

                        break;

                    case "reload":

                        form.writeToConsole("There's a button to do this, are you trying to be cool & hip with your command lines?");

                        return;

                    case "changelog":

                        Process.Start("https://github.com/lyokofirelyte/PseudoCompiler/commits/master");

                        break;

                    case "run":

                        form.clearConsole();
                        form.Invoke(new StartCode(startPseudoCode));

                        break;

                    case "source":

                        Process.Start("https://github.com/lyokofirelyte/PseudoCompiler");
                        Process.Start("https://github.com/lyokofirelyte/PseudoCompiler/blob/master/Program.cs");

                        break;

                    case "example":


                        WebClient wc = new WebClient();

                        wc.DownloadProgressChanged += (a, b) =>
                        {
                            form.writeToConsole("\r" + b.ProgressPercentage.ToString() + " ");
                        };

                        wc.DownloadFileCompleted += (a, b) =>
                        {
                            writeLine("Download Complete. The example.txt is in this directory somewhere.", "system");
                            Process.Start("example.txt");
                        };

                        wc.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/lyokofirelyte/PseudoCompiler/master/example.txt"), "example.txt");

                        break;

                    case "help":

                        form.clearConsole();

                        foreach (string s in new string[]
                        {
                                "> Pseudo Compiler Commands <",
                                "--- --- --- --- --- --- ---",
                                "",
                                "run (runs the program)",
                                "example (displays example pseudo code)",
                                "about (what even is this?)",
                                "reload (reloads the text file for a fresh start)",
                                "open (open a new file to run)",
                                "root (view program files)",
                                "newinstance (open a new instance in another window)",
                                "source (view source code on github)",
                                "changelog (view a poorly described list of changes)",
                                "exit (exits the program)",
                                "",
                                "--- --- --- --- --- --- ---",
                                "Most Windows CMD commands will also work, such as dir, ipconfig, ping, etc."
                        })
                        {
                            form.writeToConsole(s);
                        }

                        break;

                    case "about":

                        // form.clearConsole();

                        foreach (string s in new string[]
                        {
                                 "--- --- --- --- --- --- ---",
                                 "Greetings! I'm David. I wrote this thing.",
                                 "This program converts your pseudo code into C# code, compiles, and runs.",
                                 "I hope you enjoy!",
                                 "--- --- --- --- --- --- ---",
                        })
                        {
                            form.writeToConsole(s, Color.White);
                        }

                        break;

                    case "newinstance":

                        Process.Start(new ProcessStartInfo(Application.ExecutablePath));

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
                            form.writeToConsole(proc.StandardOutput.ReadLine());
                        }

                        break;
                }
            }
        }

        delegate void StartCode();

        // wow this actually worked, go me!
        public void startPseudoCode()
        {
            new System.Threading.Thread(() =>
            {
                form.runThread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        utils.CompileAndRun(new string[] { toCompile });
                    }
                    catch (Exception) { }
                });

                form.runThread.Start();

                while (form.runThread.IsAlive) { } // why (a)wait when you can be lazy now! (wait, what?)

                doneCode();
            }).Start();
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
            form.writeToConsole(message);
        }
    }
}