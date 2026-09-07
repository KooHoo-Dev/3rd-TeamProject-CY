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
        [SerializeField] private IntroScreenShakeController screenShake;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text dialogueText;

        [Header("TypeWriter")]
        [SerializeField] private float secondsPerCharacter = 0.035f;
        [SerializeField] private float delaySeconds = 1f;
        
        private const string DelayTag = "<delay>";

        private bool advanceRequested;
        private bool isTyping;
        private bool completeTypingRequested;
        
        private void Awake()
        {
            dialoguePanel.SetActive(false);
        }

        private void Update()
        {
            bool advanceInputPressed =
                (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
            
            if (!advanceInputPressed) return;

            if (isTyping)
            {
                completeTypingRequested = true;
                return;
            }

            advanceRequested = true;
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

                portraits.SetSpeaker(line.positionId);
                
                if (line.shake) screenShake.Shake();
                
                advanceRequested = false;
                yield return TypeLine(line.text);

                yield return new WaitUntil(() => advanceRequested);

                if (line.hideAfter)
                {
                    yield return portraits.Hide(line.positionId);
                }
            }

            dialoguePanel.SetActive(false);
        }
        
        private IEnumerator TypeLine(string text)
        {
            isTyping = true;
            completeTypingRequested = false;
            dialogueText.text = string.Empty;

            string visibleText = text.Replace(DelayTag, string.Empty);

            for (int index = 0; index < text.Length;)
            {
                if (completeTypingRequested)
                {
                    dialogueText.text = visibleText;
                    isTyping = false;
                    yield break;
                }

                if (HasDelayTagAt(text, index))
                {
                    index += DelayTag.Length;

                    yield return WaitForDuration(delaySeconds);

                    if (completeTypingRequested)
                    {
                        dialogueText.text = visibleText;
                        isTyping = false;
                        yield break;
                    }

                    continue;
                }

                if (text[index] == '<')
                {
                    int tagEndIndex = text.IndexOf('>', index + 1);

                    if (tagEndIndex >= 0)
                    {
                        int tagLength = tagEndIndex - index + 1;

                        dialogueText.text += text.Substring(index, tagLength);
                        index += tagLength;

                        continue;
                    }
                }
                
                dialogueText.text += text[index];
                index++;

                if (index >= text.Length) break;

                yield return WaitForDuration(secondsPerCharacter);
            }

            isTyping = false;
        }

        private IEnumerator WaitForDuration(float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (completeTypingRequested) yield break;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private static bool HasDelayTagAt(string text, int index)
        {
            return index + DelayTag.Length <= text.Length
                   && text.IndexOf(DelayTag, index) == index;
        }
    }
}