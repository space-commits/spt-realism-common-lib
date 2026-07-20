using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public class PatchOverrideHandler<TContext>
    {
        private readonly List<Action<TContext>> _subscribers = new();

        public IDisposable Register(Action<TContext> subscriber)
        {
            _subscribers.Add(subscriber);
            return new Subscription(() => _subscribers.Remove(subscriber));
        }

        public void RunOverrides(TContext ctx)
        {
            foreach (var h in _subscribers)
                h(ctx);
        }
    }
}
