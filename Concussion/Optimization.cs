using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concussion
{
    public class Optimization
    {
        public List<Operation> pattern;
        public List<Operation> replacement;

        public Optimization(Operation[] pattern, Operation[] replacement)
        {
            this.pattern = pattern.ToList();
            this.replacement = replacement.ToList();
        }

        public int Length
        {
            get
            {
                return pattern.Count;
            }
        }

        public int KeepAmount
        {
            get
            {
                return pattern.Count - replacement.Count;
            }
        }
    }
}
