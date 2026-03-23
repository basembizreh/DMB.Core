using DMB.Core.Dmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMB.Core.Actions
{
    public class ActionBinding
    {
        [Dmf]
        public string? ReferenceName { get; set; }

        [Dmf]
        public string? ActionExpression { get; set; }

        [Dmf]
        [ExpandableProperty]
        public ActionBinding? OnSuccessAction { get; set; }

        [Dmf]
        [ExpandableProperty]
        public virtual ActionBinding? OnFailureAction { get; set; }

        public ActionBinding Clone()
        {
            return new ActionBinding
            {
                ReferenceName = this.ReferenceName,
                ActionExpression = this.ActionExpression,
                OnSuccessAction = this.OnSuccessAction?.Clone(),
                OnFailureAction = this.OnFailureAction?.Clone()
            };
        }

        public override string? ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.ReferenceName)
                && !string.IsNullOrWhiteSpace(this.ActionExpression))
            {
                return $"[{this.ReferenceName}].[{this.ActionExpression}]";
            }
            return base.ToString();
        }
    }
}
