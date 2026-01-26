using EFT;
using EFT.InputSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public sealed class InputContext
    {
        public ECommand Command;
    }

    public static class Pipelines
    {
        public static readonly PatchPipeline<InputContext> InputPipeline =
            new PatchPipeline<InputContext>();
    }
}
