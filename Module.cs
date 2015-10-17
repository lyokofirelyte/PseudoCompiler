using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoCompiler
{
    public class Module
    {

        private string name;
        private List<Variable> vars = new List<Variable>();
        private List<string> lines = new List<string>();

        public Module(string name)
        {
            this.name = name;
        }

        public List<Variable> getVars()
        {
            return vars;
        }

        public List<string> getLines()
        {
            return lines;
        }

        public void addLine(string line)
        {
            lines.Add(line);
        }

        public void setLines(List<string> lines)
        {
            this.lines = lines;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
