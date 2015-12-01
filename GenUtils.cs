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
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace PseudoCompiler
{
    class GenUtils
    {

        private PseudoMain main;

        public GenUtils(PseudoMain instance)
        {
            this.main = instance;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(UInt32 uAction, int uParam, string lpvParam, int fuWinIni);

        public partial class FancyLabel : Label
        {
            public FancyLabel()
            {
                SetStyle(ControlStyles.Selectable, true);
                SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }
        }

        public void CompileAndRun(string[] code)
        {
            CompilerParameters CompilerParams = new CompilerParameters();
            string outputDirectory = Directory.GetCurrentDirectory();

            CompilerParams.GenerateInMemory = true;
            CompilerParams.TreatWarningsAsErrors = false;
            CompilerParams.GenerateExecutable = false;
            CompilerParams.CompilerOptions = "/optimize";

            string[] references = { "System.dll" };
            CompilerParams.ReferencedAssemblies.AddRange(references);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults compile = provider.CompileAssemblyFromSource(CompilerParams, code);

            if (compile.Errors.HasErrors)
            {
                Console.WriteLine("There is an issue with your code. Here's what happened:");
                string error = compile.Errors[compile.Errors.Count - 1].ToString().Substring(compile.Errors[compile.Errors.Count - 1].ToString().IndexOf('('));
                Console.WriteLine(error);
                int number = int.Parse(error.Split(',')[0].Substring(1));
                Console.WriteLine("" + number);
                Console.WriteLine("This is somewhere in and around the below area... ish.");
                Console.ForegroundColor = ConsoleColor.Red;
                try
                {
                    Console.WriteLine(findLine(number));
                }
                catch (Exception)
                {
                    Console.WriteLine("Botched error parsing - can't find where the error is in the pseudo code.");
                }

                Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), PseudoMain.setting["fcolor"], true);
                Console.WriteLine();
                return;
            }

            System.Reflection.Module module = compile.CompiledAssembly.GetModules()[0];
            Type mt = null;
            MethodInfo methInfo = null;

            if (module != null)
            {
                mt = module.GetType("DynaCore.DynaCore");
            }

            if (mt != null)
            {
                methInfo = mt.GetMethod("Main");
            }

            if (methInfo != null)
            {
                try
                {
                    methInfo.Invoke(null, new object[] { "Test" });
                }
                catch (Exception ee)
                {
                    Console.WriteLine("Unexpected error! Here's the stacktrace:\n" + ee.Message + "\n" + ee.StackTrace);
                }
            }
        }

        public string findLine(int line)
        {

            int csline = main.lineSave[line];
            string first = "";

            if (csline - 2 >= 0)
            {
                first += main.correctedText[csline - 2];
            }

            if (csline - 1 >= 0)
            {
                first += "\n" + main.correctedText[csline - 1];
            }


            first += (!first.Equals("") ? "\n" : "") + main.correctedText[csline];

            if (main.correctedText.Length > csline + 1)
            {
                first += "\n" + main.correctedText[csline + 1];
            }

            if (main.correctedText.Length > csline + 2)
            {
                first += "\n" + main.correctedText[csline + 2];
            }

            return first;
        }

        public static ColorRGB HSL2RGB(double h, double sl, double l)
        {
            double v;
            double r, g, b;

            r = l;   // default to gray
            g = l;
            b = l;
            v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);

            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;

                m = l + l - v;
                sv = (v - m) / v;
                h *= 6.0;
                sextant = (int)h;
                fract = h - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;

                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }

            ColorRGB rgb;
            rgb.R = Convert.ToByte(r * 255.0f);
            rgb.G = Convert.ToByte(g * 255.0f);
            rgb.B = Convert.ToByte(b * 255.0f);

            return rgb;
        }

        public struct ColorRGB
        {
            public byte R;
            public byte G;
            public byte B;

            public ColorRGB(Color value)
            {
                this.R = value.R;
                this.G = value.G;
                this.B = value.B;
            }

            public static implicit operator Color(ColorRGB rgb)
            {
                Color c = Color.FromArgb(rgb.R, rgb.G, rgb.B);
                return c;
            }

            public static explicit operator ColorRGB(Color c)
            {
                return new ColorRGB(c);
            }
        }
    }
}
