using DMB.Core.Dmf;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class TextInputModelCore : ElementModel, IInputField
    {
        private string? ValueText;

        public TextInputModelCore(ModuleStateCore moduleState)
			: base(moduleState)
		{
			this.Label = this.Id;
			this.Color = new ExpressionablePropertyCore<MudBlazor.Color>() { Value = MudBlazor.Color.Default };
			this.TextAlign = new ExpressionablePropertyCore<Align>() { Value = Align.Start };
        }

		public override string GetElementNamePrefix() => "TextInput";

		[Dmf]
		public virtual string Label { get; set; } = "Text Input";

		[Dmf]
		[ExpandableProperty]
		public virtual IExpressionablePropertyCore<Align> TextAlign { get; set; } = default!;

		[Dmf]
		[ExpandableProperty]
		public virtual IExpressionablePropertyCore<MudBlazor.Color> Color { get; set; } = default!;

		[Dmf]
		public virtual MudBlazor.Variant Variant { get; set; } = MudBlazor.Variant.Outlined;

		[Dmf]
		public virtual MudBlazor.Margin Margin { get; set; } = MudBlazor.Margin.Normal;

        [Dmf]
        public virtual string? Value 
		{
			get => this.ValueText;
            set => this.ValueText = (string?)value; 
		}

		[Browsable(false)]
        public Type ValueType => typeof(string);

        [Dmf]
		public virtual int DebounceInterval { get; set; } = 0;
    }
}
