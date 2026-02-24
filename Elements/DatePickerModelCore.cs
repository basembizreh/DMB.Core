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
    public class DatePickerModelCore : ElementModel, IValueElement
    {
        private DateTime? ValueDate;

        public DatePickerModelCore(ModuleStateCore moduleState) : base(moduleState) { }

        public override string GetElementNamePrefix() => "DatePicker";

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
        public object? Value
        {
            get => ValueDate;
            set => ValueDate = (DateTime?)value;
        }

        public Type ValueType => typeof(DateTime?);
    }
}
