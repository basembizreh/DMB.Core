using System;
using System.Collections.Generic;

namespace DMB.Core.Elements
{
	public class DatasetRowModelCore
	{
		public Dictionary<string, object?> Values { get; set; }
			= new(StringComparer.OrdinalIgnoreCase);

		public virtual DatasetRowModelCore Clone()
			=> new DatasetRowModelCore
			{
				Values = new Dictionary<string, object?>(this.Values, StringComparer.OrdinalIgnoreCase)
			};

		/// <summary>
		/// Compare values as strings (safe for null and non-string values).
		/// Useful for static/manual datasets and UI equality checks.
		/// </summary>
		public virtual bool ContentEquals(DatasetRowModelCore other)
		{
			if (other is null)
				return false;

			if (Values.Count != other.Values.Count)
				return false;

			foreach (var kv in Values)
			{
				if (!other.Values.TryGetValue(kv.Key, out var otherVal))
					return false;

				var a = Convert.ToString(kv.Value) ?? string.Empty;
				var b = Convert.ToString(otherVal) ?? string.Empty;

				if (!string.Equals(a, b, StringComparison.Ordinal))
					return false;
			}

			return true;
		}

		// -----------------------
		// Helper methods
		// -----------------------

		public bool TryGetValue(string fieldName, out object? value)
		{
			value = null;

			if (string.IsNullOrWhiteSpace(fieldName))
				return false;

			return Values.TryGetValue(fieldName, out value);
		}

		public string GetString(string fieldName, string defaultValue = "")
		{
			if (!TryGetValue(fieldName, out var v) || v is null)
				return defaultValue;

			return Convert.ToString(v) ?? defaultValue;
		}

		public T? Get<T>(string fieldName, T? defaultValue = default)
		{
			if (!TryGetValue(fieldName, out var v) || v is null)
				return defaultValue;

			// If already correct type
			if (v is T typed)
				return typed;

			try
			{
				// Handle enums
				var targetType = typeof(T);
				if (targetType.IsEnum)
				{
					if (v is string s)
						return (T)Enum.Parse(targetType, s, ignoreCase: true);

					return (T)Enum.ToObject(targetType, v);
				}

				// Convert primitives (int, decimal, bool, DateTime, ...)
				return (T)Convert.ChangeType(v, targetType);
			}
			catch
			{
				return defaultValue;
			}
		}

		public void Set(string fieldName, object? value)
		{
			if (string.IsNullOrWhiteSpace(fieldName))
				return;

			Values[fieldName] = value;
		}
	}
}