using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace DMB.Core.Dmf
{
    internal static class DmfReflect
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropsCache = new();

        public static void WriteAll(XElement node, object obj)
        {
            WriteAttributes(node, obj);
            WriteExpandable(node, obj);
            WriteChildrenCollections(node, obj);
        }

        public static void ReadAll(XElement node, object obj)
        {
            ReadAttributes(node, obj);
            ReadExpandable(node, obj);
            ReadChildrenCollections(node, obj);
        }

        // ===== Existing (attributes) =====

        public static void WriteAttributes(XElement node, object obj)
        {
            var t = obj.GetType();
            var props = PropsCache.GetOrAdd(t, tt =>
                tt.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<DmfAttribute>() != null)
                  .ToArray());

            foreach (var p in props)
            {
                if (p.GetCustomAttribute<ExpandablePropertyAttribute>() != null)
                    continue;
                var attrName = p.GetCustomAttribute<DmfNameAttribute>()?.Name ?? p.Name;
                var val = p.GetValue(obj);
                if (val == null) continue;

                var text = ToStringValue(p.PropertyType, val);
                node.SetAttributeValue(ToAttrName(attrName), text);
            }
        }

        public static void ReadAttributes(XElement node, object obj)
        {
            var t = obj.GetType();
            var props = PropsCache.GetOrAdd(t, tt =>
                tt.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<DmfAttribute>() != null)
                  .ToArray());

            foreach (var p in props)
            {
                if (p.GetCustomAttribute<ExpandablePropertyAttribute>() != null)
                    continue;

                var attrName = p.GetCustomAttribute<DmfNameAttribute>()?.Name ?? p.Name;
                var xAttr = node.Attribute(ToAttrName(attrName));
                if (xAttr == null) continue;

                if (TryParseValue(p.PropertyType, xAttr.Value, out var parsed))
                    p.SetValue(obj, parsed);
            }
        }

        // ===== NEW: Expandable handling =====

        private static void WriteExpandable(XElement parent, object obj)
        {
            var t = obj.GetType();
            var props = PropsCache.GetOrAdd(t, tt =>
                tt.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<DmfAttribute>() != null)
                  .ToArray());

            foreach (var p in props)
            {
                if (p.GetCustomAttribute<ExpandablePropertyAttribute>() == null)
                    continue;

                var childName = p.GetCustomAttribute<DmfNameAttribute>()?.Name ?? p.Name;
                var val = p.GetValue(obj);
                if (val == null) continue;

                var childNode = new XElement(childName);

                WriteAll(childNode, val);

                parent.Add(childNode);
            }
        }

        private static void ReadExpandable(XElement parent, object obj)
        {
            var t = obj.GetType();
            var props = PropsCache.GetOrAdd(t, tt =>
                tt.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<DmfAttribute>() != null)
                  .ToArray());

            foreach (var p in props)
            {
                if (p.GetCustomAttribute<ExpandablePropertyAttribute>() == null)
                    continue;

                var childName = p.GetCustomAttribute<DmfNameAttribute>()?.Name ?? p.Name;
                var childNode = parent.Element(childName);
                if (childNode == null) continue;

                var current = p.GetValue(obj);

                if (current == null)
                {
                    throw new InvalidOperationException($"Expandable property '{t.Name}.{p.Name}' is null during Load. " +
                        $"All properties marked with [ExpandableProperty] must be initialized in the constructor.");
                }

                ReadAll(childNode, current);
            }
        }

        private static void ReadChildrenCollections(XElement node, object model)
        {
            var t = model.GetType();
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var p in props)
            {
                var chAttr = p.GetCustomAttributes(typeof(DmfChildrenAttribute), true)
                              .OfType<DmfChildrenAttribute>()
                              .FirstOrDefault();
                if (chAttr is null) continue;

                var container = node.Element(chAttr.ContainerName);
                if (container == null) continue;

                var collectionObj = p.GetValue(model);
                if (collectionObj is null)
                {
                    throw new InvalidOperationException($"DmfChildren collection '{t.Name}.{p.Name}' is null during Load. " +
                        $"It must be initialized in the constructor or property getter.");
                }

                // Must support Clear() to avoid duplicates
                collectionObj.GetType().GetMethod("Clear")?.Invoke(collectionObj, null);

                // Must support Add(T)
                var addMethod = collectionObj.GetType().GetMethod("Add");
                if (addMethod == null)
                    throw new InvalidOperationException(
                        $"DmfChildren collection '{t.Name}.{p.Name}' does not expose a public Add(T) method.");

                var itemType = addMethod.GetParameters().First().ParameterType;

                foreach (var itemNode in container.Elements(chAttr.ItemName))
                {
                    var item = Activator.CreateInstance(itemType);
                    if (item == null) continue;

                    ReadAll(itemNode, item);
                    addMethod.Invoke(collectionObj, new[] { item });
                }
            }
        }

        private static void WriteChildrenCollections(XElement node, object model)
        {
            var props = model.GetType().GetProperties();

            foreach (var p in props)
            {
                var chAttr = p.GetCustomAttributes(typeof(DmfChildrenAttribute), true)
                              .OfType<DmfChildrenAttribute>()
                              .FirstOrDefault();
                if (chAttr is null) continue;

                var value = p.GetValue(model);
                if (value is null) continue;
                if (value is not System.Collections.IEnumerable items) continue;

                var container = new XElement(chAttr.ContainerName);

                foreach (var item in items)
                {
                    if (item is null) continue;

                    var itemNode = new XElement(chAttr.ItemName);

                    // Write item attributes & expandable props
                    WriteAll(itemNode, item); // recursion
                    container.Add(itemNode);
                }

                node.Add(container);
            }
        }

        // ===== Helpers =====

        private static string ToAttrName(string propName)
        {
            if (string.IsNullOrEmpty(propName)) return propName;
            return char.ToLowerInvariant(propName[0]) + propName.Substring(1);
        }

        private static string ToStringValue(Type type, object value)
        {
            var nt = Nullable.GetUnderlyingType(type) ?? type;

            if (nt.IsEnum) return value.ToString() ?? "";
            if (nt == typeof(bool)) return (bool)value ? "true" : "false";

            if (nt == typeof(DateTime))
                return ((DateTime)value).ToString("o", CultureInfo.InvariantCulture);

            if (value is IFormattable f)
                return f.ToString(null, CultureInfo.InvariantCulture) ?? "";

            return value.ToString() ?? "";
        }

        private static bool TryParseValue(Type type, string text, out object? value)
        {
            value = null;
            var nt = Nullable.GetUnderlyingType(type) ?? type;

            if (nt == typeof(string)) { value = text; return true; }

            if (nt.IsEnum)
            {
                if (Enum.TryParse(nt, text, true, out var e)) { value = e; return true; }
                return false;
            }

            if (nt == typeof(int) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            { value = i; return true; }

            if (nt == typeof(decimal) && decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            { value = d; return true; }

            if (nt == typeof(double) && double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var db))
            { value = db; return true; }

            if (nt == typeof(bool))
            {
                if (bool.TryParse(text, out var b)) { value = b; return true; }
                if (text == "1") { value = true; return true; }
                if (text == "0") { value = false; return true; }
                return false;
            }

            if (nt == typeof(DateTime) &&
                DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            { value = dt; return true; }

            return false;
        }
    }
}