using CY.Network;
using UnityEngine;

namespace CY.Core
{
    public class RoundTimer : MonoBehaviour
    {
        public float ElapsedTime => elapsedTime;
        public bool IsRunning => isRunning;

        private float elapsedTime;
        private bool isRunning;

        public void StartRound()
        {
            elapsedTime = 0f;
            isRunning = true;
        }

        public void StopRound()
        {
            isRunning = false;
        }

        private void Update()
        {
            if (!Session.IsHost) return;
            if (!isRunning) return;

            elapsedTime += Time.deltaTime;
        }
    }
}