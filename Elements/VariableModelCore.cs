using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class VariableModelCore : IModuleItem, IValueElement
	{
		private readonly ModuleDocumentCore _moduleDocument;
		private string _id = "";

		public VariableModelCore(ModuleDocumentCore moduleDocument)
		{
			this._moduleDocument = moduleDocument;
		}

		[Browsable(false)]
		public ModuleDocumentCore ModuleDocument => this._moduleDocument;

        [Dmf]
		public string Name
		{
			get { return this.Id; }
			set { this.Id = value; }
		}

		[Dmf]
		public virtual object? Value { get; set; } = "";

		public virtual string Id
		{
			get => this._id;
			set
			{
				var (ok, error) = this._moduleDocument.CanSetItemId(this, value);
				if (!ok)
				{
					throw new Exception(error);
				}

				this._id = value!.Trim();
			}
		}

		public string GetElementNamePrefix() => "Variable";

        [Dmf]
		[DefaultValue(VariableDataType.String)]
        public virtual VariableDataType DataType { get; set; } = VariableDataType.String;
	}

    public enum VariableDataType
    {
        String,
        Integer,
        Float,
        DateOnly,
        DateTime,
        TimeOnly,
        Boolean,
        Object
    }
}
