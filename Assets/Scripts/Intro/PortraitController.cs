using System;
using System.Collections;
using UnityEngine;

namespace Intro
{
    [Serializable]
    public class CharacterPortrait
    {
        public string characterId;
        public Sprite portrait;
    }

    public class PortraitController : MonoBehaviour
    {
        [SerializeField] private CharacterPortrait[] characters;
        [SerializeField] private PortraitSlot[] slots;

        public IEnumerator Show(string characterId, string positionId)
        {
            CharacterPortrait character = Array.Find(characters, 
                item => item.characterId == characterId);

            PortraitSlot slot = Array.Find(slots,
                item => item.PositionId == positionId);

            yield return slot.Show(characterId, character.portrait);
        }

        public IEnumerator Hide(string positionId)
        {
            PortraitSlot slot = Array.Find(slots,
                item => item.PositionId == positionId);

            yield return slot.Hide();
        }
    }
}