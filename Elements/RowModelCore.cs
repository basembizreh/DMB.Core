using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class RowModelCore(ModuleStateCore moduleState) :
		ElementModel(moduleState)
	{
		[Browsable(false)]
		public GridModelCore? ParentGrid { get; set; }

		[Browsable(false)]
        [ChildElements]
        public List<CellModelCore> Cells { get; set; } = new();

		//public int RowIndex { get; set; }
		public int RowIndex 
		{
			get { return this.ParentGrid is null ? -1 : this.ParentGrid.Rows.IndexOf(this); } 
		}

		public override string GetElementNamePrefix() => "Row";
	}
}
