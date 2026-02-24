using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public interface IValueElement
    {
        object? Value { get; set; } 
        Type ValueType { get; }
    }
}
