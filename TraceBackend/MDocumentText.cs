using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceBackend
{
    public partial class MDocumentText : IDocumentElement
    {
        string _txt;
        public string Text { get { return _txt; } set { _txt = value; TextChanged?.Invoke(this, EventArgs.Empty); } }
        public event EventHandler TextChanged;
    }
}
