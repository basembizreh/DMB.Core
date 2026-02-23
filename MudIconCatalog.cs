using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core
{
	public class MudIconCatalog
	{
		private static IReadOnlyList<MudIconItem>? _cache;

		public static IReadOnlyList<MudIconItem> GetAll()
		{
			if (_cache != null) return _cache;

			var iconsType = Type.GetType("MudBlazor.Icons, MudBlazor");
			if (iconsType == null)
				return _cache = Array.Empty<MudIconItem>();

			var items = new List<MudIconItem>();

			// material groups most common
			AddGroup(items, iconsType, "Material", "Filled");
			AddGroup(items, iconsType, "Material", "Outlined");
			AddGroup(items, iconsType, "Material", "Rounded");
			AddGroup(items, iconsType, "Material", "Sharp");
			AddGroup(items, iconsType, "Material", "TwoTone");

			//_cache = items
			//	.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
			//	.ToList();

			_cache = items
				.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
				.Select(g => g.First())
				.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
				.ToList();

			return _cache;
		}

		private static void AddGroup(List<MudIconItem> items, Type iconsType, string family, string variant)
		{
			var familyType = iconsType.GetNestedType(family, BindingFlags.Public);
			var variantType = familyType?.GetNestedType(variant, BindingFlags.Public);
			if (variantType == null) return;

			// constants usually are public const string OR public static string property
			foreach (var f in variantType.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (f.FieldType != typeof(string)) continue;
				var value = f.GetValue(null) as string;
				if (string.IsNullOrWhiteSpace(value)) continue;

				items.Add(new MudIconItem(
					Family: family,
					Variant: variant,
					Name: f.Name,
					Key: $"{family}.{variant}.{f.Name}",
					Value: value
				));
			}

			foreach (var p in variantType.GetProperties(BindingFlags.Public | BindingFlags.Static))
			{
				if (p.PropertyType != typeof(string)) continue;
				var value = p.GetValue(null) as string;
				if (string.IsNullOrWhiteSpace(value)) continue;

				items.Add(new MudIconItem(
					Family: family,
					Variant: variant,
					Name: p.Name,
					Key: $"{family}.{variant}.{p.Name}",
					Value: value
				));
			}
		}
	}

	public sealed record MudIconItem(
		string Family,
		string Variant,
		string Name,
		string Key,
		string Value);
}
