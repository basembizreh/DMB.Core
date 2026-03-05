using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            this.ToolBarGrid = new GridModelCore(this.ModuleStateCore);
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

        [Dmf]
        public virtual bool HasToolbar { get; set; }

        [Dmf]
        public virtual bool Hover { get; set; } = true;

        [Dmf]
        public virtual bool Dense { get; set; } = false;

        [Dmf]
        public virtual bool Striped { get; set; } = false;

        [Dmf]
        public virtual bool Bordered { get; set; } = false;

        [Dmf]
        [DefaultValue(10)]
        public virtual int RowsPerPage { get; set; } = 10;

        public override string GetElementNamePrefix() => "DataGrid";

        protected override void OnIdChanged(string oldId, string newId)
        {
            base.OnIdChanged(oldId, newId);
            foreach (var column in this.Columns)
            {
                column.DataGridId = newId;
            }
        }

        //[DmfChildren()]
        public virtual GridModelCore ToolBarGrid { get; set; }
    }
}
