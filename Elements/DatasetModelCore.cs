using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Elements
{
	public class DatasetModelCore<T> : IModuleItem
		where T : DatasetFieldModelCore
    {
		private readonly ModuleStateCore _moduleState;
		private string _id = "";
		private DatasetFieldsCollection<T>? _fields;

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

		public string DatasetName { get => this.Id; }

        protected ModuleStateCore ModuleStateCore => _moduleState;

		public virtual DatasetFieldsCollection<T> Fields 
		{
			get
			{
				if (this._fields is null)
				{
					this._fields = new DatasetFieldsCollection<T>(this.DatasetName);
                }
				return this._fields;
			}
        }

		public virtual List<DatasetRowModelCore> Rows { get; set; } = new();

		public string GetElementNamePrefix() => "Dataset";
	}
}
