using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public interface IExpressionablePropertyCore
    {
        object? Owner { get; set; }

        //ValueMode ValueMode { get; set; }

        //string? Expression { get; set; }

        //T Value { get; set; }
    }

    public enum ValueMode
    {
        Expression,
		Literal
	}
}
