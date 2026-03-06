using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Dmf
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DmfChildAttribute : Attribute
    {
        public string ElementName { get; }
        public string? FactoryMethodName { get; }

        public DmfChildAttribute(string elementName, string? factoryMethodName = null)
        {
            ElementName = elementName;
            FactoryMethodName = factoryMethodName;
        }
    }
}
