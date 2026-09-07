using System;

namespace Intro
{
    [Serializable]
    public class DialogueDocument
    {
        public int schemaVersion;
        public string scenarioId;
        public DialogueGroup[] groups;
    }

    [Serializable]
    public class DialogueGroup
    {
        public string id;
        public DialogueLine[] lines;
    }

    [Serializable]
    public class DialogueLine
    {
        public string id;
        public string characterId;
        public string positionId;
        public bool hideAfter;
        public string text;
    }
}