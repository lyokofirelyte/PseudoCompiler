using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.JScript;

/**
 * I used http://odetocode.com/articles/80.aspx to create this class for easier evaluation.
**/

namespace PseudoCompiler
{
    public class FullEval
    {
        public static string EvalToString(string statement)
        {
            object o = _evaluatorType.InvokeMember(
                        "Eval",
                        BindingFlags.InvokeMethod,
                        null,
                        _evaluator,
                        new object[] { statement }
                     );
            return o.ToString();
        }

        static FullEval()
        {
            ICodeCompiler compiler = new JScriptCodeProvider().CreateCompiler();

            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;

            CompilerResults results = compiler.CompileAssemblyFromSource(parameters, _jscriptSource);

            Assembly assembly = results.CompiledAssembly;
            _evaluatorType = assembly.GetType("Evaluator.Evaluator");
            _evaluator = Activator.CreateInstance(_evaluatorType);
        }

        private static object _evaluator = null;
        private static Type _evaluatorType = null;
        private static readonly string _jscriptSource =

            @"package Evaluator
            {
               class Evaluator
               {
                  public function Eval(expr : String) : String 
                  { 
                     return eval(expr); 
                  }
               }
            }";
    }
}