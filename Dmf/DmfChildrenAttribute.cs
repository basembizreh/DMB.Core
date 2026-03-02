using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Dmf
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DmfChildrenAttribute : Attribute
    {
        public string ContainerName { get; }
        public string ItemName { get; }

        public DmfChildrenAttribute(string containerName, string itemName)
        {
            ContainerName = containerName;
            ItemName = itemName;
        }
    }
}
