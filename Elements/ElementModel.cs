using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public abstract class ElementModel : IModuleItem
	{
		private ModuleStateCore _moduleState;
		private string _id = "";

		protected ElementModel(ModuleStateCore moduleState)
		{
			this._moduleState = moduleState;
			this.Id = moduleState.GenerateNextElementId(this);
			this.Visible = new ExpressionablePropertyCore<bool>() { Value = true };
        }

		[Browsable(false)]
		public ModuleStateCore ModuleStateCore => this._moduleState;

		[Dmf]
		public string Id
		{
			get => _id;
			set
			{
				var (ok, error) = this._moduleState.CanSetItemId(this, value);
				if (!ok)
				{
					throw new Exception(error);
				}

				var oldId = this._id;
				this._id = value!.Trim();
				this._moduleState.RaiseItemIdChanged(this, oldId, this._id);
			}
		}

		[Dmf]
        [Expression]
        [System.ComponentModel.Category("Appearance")]
        public virtual string? Class { get; set; }

		[Dmf]
        [Expression]
        [System.ComponentModel.Category("Appearance")]
        public virtual string? Style { get; set; }

		[Browsable(false)]
		public CellModelCore? ParentCell { get; set; }

		public virtual string GetElementNamePrefix() => throw new NotImplementedException();

		[Dmf]
		[ExpandableProperty]
		public virtual IExpressionablePropertyCore<bool> Visible { get; set; } = default!;
    }

	public enum ContentAlignment
	{
		Start,
		Center,
		End,
		Stretch
	}

	public sealed class SelectItem
	{
		public string Value { get; set; } = "";
		public string Text { get; set; } = "";
	}
}
