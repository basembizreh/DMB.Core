using DMB.Core;
using DMB.Core.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class GridModelCore(ModuleStateCore moduleState) :
		ElementModel(moduleState)
	{
		[Browsable(false)]
		public List<RowModelCore> Rows { get; set; } = new();

		public override string GetElementNamePrefix() => "Grid";
	}
}
