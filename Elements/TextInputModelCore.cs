using DMB.Core.Dmf;
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
		}

		public override string GetElementNamePrefix() => "TextInput";

		[Dmf]
		public virtual string Label { get; set; } = "Text Input";


		[Dmf]
		public virtual TextAlign TextAlign { get; set; } = TextAlign.Start;

		[Dmf]
		public virtual MudBlazor.Color Color { get; set; } = MudBlazor.Color.Default;

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
    }
}
