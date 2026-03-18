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
        private GridModelCore? _toolbarGrid;
        private string? _dataset;

        public DataGridModelCore(ModuleDocumentCore moduleDocument)
            : base(moduleDocument)
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

        protected virtual GridModelCore InstantiateAndRegisterToolbarGrid()
        {
            var grid = new GridModelCore(this.ModuleDocumentCore);
            grid.Id = $"{this.Id}_ToolBarGrid";
            var row = new RowModelCore(this.ModuleDocumentCore);
            row.Id = $"{grid.Id}_Row";
            var cell = new CellModelCore(this.ModuleDocumentCore, row);
            cell.Id = $"{row.Id}_Cell";
            row.Cells.Add(cell);
            grid.Rows.Add(row);
            this.ModuleDocumentCore.Register(grid, false);

            return grid;
        }

        [DmfChildren("Columns", "Column")]
        public virtual DataGridColumnsCollection<T> Columns
        {
            get
            {
                if (this._columns is null)
                {
                    this._columns = new DataGridColumnsCollection<T>(this.ModuleDocumentCore, this.Id);
                }
                return this._columns;
            }
        }

        [Dmf]
        public virtual bool ShowToolbar { get; set; }

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
            this.Columns.DataGridId = newId;
        }


        // Treat toolbar as a Dmf child; factory method should instantiate and register the grid
        [DmfChild("ToolBarGrid")]
        public virtual GridModelCore? ToolBarGrid 
        {
            get
            {
                if (!this.ModuleDocumentCore.IsLoading && this._toolbarGrid is null)
                {
                    this._toolbarGrid = this.InstantiateAndRegisterToolbarGrid();
                    this._toolbarGrid.Owner = this;
                }
                return this._toolbarGrid;
            }
            set
            {
                this._toolbarGrid = value;
                if (this._toolbarGrid != null && this._toolbarGrid.Owner is null)
                {
                    this._toolbarGrid.Owner = this;
                }
            }
        }
    }
}
