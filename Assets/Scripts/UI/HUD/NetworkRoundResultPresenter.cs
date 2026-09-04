using CY.Core;
using CY.Network;
using UnityEngine;

namespace CY.UI
{
    public class NetworkRoundResultPresenter : MonoBehaviour
    {
        [SerializeField] private NetworkRoundStatus networkStatus;
        [SerializeField] private ResultUI resultUI;

        private RoundPhase lastPhase = RoundPhase.Waiting;

        private void Update()
        {
            if (Session.IsHost) return;

            RoundPhase phase = networkStatus.Phase;
            if (phase == lastPhase) return;

            lastPhase = phase;
            
            if (phase == RoundPhase.Success) resultUI.OnRoundSuccess();
            else if (phase == RoundPhase.Failed) resultUI.OnRoundFail();
        }
    }
}