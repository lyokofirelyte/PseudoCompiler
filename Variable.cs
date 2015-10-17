using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoCompiler
{
    public class Variable
    {

        private string name;
        private string value;
        private string module;
        private Variable linkedTo;
        private string uuid;

        public Variable(string name, string value, string module)
        {
            Name = name;
            Value = value;
            Module = module;
            uuid = Guid.NewGuid().ToString();
        }
        
        public bool hasReference()
        {
            return linkedTo != null;
        }

        public void addReference(Variable var)
        {
            linkedTo = var;
        }

        public void removeReference()
        {
            linkedTo = null;
        }

        public bool hasReference(string module)
        {
            return linkedTo != null && linkedTo.Module.Equals(module);
        }

        public bool isExactMatch(string module, string name)
        {
            return Module.Equals(module) && Name.Equals(name);
        }

        public Variable getReference()
        {
            return linkedTo;
        }

        public string Name
        {
            get { return name; }
            set { this.name = value; }
        }

        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public string Module
        {
            get { return module; }
            set { this.module = value; }
        }
    }
}
