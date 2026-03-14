using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
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

		public ModuleDocumentCore ModuleState => this._moduleDocument;

        [Dmf]
		public string Name
		{
			get { return this.Id; }
			set { this.Id = value; }
		}

		[Dmf]
		public virtual string? Value { get; set; } = "";

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
	}
}
