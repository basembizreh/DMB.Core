using DMB.Core;
using DMB.Core.Dmf;
using DMB.Core.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class GridModelCore(ModuleDocumentCore moduleDocument) :
		ElementModel(moduleDocument), IHasOwner
    {
		[Browsable(false)]
		[ChildElements]
        public List<RowModelCore> Rows { get; set; } = new();

		public override string GetElementNamePrefix() => "Grid";

        public ElementModel? Parent { get; set; }

        [Browsable(false)]
        public object? Owner { get; set; }
    }
}
