using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class ExpressionablePropertyCore<T> : IExpressionablePropertyCore<T>
    {
        [Dmf]
        public ValueMode ValueMode { get; set; } = ValueMode.Literal;

        [Dmf]
        public string? Expression { get; set; }

        [Dmf]
        public T Value { get; set; } = default!;
    }
}
