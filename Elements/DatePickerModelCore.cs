using DMB.Core;
using DMB.Core.Dmf;
using DMB.Core.Elements;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class DatePickerModelCore : ElementModel, IInputField
    {
        private DateTime? ValueDate;

        public DatePickerModelCore(ModuleDocumentCore moduleDocument) : base(moduleDocument) { }

        public override string GetElementNamePrefix() => "DatePicker";

        [Dmf]
        public string DateFormat => "yyyy-MM-dd";

        [Dmf]
        public string Label { get; set; } = "Date";

        [Dmf]
        public MudBlazor.Color Color { get; set; } = MudBlazor.Color.Primary;

        [Dmf]
        public Variant Variant { get; set; } = Variant.Outlined;

        [Dmf]
        public Margin Margin { get; set; } = Margin.None;

        [Dmf]
        public bool Disabled { get; set; } = false;

        [Dmf]
        public virtual object? Value
        {
            get => ValueDate?.ToString(this.DateFormat);
            set => ValueDate = value == null ? null : DateTime.ParseExact(value.ToString()!
                , this.DateFormat, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
