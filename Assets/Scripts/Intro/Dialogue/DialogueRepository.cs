using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Intro
{
    public class DialogueRepository : MonoBehaviour
    {
        [SerializeField] private string relativeFilePath = "Dialogue/intro_dialogue.json";

        private readonly Dictionary<string, DialogueGroup> groupsById = new();

        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            string path = Path.Combine(Application.streamingAssetsPath, relativeFilePath);

            DialogueDocument document =
                JsonUtility.FromJson<DialogueDocument>(File.ReadAllText(path));

            foreach (DialogueGroup group in document.groups)
            {
                groupsById.Add(group.id, group);
            }
        }

        public DialogueGroup GetGroup(string groupId)
        {
            return groupsById[groupId];
        }
    }
}