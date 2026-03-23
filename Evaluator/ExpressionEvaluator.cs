using System.Globalization;
using System.Linq;
using DynamicExpresso;
using DMB.Core.Elements;

namespace DMB.Core.Evaluator
{
    public class ExpressionEvaluator
    {
        private readonly Interpreter _interpreter;
        private readonly object _sync = new();

        public ExpressionEvaluator(ModuleDocumentCore moduleDocument)
        {
            _interpreter = new Interpreter();

            // Allow LINQ extension methods like FirstOrDefault()
            _interpreter.Reference(typeof(Enumerable));
            _interpreter.Reference(typeof(CultureInfo));

            // Globals
            _interpreter.SetVariable("Globals", moduleDocument.Globals);

            // Vars: by name -> value
            var vars = moduleDocument.AllItems
                .OfType<VariableModelCore>()
                .ToDictionary(v => v.Name, v => v.Value, StringComparer.OrdinalIgnoreCase);

            _interpreter.SetVariable("Variables", vars);

            // Inputs: by element Id -> element itself
            var inputs = moduleDocument.AllItems
                .OfType<IValueElement>()
                .Cast<IModuleItem>()
                .ToDictionary(i => i.Id, i => (IValueElement)i, StringComparer.OrdinalIgnoreCase);

            _interpreter.SetVariable("Inputs", inputs);

            // Datasets: by dataset Id -> dataset object
            var datasets = moduleDocument.AllItems
                .OfType<DatasetModelCore<DatasetFieldModelCore>>()
                .ToDictionary(d => d.Id, d => d, StringComparer.OrdinalIgnoreCase);

            _interpreter.SetVariable("Datasets", datasets);
        }

        public object? Evaluate(string? expression)
            => Evaluate(expression, contextVars: null);

        public object? Evaluate(string? expression, IDictionary<string, object?>? contextVars)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return null;

            var exp = NormalizeExpression(expression);

            lock (_sync)
            {
                if (contextVars is not null)
                {
                    foreach (var kv in contextVars)
                        _interpreter.SetVariable(kv.Key, kv.Value);
                }

                try
                {
                    return _interpreter.Eval(exp);
                }
                finally
                {
                    if (contextVars is not null)
                    {
                        foreach (var kv in contextVars)
                            _interpreter.UnsetVariable(kv.Key);
                    }
                }
            }
        }

        public object? EvaluateForRow(string? expression, IDictionary<string, object?> rowValues)
        {
            var ctx = new Dictionary<string, object?>
            {
                ["DataRow"] = rowValues
            };

            return Evaluate(expression, ctx);
        }

        public object? EvaluateForAction(
            string? expression,
            object? result = null,
            Exception? exception = null)
        {
            var ctx = new Dictionary<string, object?>();

            // Intentionally allow null values too
            if (result is not null || exception is null)
            {
                if (result is not null)
                    ctx["Result"] = result;
            }

            if (exception is not null)
                ctx["Exception"] = exception;

            return Evaluate(expression, ctx.Count == 0 ? null : ctx);
        }

        public static string NormalizeExpression(string expression)
        {
            var s = expression.Trim();
            if (s.Length > 0 && s[0] == '=')
                s = s.Substring(1).TrimStart();
            return s;
        }
    }
}