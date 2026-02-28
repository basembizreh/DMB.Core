using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class CellModelCore(ModuleStateCore moduleState, RowModelCore parentRow)
		: ElementModel(moduleState)
	{
		[Browsable(false)]
		public ElementModel? Element { get; set; }

		[ReadOnly(true)]
		[Dmf]
		public int ColSpan { get; set; } = 1;

		public int CellIndex
		{
			get
			{
				return this.Row?.Cells.IndexOf(this) ?? -1;
			}
		}

        [Dmf]
        [DefaultValue(ContentAlignment.Start)]
        public ContentAlignment HorizontalAlignment { get; set; } = ContentAlignment.Start;

        [Dmf]
        [DefaultValue(ContentAlignment.Start)]
        public ContentAlignment VerticalAlignment { get; set; } = ContentAlignment.Start;

        [Browsable(false)]
		public RowModelCore Row { get; } = parentRow;

		public override string GetElementNamePrefix() => "Cell";

        [Browsable(false)]
        public override IExpressionablePropertyCore<bool>? Visible { get => base.Visible;  }
	}
}
