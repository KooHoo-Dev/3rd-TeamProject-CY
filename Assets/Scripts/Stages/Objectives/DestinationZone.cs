using CY.Core;
using CY.Vehicle;
using UnityEngine;

namespace CY.Stages
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DestinationZone : MonoBehaviour
    {
        [SerializeField] private RoundManager roundManager;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null) return;
            if (body.GetComponent<VehicleController>() == null) return;

            roundManager.OnDestinationReached();
        }
    }
}