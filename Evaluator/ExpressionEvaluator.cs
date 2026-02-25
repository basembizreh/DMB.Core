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

        public ExpressionEvaluator(ModuleStateCore state)
        {
            this._interpreter = new Interpreter();

            this._interpreter.SetVariable("Globals", state.Globals);

            var vars = state.AllItems.OfType<VariableModelCore>().Cast<VariableModelCore>()
                .ToDictionary(p => p.Name, c => c.Value);

            if (vars != null)
            {
                this._interpreter.SetVariable("Vars", vars);
            }

            var inputs = state.AllItems.OfType<IValueElement>()
                .Cast<IModuleItem>()
                .ToDictionary(i => i.Id, i => ((IValueElement)i).Value);

            if (inputs != null)
            {
                this._interpreter.SetVariable("Inputs", inputs);
            }

            this._interpreter.SetVariable("Globals", state.Globals);
        }

        public object? Evaluate(string expression) => this._interpreter.Eval(expression);
    }
}
