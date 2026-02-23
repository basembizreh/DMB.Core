using DMB.Core.Dmf;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class SwitchModelCore : ElementModel
    {
        public SwitchModelCore(ModuleStateCore moduleState) : base(moduleState) { }

        public override string GetElementNamePrefix() => "Switch";

        [Dmf]
        public virtual string Label { get; set; } = "Switch";

        [Dmf]
        public virtual bool Checked { get; set; } = false;

        [Dmf]
        public virtual Color Color { get; set; } = Color.Primary;

        [Dmf]
        public virtual bool Disabled { get; set; } = false;
    }
}
