using System;
using System.Collections.Generic;

namespace DMB.Core.Elements
{
    public class DatasetRowModelCore
    {
        public Dictionary<string, string> Values { get; set; }
            = new(StringComparer.OrdinalIgnoreCase);

        public virtual DatasetRowModelCore Clone()
            => new DatasetRowModelCore
			{
                Values = new Dictionary<string, string>(this.Values, StringComparer.OrdinalIgnoreCase)
            };

        public virtual bool ContentEquals(DatasetRowModelCore other)
        {
            if (other == null) return false;
            if (Values.Count != other.Values.Count) return false;

            foreach (var kv in Values)
            {
                if (!other.Values.TryGetValue(kv.Key, out var v)) return false;
                if (!string.Equals(kv.Value ?? "", v ?? "", StringComparison.Ordinal)) return false;
            }

            return true;
        }
    }
}