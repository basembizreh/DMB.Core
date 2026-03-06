using DMB.Core.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core
{
	public class ModuleStateCore
	{
		private List<IModuleItem> items = new();
		public event Action<IModuleItem, string, string>? ItemIdChanged;
		public event Action? StateChanged;

        public IDictionary<string, string?> Globals { get; }
			= new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        public ModuleStateCore()
        {
            this.Globals["Language"] = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        }

        private int _itemsVersion = 0;
        private int _cacheVersion = -1;
        private Dictionary<string, IModuleItem>? _itemsByIdCache;

        internal protected void MarkItemsChanged()
        {
            this._itemsVersion++;
        }

        public bool TryGetItemById(string id, out IModuleItem item)
        {
            EnsureCache();
            return _itemsByIdCache!.TryGetValue(id, out item!);
        }

        private void EnsureCache()
        {
            if (_itemsByIdCache is not null && _cacheVersion == _itemsVersion)
                return;

            _itemsByIdCache = new Dictionary<string, IModuleItem>(StringComparer.Ordinal);
            foreach (var it in this.items)
                if (!string.IsNullOrWhiteSpace(it.Id))
                    _itemsByIdCache[it.Id] = it;

            _cacheVersion = _itemsVersion;
        }

        public List<IModuleItem> AllItems
		{
			get { return this.items; }
		}

        public GridModelCore? MainGrid { get; set; }

		public void RaiseItemIdChanged(IModuleItem item, string oldId, string newId) =>
			this.ItemIdChanged?.Invoke(item, oldId, newId);

		internal string GenerateNextElementId(IModuleItem item)
		{
			string prefix = item.GetElementNamePrefix();
			int max = 0;

			foreach (var e in this.items)
			{
				if (string.IsNullOrWhiteSpace(e.Id)) continue;

				// Pattern: Prefix_Number
				if (!e.Id.StartsWith(prefix + "_", StringComparison.OrdinalIgnoreCase))
					continue;

				var tail = e.Id.Substring(prefix.Length + 1); // after "Prefix_"
				if (int.TryParse(tail, out int n) && n > max)
					max = n;
			}

			return $"{prefix}_{max + 1}";
		}

        internal (bool ok, string error) CanSetItemId(IModuleItem? item, string newId)
        {
            newId = (newId ?? "").Trim();

            if (!IdValidator.IsValid(newId))
                return (false, "Invalid C# identifier");

            // Unique across ALL items (recommended)
            bool exists = this.items.Any(x =>
                !ReferenceEquals(x, item) &&
                string.Equals(x.Id, newId, StringComparison.OrdinalIgnoreCase));

            if (exists)
                return (false, $"Id '{newId}' already exists.");

            return (true, "");
        }

        public void SetMainGrid(GridModelCore grid) => this.MainGrid = grid;

		public GridModelCore? GetMainGrid() => this.MainGrid;

		public void RaiseStateChanged() => this.StateChanged?.Invoke();

		public virtual void Register(IModuleItem item, bool autoAssignId = true)
		{
            if (!this.AllItems.Contains(item))
            {
                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    if (autoAssignId)
                    {
                        item.Id = this.GenerateNextElementId(item);
                    }
                }

                var props = item.GetType().GetProperties().Where(p => p.GetCustomAttributes<ChildElementsAttribute>().Any());
                if (props.Any())
                {
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(item, null);
                        if (value is System.Collections.IEnumerable enumerable)
                        {
                            foreach (var obj in enumerable)
                            {
                                if (obj is IModuleItem child)
                                    this.Register(child);
                            }
                        }
                    }
                }
                this.AllItems.Add(item);
            }
		}

        public virtual void Unregister(IModuleItem item)
        {
            var props = item.GetType().GetProperties().Where(p => p.GetCustomAttributes<ChildElementsAttribute>().Any());
            if (props.Any())
            {
                foreach (var prop in props)
                {
                    var value = prop.GetValue(item, null);
                    if (value is System.Collections.IEnumerable enumerable)
                    {
                        foreach (var obj in enumerable)
                        {
                            if (obj is IModuleItem child)
                                this.Unregister(child);
                        }
                    }
                }
            }
            this.AllItems.Remove(item);
        }

		public virtual void Clear()
		{
            foreach (var it in this.items)
            {
                if (it is IDisposable d) d.Dispose();
            }

            this.AllItems.Clear();
			this.Globals.Clear();
			this.MainGrid = null;
            this.MarkItemsChanged();
        }
    }
}
