using System;
using TMPro;
using UnityEngine;

namespace Sample.Scripts.Features
{
    public class FPSTrackerView : MonoBehaviour
    {
        public TextMeshProUGUI Text;

        private void Update()
        {
            Text.text = Mathf.RoundToInt(1f / Time.deltaTime).ToString();
        }
    }
}