using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DMB.Core
{
	internal static class IdValidator
	{
		private static readonly Regex _rx =
		new(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

		public static bool IsValid(string? id)
			=> !string.IsNullOrWhiteSpace(id) && _rx.IsMatch(id);
	}
}
