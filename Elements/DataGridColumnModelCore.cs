using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class DataGridColumnModelCore : IDatasetBound
    {
        private string? dataGridId;

        public Action? DataGridIdChanged;

        public DataGridColumnModelCore()
        {
            this.Visible = new ExpressionablePropertyCore<bool>() { Value = true };
            this.HeaderText = new ExpressionablePropertyCore<string?>();
            this.HeaderClass = new ExpressionablePropertyCore<string?>();
            this.HeaderStyle = new ExpressionablePropertyCore<string?>();
            this.CellClass = new ExpressionablePropertyCore<string?>();
            this.CellStyle = new ExpressionablePropertyCore<string?>();
            this.Format = new ExpressionablePropertyCore<string?>();

            this.CellTemplate = new CellContentTemplateCore();
        }

        [Dmf]
        public virtual string? Field { get; set; }

        [Browsable(false)]
        public ModuleDocumentCore ModuleDocumentCore { get; set; } = default!;

        [Browsable(false)]
        public virtual string? DataGridId 
        {
            get { return this.dataGridId; }
            set
            {
                if (this.dataGridId != value)
                {
                    this.dataGridId = value;
                    this.RaiseDataGridIdChanged();
                }
            } 
        }

        [Browsable(false)]
        public string? Dataset
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.DataGridId)
                    && this.ModuleDocumentCore.TryGetItemById(this.DataGridId, out var item)
                    && item is IDatasetBound dataGrid)
                {
                    return dataGrid.Dataset;
                }
                return null;
            }
            set
            {
                throw new Exception("You canno't assign the dataset for a DataGridColumn!");
            }
        }

        [Dmf]
        [ExpandableProperty]
        public virtual ExpressionablePropertyCore<string?> HeaderText { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual ExpressionablePropertyCore<string?> HeaderClass { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual ExpressionablePropertyCore<string?> HeaderStyle { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual ExpressionablePropertyCore<string?> CellClass { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual ExpressionablePropertyCore<string?> CellStyle { get; set; } = default!;

        [Dmf]
        public virtual bool EnableFitering { get; set; } = true;

        [Dmf]
        public virtual bool EnableSorting { get; set; } = true;

        [Dmf]
        [ExpandableProperty]
        public virtual ExpressionablePropertyCore<bool> Visible { get; set; } = default!;

        [Dmf]
        [ExpandableProperty]
        public virtual ExpressionablePropertyCore<string?> Format { get; set; } = default!;

        [Dmf]
        [ExpandableProperty]
        public virtual ICellContentTemplateCore? CellTemplate { get; set; } = default!;

        public virtual GridModelCore InstantiateAndRegisterCellContentGrid()
        {
            var grid = new GridModelCore(this.ModuleDocumentCore);
            var row = new RowModelCore(this.ModuleDocumentCore);
            var cell = new CellModelCore(this.ModuleDocumentCore, row);
            row.Cells.Add(cell);
            grid.Rows.Add(row);
            this.ModuleDocumentCore.Register(grid);

            return grid;
        }

        protected virtual void RaiseDataGridIdChanged()
        {
            if (!string.IsNullOrWhiteSpace(this.dataGridId) && this.ModuleDocumentCore != null)
            {
                if (this.ModuleDocumentCore.TryGetItemById(this.dataGridId, out var item))
                {
                    if (this.CellTemplate != null && this.CellTemplate.Content != null)
                    {
                        this.CellTemplate.Content.Owner = item;
                    }
                }
            }
            this.DataGridIdChanged?.Invoke();
        }
    }
}
