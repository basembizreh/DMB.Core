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

			// Globals
			_interpreter.SetVariable("Globals", state.Globals);

			// Vars: by name -> value
			var vars = state.AllItems
				.OfType<VariableModelCore>()
				.ToDictionary(v => v.Name, v => v.Value);

			_interpreter.SetVariable("Vars", vars);

			// Inputs: by element Id -> value (only IValueElement)
			var inputs = state.AllItems
				.OfType<IValueElement>()
				.Cast<IModuleItem>()
				.ToDictionary(i => i.Id, i => ((IValueElement)i).Value);

			_interpreter.SetVariable("Inputs", inputs);
		}

		/// <summary>
		/// Evaluate expression without any extra context.
		/// </summary>
		public object? Evaluate(string? expression)
			=> Evaluate(expression, contextVars: null);

		/// <summary>
		/// Evaluate expression with extra context variables (e.g. Row, Item, Index).
		/// </summary>
		public object? Evaluate(string? expression, IDictionary<string, object?>? contextVars)
		{
			if (string.IsNullOrWhiteSpace(expression))
				return null;

			var exp = NormalizeExpression(expression);

			lock (_sync)
			{
				// Inject context variables (if any)
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
					// Clean up injected variables to avoid leaking state to the next evaluation.
					if (contextVars is not null)
					{
						foreach (var kv in contextVars)
							_interpreter.UnsetVariable(kv.Key);
					}
				}
			}
		}

		/// <summary>
		/// Convenience method for dataset calculated fields:
		/// exposes Row variable as the row.Values dictionary.
		/// </summary>
		public object? EvaluateForRow(string? expression, IDictionary<string, object?> rowValues)
		{
			// NOTE: keep the variable name exactly as your ExpressionEditor writes it (Row or row).
			var ctx = new Dictionary<string, object?>
			{
				["Row"] = rowValues
			};

			return Evaluate(expression, ctx);
		}

		private static string NormalizeExpression(string expression)
		{
			var s = expression.Trim();

			// Allow Excel-style expressions that start with '='
			if (s.Length > 0 && s[0] == '=')
				s = s.Substring(1).TrimStart();

			return s;
		}
	}
}