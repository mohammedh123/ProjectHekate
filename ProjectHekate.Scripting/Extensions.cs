using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ProjectHekate.Scripting
{
    public static class Extensions
    {
        public static IEnumerable<TNodeType> GetDescendantsOfType<TNodeType>(this ParserRuleContext context) where TNodeType : class, IParseTree 
        {
            if (context == null) {
                yield break;
            }
            if (context is TNodeType) {
                yield return context as TNodeType;
                yield break;
            }

            var contextQueue = new Queue<IParseTree>();
            contextQueue.Enqueue(context);

            while (contextQueue.Any()) {
                var ctx = contextQueue.Dequeue();

                for(var i = 0; i < ctx.ChildCount; i++) {
                    var child = ctx.GetChild(i);

                    if (child is TNodeType) yield return child as TNodeType;
                    contextQueue.Enqueue(child);
                }
            }
        }
    }
}
