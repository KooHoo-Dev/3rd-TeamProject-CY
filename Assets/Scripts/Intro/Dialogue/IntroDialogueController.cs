using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Intro
{
    public class IntroDialogueController : MonoBehaviour
    {
        [SerializeField] private DialogueRepository repository;
        [SerializeField] private PortraitController portraits;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text dialogueText;

        private bool advanceRequested;

        private void Awake()
        {
            dialoguePanel.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame ||
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                advanceRequested = true;
            }
        }

        public IEnumerator PlayGroup(string groupId)
        {
            DialogueGroup group = repository.GetGroup(groupId);

            dialoguePanel.SetActive(true);

            foreach (DialogueLine line in group.lines)
            {
                yield return portraits.Show(
                    line.characterId,
                    line.positionId
                );

                advanceRequested = false;
                dialogueText.text = line.text;

                yield return new WaitUntil(() => advanceRequested);

                if (line.hideAfter)
                {
                    yield return portraits.Hide(line.positionId);
                }
            }

            dialoguePanel.SetActive(false);
        }
    }
}