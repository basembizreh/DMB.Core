using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class DataGridModelCore<T> : ElementModel, IDatasetBound
        where T : DataGridColumnModelCore
    {
        private DataGridColumnsCollection<T>? _columns;

        private string? _dataset;

        public DataGridModelCore(ModuleStateCore moduleState)
            : base(moduleState)
        {
        }

        [Dmf]
        public virtual string? Dataset 
        {
            get
            {
                return this._dataset;
            }
            set
            {
                if (this._dataset != value)
                {
                    this._dataset = value;
                }
            }
        }

        [DmfChildren("Columns", "Column")]
        public virtual DataGridColumnsCollection<T> Columns
        {
            get
            {
                if (this._columns is null)
                {
                    this._columns = new DataGridColumnsCollection<T>(this.ModuleStateCore, this.Id);
                }
                return this._columns;
            }
        }

        public override string GetElementNamePrefix() => "DataGrid";
    }
}
