using System;
using Game.Scripts.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Game.Scripts
{
    public class UIController : MonoBehaviour
    {
        [Required] public RectTransform ClockHand;
        [Required] public TextMeshProUGUI AgeCounter;
        [Required, Range(0, 2)] public float DegreesPerDay = 1;
        
        private void Update()
        {
            var currentRotation = ClockHand.eulerAngles;
            currentRotation.z = -1 * (AgeManager.Instance.CurrentDay * DegreesPerDay % 360);
            ClockHand.eulerAngles = currentRotation;
        }
        
        private void OnEnable()
        {
            AgeManager.Instance.OnAgeChanged += OnYearChanged;
        }
        
        private void OnDisable()
        {
            AgeManager.Instance.OnAgeChanged -= OnYearChanged;
        }
        
        private void OnYearChanged(int year)
        {
            AgeCounter.text = year.ToString();
        }
    }
}