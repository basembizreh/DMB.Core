using DMB.Core;
using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class TextBlockModelCore(ModuleStateCore moduleState) : ElementModel(moduleState)
	{
		public override string GetElementNamePrefix() => "Text";

		[Dmf]
		[Expression]
		public virtual string? Text { get; set; } = "Text";

		[Dmf]
		public virtual MudBlazor.Typo Typo { get; set; } = MudBlazor.Typo.body1;

		[Dmf]
		public virtual MudBlazor.Align TextAlign { get; set; } = MudBlazor.Align.Start;

		[Dmf]
		[ExpandableProperty]
		public virtual IExpressionablePropertyCore<MudBlazor.Align> TextAlignExpression { get; set; } = default!;

		[Dmf]
		public virtual MudBlazor.Color Color { get; set; } = MudBlazor.Color.Default;
	}

	public interface IExpressionablePropertyCore<T>
	{
		bool Enabled { get; set; }

		string? Expression { get; set; }

		T Value { get; set; }
	}
}