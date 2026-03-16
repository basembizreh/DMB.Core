using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class CellContentTemplateCore : ICellContentTemplateCore
    {
        public CellContentTemplateCore()
        {    
        }

        [Dmf]
        public virtual bool Enabled { get; set; }

        [DmfChild("Content")]
        public virtual GridModelCore? Content { get; set; }
    }
}
