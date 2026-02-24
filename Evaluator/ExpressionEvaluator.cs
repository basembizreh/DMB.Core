using DMB.Core.Elements;
using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Evaluator
{
	public class ExpressionEvaluator
	{
		private readonly Interpreter _interpreter;
		private Dictionary<string, object?>? _variables;
		private List<IModuleItem>? items;

		public ExpressionEvaluator(Dictionary<string, object?>? variables, List<IModuleItem>? moduleItems)
		{
			this._interpreter = this.CreateInterpreter();
			this._variables = variables;
			this.items = moduleItems;

			if (this._variables != null)
			{
				foreach (var v in this._variables)
				{
					this._interpreter.SetVariable(v.Key, v.Value);
				}
			}

			if (this.items != null)
			{
				var inputs = this.items.Where(i => i is IValueElement).Cast<IValueElement>()
					.Select(i => new { Name = ((IModuleItem)i).Id, Value = i.Value })
					.ToDictionary(p => p.Name, c => c.Value);

				this._interpreter.SetVariable("Inputs", inputs);
			}
		}

		private Interpreter CreateInterpreter()
		{
			return new Interpreter();
		}

		public object Evaluate(string expression)
		{
			return this._interpreter.Eval(expression);
		}
	}
}
