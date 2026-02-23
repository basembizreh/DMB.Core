using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core
{
	public static class MudIconResolver
	{
		private static readonly Dictionary<string, string> _map =
			MudIconCatalog.GetAll().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

		public static string? Resolve(string? key)
			=> (key != null && _map.TryGetValue(key, out var v)) ? v : null;
	}
}
