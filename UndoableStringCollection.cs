using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DMB.Core
{
    public sealed class UndoableStringCollection : Collection<string>
    {
        public event Action? Changed;

        /// <summary>
        /// If true, values must be unique ignoring case (EmployeeId == employeeid)
        /// </summary>
        public bool UniqueIgnoreCase { get; set; } = true;

        /// <summary>
        /// Optional: validate each value (e.g., C# identifier rules).
        /// Return (true,"") if ok, otherwise (false,"error message")
        /// </summary>
        public Func<string, (bool ok, string error)>? Validator { get; set; }

        public UndoableStringCollection() 
        {
        }

        protected override void InsertItem(int index, string item)
        {
            item = Normalize(item);

            EnsureUniqueOrThrow(item, indexToIgnore: null);

            base.InsertItem(index, item);
            Changed?.Invoke();
        }

        protected override void SetItem(int index, string item)
        {
            item = Normalize(item);

            ValidateOrThrow(item, indexToIgnore: index);
            EnsureUniqueOrThrow(item, indexToIgnore: index);

            base.SetItem(index, item);
            Changed?.Invoke();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            Changed?.Invoke();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            Changed?.Invoke();
        }

        private static string Normalize(string? s) => (s ?? "").Trim();

        private void ValidateOrThrow(string item, int? indexToIgnore)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new InvalidOperationException("Value cannot be empty.");

            if (Validator is null) return;

            var (ok, error) = Validator(item);
            if (!ok)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "Invalid value." : error);
        }

        private void EnsureUniqueOrThrow(string item, int? indexToIgnore)
        {
            if (!UniqueIgnoreCase) return;

            bool exists = this
                .Select((v, i) => new { v, i })
                .Any(x =>
                    (!indexToIgnore.HasValue || x.i != indexToIgnore.Value) &&
                    string.Equals(x.v, item, StringComparison.OrdinalIgnoreCase));

            if (exists)
                throw new InvalidOperationException($"Duplicate value '{item}' is not allowed.");
        }
    }
}