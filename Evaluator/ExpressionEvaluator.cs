using System;
using System.Collections.Generic;
using System.Linq;
using DynamicExpresso;
using DMB.Core.Elements;

namespace DMB.Core.Evaluator
{
	public class ExpressionEvaluator
	{
		private readonly Interpreter _interpreter;
		private readonly object _sync = new();

		public ExpressionEvaluator(ModuleStateCore state)
		{
			_interpreter = new Interpreter();

			// Allow LINQ extension methods like FirstOrDefault()
			_interpreter.Reference(typeof(Enumerable));

			// Globals
			_interpreter.SetVariable("Globals", state.Globals);

			// Vars: by name -> value
			var vars = state.AllItems
				.OfType<VariableModelCore>()
				.ToDictionary(v => v.Name, v => v.Value, StringComparer.OrdinalIgnoreCase);

			_interpreter.SetVariable("Vars", vars);

			// Inputs: by element Id -> value (only IValueElement)
			var inputs = state.AllItems
				.OfType<IValueElement>()
				.Cast<IModuleItem>()
				.ToDictionary(i => i.Id, i => (IValueElement)i);
                //.ToDictionary(i => i.Id, i => ((IValueElement)i).Value, StringComparer.OrdinalIgnoreCase);

            _interpreter.SetVariable("Inputs", inputs);

			// Datasets: by dataset Id -> dataset object
			// NOTE: adjust the generic type if your DatasetModelCore type differs.
			//var datasets = state.AllItems
			//	.OfType<DatasetModelCore<DatasetFieldModelCore>>()
			//	.ToDictionary(d => d.Id, d => (object)d, StringComparer.OrdinalIgnoreCase);
			var datasets = state.AllItems
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
				// Keep the name exactly as your editor generates: Row
				["Row"] = rowValues
			};

			return Evaluate(expression, ctx);
		}

		private static string NormalizeExpression(string expression)
		{
			var s = expression.Trim();
			if (s.Length > 0 && s[0] == '=')
				s = s.Substring(1).TrimStart();
			return s;
		}
	}
}