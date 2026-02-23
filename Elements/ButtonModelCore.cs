using DMB.Core;
using DMB.Core.Dmf;
using DMB.Core.Elements;
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
		}

		public override string GetElementNamePrefix() => "Button";

		[Dmf]
		public string Text { get; set; } = "";

		[Dmf]
		public MudBlazor.Color Color { get; set; } = MudBlazor.Color.Primary;

		[Dmf]
		public MudBlazor.Variant Variant { get; set; } = MudBlazor.Variant.Filled;

		[Dmf]
		public virtual string StartIconKey { get; set; } = "";

		[Dmf]
		public virtual string EndIconKey { get; set; } = "";
	}
}
