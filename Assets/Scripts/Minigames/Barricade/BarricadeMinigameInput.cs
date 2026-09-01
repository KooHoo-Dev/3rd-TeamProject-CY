using UnityEngine;

namespace CY.Minigame
{
    [RequireComponent(typeof(BarricadeMinigameController))]
    public class BarricadeMinigameInput : MonoBehaviour
    {
        private BarricadeMinigameController controller;

        private void Awake()
        {
            controller = GetComponent<BarricadeMinigameController>();
        }

        public void NotifyFuseClick(int fuseIndex)
        {
            controller.SendFuseClick(fuseIndex);
        }
    }

}
