using DMB.Core;
using DMB.Core.Dmf;
using DMB.Core.Elements;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class ButtonModelCore : ElementModel
	{
		public ButtonModelCore(ModuleStateCore moduleState)
			: base(moduleState)
		{
			this.Text = this.Id;
			this.Color = new ExpressionablePropertyCore<Color>() { Value = MudBlazor.Color.Default };
		}

		public override string GetElementNamePrefix() => "Button";

		[Dmf]
		public virtual string Text { get; set; } = "";

		[Dmf]
		[ExpandableProperty]
		public virtual IExpressionablePropertyCore<Color> Color { get; set; } = default!;

		[Dmf]
		public virtual MudBlazor.Variant Variant { get; set; } = MudBlazor.Variant.Filled;

		[Dmf]
		public virtual string StartIconKey { get; set; } = "";

		[Dmf]
		public virtual string EndIconKey { get; set; } = "";
	}
}
