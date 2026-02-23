using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Dmf
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DmfAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DmfNameAttribute : Attribute
    {
        public string Name { get; }
        public DmfNameAttribute(string name) => Name = name;
    }
}
