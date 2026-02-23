using DMB.Core;
using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class TextBlockModelCore(ModuleStateCore moduleState) : ElementModel(moduleState)
	{
		public override string GetElementNamePrefix() => "Text";

		[Dmf]
		public virtual string? Text { get; set; } = "Text";

		[Dmf]
		public virtual MudBlazor.Typo Typo { get; set; } = MudBlazor.Typo.body1;

		[Dmf]
		public virtual TextAlign TextAlign { get; set; } = TextAlign.Start;

		[Dmf]
		public virtual MudBlazor.Color Color { get; set; } = MudBlazor.Color.Default;
	}
}
