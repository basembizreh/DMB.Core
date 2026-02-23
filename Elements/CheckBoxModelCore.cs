using DMB.Core;
using DMB.Core.Dmf;
using DMB.Core.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class CheckBoxModelCore : ElementModel
    {
        public CheckBoxModelCore(ModuleStateCore moduleState) : base(moduleState) { }

        public override string GetElementNamePrefix() => "Checkbox";

        [Dmf]
        public string Label { get; set; } = "Checkbox";

        [Dmf]
        public bool Checked { get; set; } = false;

        [Dmf]
        public MudBlazor.Color Color { get; set; } = MudBlazor.Color.Default;

        [Dmf]
        public bool Disabled { get; set; } = false;

        [Dmf]
        public bool ReadOnly { get; set; } = true; // preview safe
    }
}
