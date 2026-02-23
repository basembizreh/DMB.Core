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
    public class TimePickerModelCore : ElementModel
    {
        public TimePickerModelCore(ModuleStateCore moduleState) : base(moduleState) { }

        public override string GetElementNamePrefix() => "TimePicker";

        [Dmf]
        public virtual string Label { get; set; } = "Time";

        [Dmf]
        public virtual MudBlazor.Color Color { get; set; } = MudBlazor.Color.Primary;

        [Dmf]
        public virtual Variant Variant { get; set; } = Variant.Outlined;

        [Dmf]
        public virtual bool Dense { get; set; } = true;

        [Dmf]
        public virtual Margin Margin { get; set; } = Margin.None;
        [Dmf]
        public virtual bool Disabled { get; set; } = false;

        [Dmf]
        public virtual TimeSpan? Value { get; set; } = null;
    }
}
