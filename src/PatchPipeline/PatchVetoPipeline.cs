using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public sealed class PatchVetoPipeline<TContext>
    {
        private readonly List<Func<TContext, bool>> _subscribers = new();

        /// <summary>
        /// Return true to stop the patch running.
        /// </summary>
        public IDisposable Register(Func<TContext, bool> subscriber)
        {
            _subscribers.Add(subscriber);
            return new Subscription(() => _subscribers.Remove(subscriber));
        }

        public bool ShouldVeto(TContext ctx)
        {
            bool veto = false;

            //no early out, every subscriber still gets to run
            foreach (var sub in _subscribers)
            {
                if (sub(ctx))
                    veto = true; 
            }

            return veto;
        }
    }
}
