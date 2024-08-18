using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

        public Action<int> OnAgeChanged;

        private float daysPerYear = 365;
        
        private Tweener m_CurrentAgingTweener;

        private void Update()
        {
            var oldAge = Mathf.FloorToInt(CurrentDay / daysPerYear);
            CurrentDay += DaysPerSecond * Time.deltaTime;
            var newAge = Mathf.FloorToInt(CurrentDay / daysPerYear);

            if (newAge > oldAge)
            {
                OnAgeChanged.Invoke(newAge);
            }
        }
            
        public void SpeedUp(float daysPerSecond, float animationDurationInSeconds)
        {
            if (m_CurrentAgingTweener == null || m_CurrentAgingTweener.IsPlaying()) return;
            m_CurrentAgingTweener = DOTween
                .To(() => DaysPerSecond, x => DaysPerSecond = x, daysPerSecond, animationDurationInSeconds)
                .From(true);
            SoundManager.Instance.Play("Ticking");
        }
    }
}