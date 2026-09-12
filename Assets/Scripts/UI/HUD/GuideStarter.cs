using UnityEngine;

namespace CY.UI
{
    [RequireComponent(typeof(TutorialGuideUI))]
    public class PlayTestGuideStarter : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string guideMessage = "";

        private void Start()
        {
            GetComponent<TutorialGuideUI>().Show(guideMessage);
        }
    }
}