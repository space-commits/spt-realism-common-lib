using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public sealed class PatchVetoPipeline<TContext>
    {
        private readonly List<Func<TContext, bool>> _subscribers = new();

        /// <summary>
        /// Return true if you want to prevent the patch from running.
        /// </summary>
        public IDisposable Register(Func<TContext, bool> subscriber)
        {
            _subscribers.Add(subscriber);
            return new Subscription(() => _subscribers.Remove(subscriber));
        }

        public bool ShouldVeto(TContext ctx)
        {
            bool veto = false;

            //allow all subs to run logic
            foreach (var sub in _subscribers)
            {
                if (sub(ctx))
                    veto = true; 
            }

            return veto;
        }
    }
}
