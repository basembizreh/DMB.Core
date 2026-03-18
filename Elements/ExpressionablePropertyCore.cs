using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class ExpressionablePropertyCore<T> : IExpressionablePropertyCore, IHasOwner
    {
        [Dmf]
        public virtual ValueMode ValueMode { get; set; } = ValueMode.Literal;

        [Dmf]
        public virtual string? Expression { get; set; }

        [Dmf]
        public virtual T Value { get; set; } = default!;

        [Browsable(false)]
        public object? Owner { get; set; }
    }
}
