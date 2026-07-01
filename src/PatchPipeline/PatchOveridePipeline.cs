using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public sealed class PatchOveridePipeline<TContext>
    {
        private readonly List<Func<TContext, bool>> _subscribers = new();

        public IDisposable Register(Func<TContext, bool> subscriber)
        {
            _subscribers.Add(subscriber);
            return new Subscription(() => _subscribers.Remove(subscriber));
        }

        public bool ShouldOverride(TContext ctx)
        {
            bool seen = false;

            foreach (var sub in _subscribers)
            {
                if (sub(ctx))
                {
                    if (seen)
                        Plugin.ModLogger.LogError($"{Plugin.MOD_NAME}: Multiple overrides for same patch");
                    seen = true;
                }
            }

            return seen;
        }
    }
}
