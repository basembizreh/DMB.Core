using System.Linq;
using System.Xml.Linq;
using DMB.Core.Elements;

namespace DMB.Core.Dmf
{
    public static class DmfService
    {
        public static void Save(ModuleStateCore state, string filePath)
        {
            var rootGrid = state.GetMainGrid();
            if (rootGrid == null) return;

            var module = new XElement("Module",
                new XAttribute("version", DmfConstants.CurrentVersion));

            module.Add(SaveGrid(rootGrid));

            var doc = new XDocument(module);
            doc.Save(filePath);
        }

        public static GridModelCore? Load(ModuleStateCore state, string filePath)
        {
            var doc = XDocument.Load(filePath);

            var version = doc.Root?.Attribute("version")?.Value;
            if (version != DmfConstants.CurrentVersion)
            {
                throw new Exception($"Unsupported DMF version: {version}");
            }

            var gridNode = doc.Root?.Element("Grid");
            if (gridNode == null)
                return null;

            state.Clear();

            var rootGrid = LoadGrid(state, gridNode, null);
            state.SetMainGrid(rootGrid);

            state.RaiseStateChanged();
            return rootGrid;
        }

        private static XElement SaveGrid(GridModelCore grid)
        {
            var node = NewNode(grid);
            DmfReflect.WriteAttributes(node, grid);

            foreach (var row in grid.Rows)
                node.Add(SaveRow(row));

            return node;
        }

        private static XElement SaveRow(RowModelCore row)
        {
            var node = NewNode(row);
            DmfReflect.WriteAttributes(node, row);

            foreach (var cell in row.Cells)
                node.Add(SaveCell(cell));

            return node;
        }

        private static XElement SaveCell(CellModelCore cell)
        {
            var node = NewNode(cell);
            DmfReflect.WriteAttributes(node, cell);

            if (cell.Element != null)
                node.Add(SaveElement(cell.Element));

            return node;
        }

        private static XElement SaveElement(ElementModel el)
        {
            if (el is GridModelCore g)
                return SaveGrid(g);

            var node = NewNode(el);
            DmfReflect.WriteAttributes(node, el);
            return node;
        }

        private static GridModelCore LoadGrid(ModuleStateCore state, XElement node, CellModelCore? parentCell)
        {
            var grid = new GridModelCore(state)
            {
                ParentCell = parentCell
            };

            state.Register(grid);
            DmfReflect.ReadAttributes(node, grid);

            foreach (var rowNode in node.Elements("Row"))
            {
                var row = LoadRow(state, rowNode, grid);
                grid.Rows.Add(row);
            }

            if (parentCell != null)
                parentCell.Element = grid;

            return grid;
        }

        private static RowModelCore LoadRow(ModuleStateCore state, XElement node, GridModelCore parentGrid)
        {
            var row = new RowModelCore(state)
            {
                ParentGrid = parentGrid
            };

            state.Register(row);
            DmfReflect.ReadAttributes(node, row);

            foreach (var cellNode in node.Elements("Cell"))
            {
                var cell = LoadCell(state, cellNode, row);
                row.Cells.Add(cell);
            }

            return row;
        }

        private static CellModelCore LoadCell(ModuleStateCore state, XElement node, RowModelCore parentRow)
        {
            var cell = new CellModelCore(state, parentRow)
            {
                Row = parentRow
            };

            state.Register(cell);
            DmfReflect.ReadAttributes(node, cell);

            var child = node.Elements().FirstOrDefault();
            if (child != null)
            {
                var el = LoadElement(state, child, cell);
                cell.Element = el;
                if (el != null) el.ParentCell = cell;
            }

            return cell;
        }

        private static ElementModel? LoadElement(ModuleStateCore state, XElement node, CellModelCore parentCell)
        {
            ElementModel el;

            switch (node.Name.LocalName)
            {
                case "Grid":
                    return LoadGrid(state, node, parentCell);

                case "Button":
                    el = new ButtonModelCore(state);
                    break;

                case "TextBlock":
                    el = new TextBlockModelCore(state);
                    break;

                default:
                    return null;
            }

            el.ParentCell = parentCell;

            state.Register(el);
            DmfReflect.ReadAttributes(node, el);

            return el;
        }

        private static XElement NewNode(object obj)
        {
            var typeName = obj.GetType().Name;
            if (typeName.EndsWith("Model"))
                typeName = typeName.Substring(0, typeName.Length - 5);

            return new XElement(typeName);
        }
    }
}
