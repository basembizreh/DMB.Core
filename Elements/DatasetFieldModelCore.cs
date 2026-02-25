using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class DatasetFieldModelCore : IDatasetBound
	{
        [Dmf]
        public virtual string Name { get; set; } = "";

        [Dmf]
        public virtual string Source { get; set; } = "";

        [Browsable(false)]
        public string? Dataset { get; set; }
    }
}
