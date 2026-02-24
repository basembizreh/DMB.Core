using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class DatasetModelCore : IModuleItem
	{
		private readonly ModuleStateCore _moduleState;
		private string _id = "";

		public DatasetModelCore(ModuleStateCore moduleState)
		{
			this._moduleState = moduleState;
		}


		[Dmf]
		public virtual string Id
		{
			get => _id;
			set
			{
				var (ok, error) = _moduleState.CanSetItemId(this, value);
				if (!ok)
				{
					throw new Exception(error);
				}

				_id = value!.Trim();
			}
		}

		protected ModuleStateCore ModuleStateCore => _moduleState;

		public virtual DatasetFieldsCollection Fields { get; set; } = new();

		public virtual List<DatasetRowModelCore> Rows { get; set; } = new();

		public string GetElementNamePrefix() => "Dataset";
	}
}
