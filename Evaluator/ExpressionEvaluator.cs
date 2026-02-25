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

        public ExpressionEvaluator(ModuleStateCore state, Dictionary<string, object?>? vars, List<IModuleItem>? moduleItems)
        {
            this._interpreter = new Interpreter();

            this._interpreter.SetVariable("Globals", state.Globals);

            if (vars != null)
                this._interpreter.SetVariable("Vars", vars);

            if (moduleItems != null)
            {
                var inputs = moduleItems
                    .OfType<IValueElement>()
                    .Cast<IModuleItem>()
                    .ToDictionary(i => i.Id, i => ((IValueElement)i).Value);

                this._interpreter.SetVariable("Inputs", inputs);
            }
        }

        public object? Evaluate(string expression) => this._interpreter.Eval(expression);
    }
}
