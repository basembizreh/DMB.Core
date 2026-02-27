using DMB.Core.Elements;
using DMB.Core.Evaluator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core
{
	public static class ExpressionablePropertyResolver<T>
		where T : struct, Enum
	{
		public static T Resolve(IExpressionablePropertyCore<T> property, ExpressionEvaluator evaluator)
		{
			if (property.ValueMode == ValueMode.Literal)
			{
				return property.Value;
			}
			else
			{
				if (property.Expression != null)
				{
					var val = evaluator.Evaluate(property.Expression)?.ToString();
					var t = typeof(T);
					if (t.IsEnum)
					{
						if (Enum.TryParse(val, true, out T propertyValue))
						{
							return propertyValue;
						}
						else
						{
							throw new Exception($"Failed to parse '{val}' as enum of type {t.Name}.");
						}
					}
					else
					{
						try
						{
							return (T)Convert.ChangeType(val, t)!;
						}
						catch (Exception ex)
						{
							throw new Exception($"Failed to convert '{val}' to type {t.Name}.", ex);
						}
					}
				}
				else
				{
					throw new InvalidOperationException("Expression is null.");
				}
			}
		}
	}
}
