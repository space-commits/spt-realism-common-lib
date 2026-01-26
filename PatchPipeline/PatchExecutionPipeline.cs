using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public sealed class PatchPipeline<TContext>
    {
        private readonly List<ExecutionHandle<TContext>> _handlers = new();

        public ExecutionHandle<TContext> Register(PatchExecutionHandler<TContext> handler)
        {
            var h = new ExecutionHandle<TContext>
            {
                Handler = handler
            };

            _handlers.Add(h);
            return h;
        }

        public void Remove(ExecutionHandle<TContext> handle)
        {
            _handlers.Remove(handle);
        }

        /// <summary>
        /// Runs the pipeline.
        /// Returns true if the original method should execute.
        /// </summary>
        public bool Process(TContext ctx)
        {
            List<PatchExecutionHandler<TContext>> blockers = null;

            // Phase 1 — discover blockers
            for (int i = 0; i < _handlers.Count; i++)
            {
                var h = _handlers[i].Handler;

                if (h(ctx) == EPatchResult.Block)
                {
                    blockers ??= new List<PatchExecutionHandler<TContext>>();
                    blockers.Add(h);
                }
            }

            // If anyone blocked, only they run
            if (blockers != null)
            {
                for (int i = 0; i < blockers.Count; i++)
                    blockers[i](ctx);

                return false; // skip original
            }

            // Otherwise, everyone runs normally
            for (int i = 0; i < _handlers.Count; i++)
                _handlers[i].Handler(ctx);

            return true;
        }
    }
}
