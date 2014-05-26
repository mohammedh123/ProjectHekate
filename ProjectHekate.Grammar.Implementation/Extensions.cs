using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ProjectHekate.Grammar.Implementation
{
    public static class Extensions
    {
        public static TNodeType GetFirstDescendantOfType<TNodeType>(this ParserRuleContext context) where TNodeType : class, IParseTree 
        {
            if (context == null) return null;
            if (context is TNodeType) return context as TNodeType;

            var contextQueue = new Queue<IParseTree>();
            contextQueue.Enqueue(context);

            while (contextQueue.Any()) {
                var ctx = contextQueue.Dequeue();

                for(var i = 0; i < ctx.ChildCount; i++) {
                    var child = ctx.GetChild(i);

                    if (child is TNodeType) return child as TNodeType;
                    contextQueue.Enqueue(child);
                }
            }

            return null;
        }
    }
}
