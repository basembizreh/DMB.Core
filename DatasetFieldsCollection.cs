using DMB.Core.Elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core
{
	public sealed class DatasetFieldsCollection<T> : Collection<T>
        where T : DatasetFieldModelCore
    {
		private readonly string _datasetId;

        public DatasetFieldsCollection(string datasetId)
        {
            this._datasetId = datasetId;
        }

        public event Action? Changed;

		public bool UniqueIgnoreCase { get; set; } = true;

		public Func<DatasetFieldModelCore, (bool ok, string error)>? Validator { get; set; }

		protected override void InsertItem(int index, T item)
		{
			item.Name = Normalize(item.Name);
			item.Dataset = this._datasetId;	
            this.EnsureUniqueOrThrow(item, indexToIgnore: null);
			base.InsertItem(index, item);
			this.Changed?.Invoke();
		}

		protected override void SetItem(int index, T item)
		{
			item.Name = Normalize(item.Name);

			this.ValidateOrThrow(item, indexToIgnore: index);
			this.EnsureUniqueOrThrow(item, indexToIgnore: index);

			base.SetItem(index, item);
			this.Changed?.Invoke();
		}

		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
			this.Changed?.Invoke();
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			this.Changed?.Invoke();
		}

		private static string Normalize(string? s) => (s ?? "").Trim();

		private void ValidateOrThrow(DatasetFieldModelCore item, int? indexToIgnore)
		{
			if (string.IsNullOrWhiteSpace(item.Name))
				throw new InvalidOperationException("Field name cannot be empty.");

			if (Validator is null) 
				return;

			var (ok, error) = Validator(item);
			if (!ok)
				throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "Invalid value." : error);
		}

		private void EnsureUniqueOrThrow(DatasetFieldModelCore item, int? indexToIgnore)
		{
			if (!UniqueIgnoreCase) return;

			bool exists = this
				.Select((v, i) => new { v, i })
				.Any(x =>
					(!indexToIgnore.HasValue || x.i != indexToIgnore.Value) &&
					string.Equals(x.v.Name, item.Name, StringComparison.OrdinalIgnoreCase));

			if (exists)
				throw new InvalidOperationException($"Duplicate value '{item}' is not allowed.");
		}
	}
}
