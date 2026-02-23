using System.Linq;
using System.Xml.Linq;
using DMB.Core.Elements;

namespace DMB.Core.Dmf
{
    public class DmfServiceCore
    {
        public void Save(ModuleStateCore state, string filePath)
        {
            var rootGrid = state.GetMainGrid();
            if (rootGrid == null) return;

            var module = new XElement("Module",
                new XAttribute("version", DmfConstants.CurrentVersion));

            module.Add(SaveGrid(rootGrid));

            var doc = new XDocument(module);
            doc.Save(filePath);
        }

        protected virtual GridModelCore InitiateGridModel(ModuleStateCore state)
        {
            var grid = new GridModelCore(state);
            return grid;
        }

        protected virtual RowModelCore InitiateRowModel(ModuleStateCore state)
        {
            var row = new RowModelCore(state);
            return row;
        }

        public GridModelCore? Load(ModuleStateCore state, string filePath)
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

        private GridModelCore LoadGrid(ModuleStateCore state, XElement node, CellModelCore? parentCell)
        {
            var grid = this.InitiateGridModel(state);
            grid.ParentCell = parentCell;

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

        private RowModelCore LoadRow(ModuleStateCore state, XElement node, GridModelCore parentGrid)
        {
            var row = this.InitiateRowModel(state);
            row.ParentGrid = parentGrid;

            state.Register(row);
            DmfReflect.ReadAttributes(node, row);

            foreach (var cellNode in node.Elements("Cell"))
            {
                var cell = LoadCell(state, cellNode, row);
                row.Cells.Add(cell);
            }

            return row;
        }

        protected virtual CellModelCore InitiateCellModel(ModuleStateCore state, RowModelCore parentRow)
        {
            var cell = new CellModelCore(state, parentRow);
            return cell;
        }

        private CellModelCore LoadCell(ModuleStateCore state, XElement node, RowModelCore parentRow)
        {
            var cell = this.InitiateCellModel(state, parentRow);
            cell.Row = parentRow;

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

        protected virtual ButtonModelCore InitiateButtonModel(ModuleStateCore state)
        {
            var button = new ButtonModelCore(state);
            return button;
        }

        protected virtual TextBlockModelCore InitiateTextBlockModel(ModuleStateCore state)
        {
            var tb = new TextBlockModelCore(state);
            return tb;
        }

        protected virtual TextInputModelCore InitiateTextInputModel(ModuleStateCore state)
        {
            var ti = new TextInputModelCore(state);
            return ti;
        }

        protected virtual SwitchModelCore InitiateSwitchModel(ModuleStateCore state)
        {
            var sw = new SwitchModelCore(state);
            return sw;
        }

        protected virtual CheckBoxModelCore InitiateCheckBoxModel(ModuleStateCore state)
        {
            var cb = new CheckBoxModelCore(state);
            return cb;
        }

        protected virtual DatePickerModelCore InitiateDatePickerModel(ModuleStateCore state)
        {
            var dp = new DatePickerModelCore(state);
            return dp;
        }

        protected virtual TimePickerModelCore InitiateTimePickerModel(ModuleStateCore state)
        {
            var tp = new TimePickerModelCore(state);
            return tp;
        }

        protected virtual ImageModelCore InitiateImageModel(ModuleStateCore state)
        {
            var img = new ImageModelCore(state);
            return img;
        }

        private ElementModel? LoadElement(ModuleStateCore state, XElement node, CellModelCore parentCell)
        {
            ElementModel el;

            switch (node.Name.LocalName)
            {
                case "Grid":
                    return LoadGrid(state, node, parentCell);

                case "Button":
                    el = this.InitiateButtonModel(state);
                    break;

                case "TextBlock":
                    el = this.InitiateTextBlockModel(state);
                    break;

                case "TextInput":
                    el = this.InitiateTextInputModel(state);
                    break;

                case "Switch":
                    el = this.InitiateSwitchModel(state);
                    break;

                case "CheckBox":
                    el = this.InitiateCheckBoxModel(state);
                    break;

                case "DatePicker":
                    el = this.InitiateDatePickerModel(state);
                    break;

                case "TimePicker":
                    el = this.InitiateTimePickerModel(state);
                    break;

                case "Image":
                    el = this.InitiateImageModel(state);
                    break;

                default:
                    return null;
            }

            el.ParentCell = parentCell;

            state.Register(el);
            DmfReflect.ReadAttributes(node, el);

            return el;
        }

        private XElement SaveGrid(GridModelCore grid)
        {
            var node = NewNode(grid);
            DmfReflect.WriteAttributes(node, grid);

            foreach (var row in grid.Rows)
                node.Add(SaveRow(row));

            return node;
        }

        private XElement SaveRow(RowModelCore row)
        {
            var node = NewNode(row);
            DmfReflect.WriteAttributes(node, row);

            foreach (var cell in row.Cells)
                node.Add(SaveCell(cell));

            return node;
        }

        private XElement SaveCell(CellModelCore cell)
        {
            var node = NewNode(cell);
            DmfReflect.WriteAttributes(node, cell);

            if (cell.Element != null)
                node.Add(SaveElement(cell.Element));

            return node;
        }

        private XElement SaveElement(ElementModel el)
        {
            if (el is GridModelCore g)
                return SaveGrid(g);

            var node = NewNode(el);
            DmfReflect.WriteAttributes(node, el);
            return node;
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
