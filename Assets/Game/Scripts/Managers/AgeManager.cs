using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Helpers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Managers
{
    [Serializable]
    public struct PhaseConfiguration
    {
        public string Name;

        [Range(0, 120)] public int Age;

        [Range(0, 1)] public float Speed;

        [Range(0, 1)] public float JumpHeight;
    }

    public class AgeManager : Singleton<AgeManager>, IDestroyable
    {
        protected AgeManager()
        {
        }

        public bool ShouldDestroyOnLoad() => true;

        public void OnSceneChanged(Scene previousScene, Scene nextScene)
        {
        }

        [Required] [TitleGroup("General")] public int CurrentDay = 0;

        [TitleGroup("General"), Range(1, 365)] public int DaysPerSecond = 1;

        [BoxGroup("Phases")] public List<PhaseConfiguration> Phases = new List<PhaseConfiguration>();

        [TitleGroup("General"), ShowInInspector]
        public PhaseConfiguration CurrentPhase => Phases
            .OrderBy(phase => phase.Age)
            .LastOrDefault(phase => phase.Age * 365 <= CurrentDay);

        private void Start()
        {
            InvokeRepeating(nameof(AddDays), 1f, 1f);
        }

        private void AddDays() => CurrentDay += DaysPerSecond;
    }
}