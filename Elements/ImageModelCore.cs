using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class ImageModelCore : ElementModel
    {
        public ImageModelCore(ModuleStateCore moduleState) : base(moduleState) { }

        public override string GetElementNamePrefix() => "Image";

        [Dmf]
        public virtual string Src { get; set; } = "images/placeholder.png";

        [Dmf]
        public virtual string Alt { get; set; } = "Image";

        [Dmf]
        public virtual string? Width { get; set; } = "100%";

        [Dmf]
        public virtual string? Height { get; set; } = null;
    }
}
