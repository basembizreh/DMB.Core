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
        public DataGridColumnModelCore()
        {
            this.Visible = new ExpressionablePropertyCore<bool>() { Value = true };
            this.HeaderText = new ExpressionablePropertyCore<string?>();
            this.HeaderClass = new ExpressionablePropertyCore<string?>();
            this.HeaderStyle = new ExpressionablePropertyCore<string?>();
            this.CellClass = new ExpressionablePropertyCore<string?>();
            this.CellStyle = new ExpressionablePropertyCore<string?>();
            this.Format = new ExpressionablePropertyCore<string?>();
        }

        [Dmf]
        public virtual string? Field { get; set; }

        [Browsable(false)]
        public ModuleStateCore ModuleStateCore { get; set; } = default!;

        [Browsable(false)]
        public virtual string? DataGridId { get; set; }

        [Browsable(false)]
        public string? Dataset
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.DataGridId))
                {
                    var item = this.ModuleStateCore.AllItems
                        .FirstOrDefault(p => p.Id == this.DataGridId);

                    if (item is IDatasetBound dataGrid)
                    {
                        return dataGrid.Dataset;
                    }
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
        public virtual IExpressionablePropertyCore<string?> HeaderText { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual IExpressionablePropertyCore<string?> HeaderClass { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual IExpressionablePropertyCore<string?> HeaderStyle { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual IExpressionablePropertyCore<string?> CellClass { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual IExpressionablePropertyCore<string?> CellStyle { get; set; } = default!;

        [Dmf]
        public virtual bool EnableFitering { get; set; } = true;

        [Dmf]
        public virtual bool EnableSorting { get; set; } = true;

        [Dmf]
        [ExpandableProperty]
        public virtual IExpressionablePropertyCore<bool> Visible { get; set; } = default!;

        [Dmf]
        [ExpandableProperty]
        public virtual IExpressionablePropertyCore<string?> Format { get; set; } = default!;
    }
}
