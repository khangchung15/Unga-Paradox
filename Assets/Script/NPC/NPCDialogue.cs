using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Sprite speakerPortrait;
        [TextArea(3, 10)]
        public string text;
        public AudioClip voiceSound;
        public float voicePitch = 1f;
        public float typingSpeed = 0.05f;
    }

    public DialogueLine[] dialogueLines;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1.5f;
    
    // Fallback values if not specified in individual lines
    public AudioClip defaultVoiceSound;
    public float defaultVoicePitch = 1f;
    public float defaultTypingSpeed = 0.05f;
}