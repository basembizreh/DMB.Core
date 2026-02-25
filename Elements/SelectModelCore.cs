using DMB.Core;
using DMB.Core.Dmf;
using DMB.Core.Elements;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class SelectModelCore(ModuleStateCore moduleState) : ElementModel(moduleState), IDatasetBound
	{
        public override string GetElementNamePrefix() => "Select";
		private string? _dataset;

        [Dmf]
        public MudBlazor.Origin AnchorOrigin { get; set; } = Origin.BottomCenter;

        [Dmf]
		public virtual string Label { get; set; } = "Select";

		[Dmf]
		public virtual Variant Variant { get; set; } = Variant.Outlined;

		[Dmf]
		public virtual MudBlazor.Color Color { get; set; } = MudBlazor.Color.Default;

		[Dmf]
		public virtual bool Dense { get; set; } = true;

		[Dmf]
		public virtual Margin Margin { get; set; } = Margin.None;

		[Dmf]
		public virtual bool Disabled { get; set; } = false;

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
					this.ValueField = null;
					this.TextField = null;	
                }
			}
		}

        [Dmf]
		public virtual string? ValueField { get; set; }

        [Dmf]
		public virtual string? TextField { get; set; }

		[Dmf]
		public bool EnableFilter { get; set; }
    }
}
