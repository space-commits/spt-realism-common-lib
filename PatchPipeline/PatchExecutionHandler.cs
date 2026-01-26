using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public delegate EPatchResult PatchExecutionHandler<TContext>(TContext ctx);

    public sealed class ExecutionHandle<TContext>
    {
        internal PatchExecutionHandler<TContext> Handler;
    }
}
