using EFT;
using EFT.InputSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealismCommonLib.PatchPipeline
{
    public class InputContext
    {
        public ECommand Command;

        public InputContext(ECommand command)
        {
            Command = command;
        }   
    }

    public static class Pipelines
    {
        // 1) Cancel pipelines
        public static readonly PatchVetoPipeline<InputContext> InputVetoPipeline = new();

        // 3) Override pipelines
        public static readonly PatchOveridePipeline<InputContext> InputOverridePipeline = new();

        // 4) Override behavior handlers
        public static readonly PatchOverrideHandler<InputContext> InputOverrideHandler = new();
    }
}
