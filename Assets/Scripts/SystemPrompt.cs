using System.Collections.Generic;
using UnityEngine;

namespace LLMs.Src
{
    [CreateAssetMenu(fileName = "Prompt", menuName = "LLMs/SystemPrompt", order = 0)]
    public class SystemPrompt : ScriptableObject
    {
        [TextArea(20, 40)]
        public string prompt;
        
        public List<OpenAI.Message> messages;
    }
}