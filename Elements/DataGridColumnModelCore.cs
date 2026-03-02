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
            this.HeaderText = new ExpressionablePropertyCore<string>() { Value = string.Empty };
            this.HeaderClass = new ExpressionablePropertyCore<string>() { Value = string.Empty };
            this.HeaderStyle = new ExpressionablePropertyCore<string>() { Value = string.Empty };
            this.CellClass = new ExpressionablePropertyCore<string>() { Value = string.Empty };
            this.CellStyle = new ExpressionablePropertyCore<string>() { Value = string.Empty };
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
        public virtual IExpressionablePropertyCore<string> HeaderText { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual IExpressionablePropertyCore<string> HeaderClass { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual IExpressionablePropertyCore<string> HeaderStyle { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual IExpressionablePropertyCore<string> CellClass { get; set; } = default!;

        [ExpandableProperty]
        [Dmf]
        public virtual IExpressionablePropertyCore<string> CellStyle { get; set; } = default!;

        [Dmf]
        public virtual bool EnableFitering { get; set; } = true;

        [Dmf]
        public virtual bool EnableSorting { get; set; } = true;

        [Dmf]
        [ExpandableProperty]
        public virtual IExpressionablePropertyCore<bool> Visible { get; set; } = default!;
    }
}
