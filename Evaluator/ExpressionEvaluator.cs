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
        private Dictionary<string, object> _variables = new Dictionary<string, object>();

        public ExpressionEvaluator(Dictionary<string, object> variables)
        {
            this._interpreter = this.CreateInterpreter();
            this._variables = variables;

            foreach (var v in this._variables)
            {
                this._interpreter.SetVariable(v.Key, v.Value);
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
