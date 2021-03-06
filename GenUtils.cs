﻿using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace PCGUI
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

            string[] references = { "System.dll", "System.Drawing.dll", "System.Windows.Forms.dll" };
            CompilerParams.ReferencedAssemblies.AddRange(references);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults compile = provider.CompileAssemblyFromSource(CompilerParams, code);

            if (compile.Errors.HasErrors)
            {
                main.form.writeToConsole("There is an issue with your code. Here's what happened:", Color.Red);
                string error = compile.Errors[compile.Errors.Count - 1].ToString().Substring(compile.Errors[compile.Errors.Count - 1].ToString().IndexOf('('));
                main.form.writeToConsole(error, Color.Red);
                int number = int.Parse(error.Split(',')[0].Substring(1));
                main.form.writeToConsole("" + number, Color.Red);
                main.form.writeToConsole("This is somewhere in and around the below area... ish.", Color.Red);
                try
                {
                    main.form.writeToConsole(findLine(number), Color.DarkRed);
                }
                catch (Exception)
                {
                    main.form.writeToConsole("Botched error parsing - can't find where the error is in the pseudo code.", Color.DarkRed);
                }

                return;
            }

            Module module = compile.CompiledAssembly.GetModules()[0];
            Type mt = null;
            MethodInfo methInfo = null;

            if (module != null)
            {
                mt = module.GetType("PCGUI.DynaCore");
            }

            if (mt != null)
            {
                methInfo = mt.GetMethod("Main");
            }

            if (methInfo != null)
            {
                try
                {
                    methInfo.Invoke(null, new object[] { "Test", main.form.getTBox("console"), main.form.getTBox("input") });
                }
                catch (Exception ee)
                {
                    if (!ee.Message.Contains("Thread was being aborted"))
                    {
                        main.form.writeToConsole("Unexpected error! Here's the stacktrace:\n" + ee.Message + "\n" + ee.StackTrace + "\n" + ee.InnerException, Color.Red);
                    }
                    else
                    {
                        main.form.writeToConsole("Program terminated early!", Color.Red);
                    }
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