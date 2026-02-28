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
    public class TextBlockModelCore : ElementModel
    {
        public TextBlockModelCore(ModuleStateCore moduleState)
            : base(moduleState)
        {
            this.TextAlign = new ExpressionablePropertyCore<MudBlazor.Align>();
            this.Color = new ExpressionablePropertyCore<MudBlazor.Color>();
        }

        public override string GetElementNamePrefix() => "Text";

        [Dmf]
        [Expression]
        public virtual string? Text { get; set; } = "Text";

        [Dmf]
        public virtual MudBlazor.Typo Typo { get; set; } = MudBlazor.Typo.body1;

        [Dmf]
        [ExpandableProperty]
        public virtual IExpressionablePropertyCore<MudBlazor.Align> TextAlign { get; set; } = default!;

        [Dmf]
		[ExpandableProperty]
		public virtual IExpressionablePropertyCore<MudBlazor.Color> Color { get; set; } = default!;
    }
}