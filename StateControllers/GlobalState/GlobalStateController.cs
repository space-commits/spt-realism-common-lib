using EFT;
using RealismCommonLib.StateControllers.InstanceState;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RealismCommonLib.Plugin;

namespace RealismCommonLib.StateControllers.GlobalState
{
    public class GlobalStateController : MonoBehaviour
    {
        void Awake() 
        {
            AddComponents(this.gameObject);
        }

        void Desrtoy() 
        {
        }

        void AddComponents(GameObject go) 
        {
            GameStateInstance = go.gameObject.AddComponent<GameState>();
            AudioControllerInstance = go.gameObject.AddComponent<RealismAudioController>();
        }
    }
}
