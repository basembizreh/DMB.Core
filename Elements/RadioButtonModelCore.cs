using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class RadioButtonModelCore : ElementModel
    {
        public RadioButtonModelCore(ModuleStateCore moduleState) : base(moduleState) { }

        public override string GetElementNamePrefix() => "Radio";

        [Dmf]
        public virtual string Label { get; set; } = "Radio";

        [Dmf]
        public virtual string Option1 { get; set; } = "Option 1";

        [Dmf]
        public virtual string Option2 { get; set; } = "Option 2";

        [Dmf]
        public virtual string Selected { get; set; } = "Option 1";

        [Dmf]
        public virtual bool Disabled { get; set; } = false;
    }
}
