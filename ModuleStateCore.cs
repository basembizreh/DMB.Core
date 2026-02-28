using DMB.Core.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        internal void EnsureUniqueIdsForSubtree(IModuleItem root)
        {
            foreach (var item in EnumerateSubtreeItems(root))
            {
                var id = (item.Id ?? "").Trim();
                var (ok, _) = this.CanSetItemId(item, id);
                if (ok && !string.IsNullOrWhiteSpace(id))
                    continue;

                item.Id = this.GenerateNextElementId(item);
            }
        }

        private static IEnumerable<IModuleItem> EnumerateSubtreeItems(IModuleItem root)
        {
            // Grid => Rows => Cells => Elements (recursively)
            if (root is GridModelCore g)
            {
                yield return g;

                foreach (var row in g.Rows)
                {
                    yield return row;

                    foreach (var cell in row.Cells)
                    {
                        yield return cell;

                        if (cell.Element != null)
                        {
                            foreach (var x in EnumerateSubtreeItems(cell.Element))
                                yield return x;
                        }
                    }
                }

                yield break;
            }

            // Row alone
            if (root is RowModelCore r)
            {
                yield return r;

                foreach (var cell in r.Cells)
                {
                    yield return cell;

                    if (cell.Element != null)
                        foreach (var x in EnumerateSubtreeItems(cell.Element))
                            yield return x;
                }

                yield break;
            }

            // Cell alone
            if (root is CellModelCore c)
            {
                yield return c;

                if (c.Element != null)
                    foreach (var x in EnumerateSubtreeItems(c.Element))
                        yield return x;

                yield break;
            }

            // Plain element / variable / dataset ... etc
            yield return root;
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

		public virtual void Register(IModuleItem item)
		{
			if (!this.AllItems.Contains(item))
				this.AllItems.Add(item);
		}

		public virtual void Unregister(IModuleItem item) => this.AllItems.Remove(item);

		public virtual void Clear()
		{
			this.AllItems.Clear();
			this.Globals.Clear();
			this.MainGrid = null;
		}
    }
}
