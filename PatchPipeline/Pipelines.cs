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
        // Cancel pipelines
        public static readonly PatchVetoPipeline<InputContext> InputVetoPipeline = new();

        // Override pipelines
        public static readonly PatchOveridePipeline<InputContext> InputOverridePipeline = new();

        // Override behavior handlers
        public static readonly PatchOverrideHandler<InputContext> InputOverrideHandler = new();
    }
}
