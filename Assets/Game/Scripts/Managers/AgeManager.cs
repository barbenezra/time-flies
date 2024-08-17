using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Helpers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace Game.Scripts.Managers
{
    public class AgeManager : Singleton<AgeManager>, IDestroyable
    {
        protected AgeManager()
        {
        }

        public bool ShouldDestroyOnLoad() => true;

        public void OnSceneChanged(Scene previousScene, Scene nextScene)
        {
        }


        [Required] [TitleGroup("General")] public float CurrentDay = 0;

        [TitleGroup("General"), Range(1, 365)] public float DaysPerSecond = 100;

        public UnityEvent<int> OnAgeChanged = new UnityEvent<int>();

        private float _daysPerYear = 365;

        private void Update()
        {
            var oldAge = Mathf.FloorToInt(CurrentDay / _daysPerYear);
            CurrentDay += DaysPerSecond * Time.deltaTime;
            var newAge = Mathf.FloorToInt(CurrentDay / _daysPerYear);

            if (newAge > oldAge)
            {
                OnAgeChanged.Invoke(newAge);
            }
        }
    }
}