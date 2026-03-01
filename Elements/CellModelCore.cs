using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
    public class CellModelCore : ElementModel
    {
        private readonly RowModelCore _parentRow;

        public CellModelCore(ModuleStateCore moduleState, RowModelCore parentRow)
            : base(moduleState)
        {
            this._parentRow = parentRow;
            this.HorizontalAlignment = new ExpressionablePropertyCore<ContentAlignment>() { Value = ContentAlignment.Start };
            this.VerticalAlignment = new ExpressionablePropertyCore<ContentAlignment>() { Value = ContentAlignment.Start };
        }

        [Browsable(false)]
        public ElementModel? Element { get; set; }

        [ReadOnly(true)]
        [Dmf]
        public int ColSpan { get; set; } = 1;

        public int CellIndex
        {
            get
            {
                return this.Row?.Cells.IndexOf(this) ?? -1;
            }
        }

        [Dmf]
        [DefaultValue(ContentAlignment.Start)]
        [ExpandableProperty]
        public virtual IExpressionablePropertyCore<ContentAlignment> HorizontalAlignment { get; set; } = default!;

        [Dmf]
        [ExpandableProperty]
        public virtual IExpressionablePropertyCore<ContentAlignment> VerticalAlignment { get; set; } = default!;

        [Browsable(false)]
        public RowModelCore Row { get => this._parentRow; }

        public override string GetElementNamePrefix() => "Cell";

        [Browsable(false)]
        public override IExpressionablePropertyCore<bool> Visible { get => base.Visible; }
    }
}
