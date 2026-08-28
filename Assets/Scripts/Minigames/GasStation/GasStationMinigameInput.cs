using UnityEngine;

namespace CY.Minigame
{

    [RequireComponent(typeof(GasStationMinigameController))]
    public class GasStationMinigameInput : MonoBehaviour
    {
        private GasStationMinigameController controller;
        private bool IsPressedLocally;

        public void Awake()
        {
            controller = GetComponent<GasStationMinigameController>();
        }

        public void NotifyPress()
        {
            if (IsPressedLocally) return;

            IsPressedLocally = true;
            controller.SendFuelPress();
        }

        public void NotifyRelease()
        {
            if (!IsPressedLocally) return;

            IsPressedLocally = false;
            controller.SendFuelRelease();
        }

        public void NotifyCancel()
        {
            NotifyRelease();
        }

        public void ResetInputState()
        {
            IsPressedLocally = false;
        }
    }
}
