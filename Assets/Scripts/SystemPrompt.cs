using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LLMs.Src
{
    [CreateAssetMenu(fileName = "Prompt", menuName = "LLMs/SystemPrompt", order = 0)]
    public class SystemPrompt : ScriptableObject
    {
        public RegexOptions RegexOptions;
        [TextArea(20, 40)]
        public string prompt;
        
        public List<Message> messages;
    }

    [System.Serializable]
    public class RegexOptions
    {
        public enum RegexType
        {
            FirstDigit,
            FirstDigitAfterChoice
        }
        
        public RegexType regexType;
        
        public int ParseMove(string move)
        {
            switch (regexType)
            {
                case RegexType.FirstDigit:
                    return ParseMoveFirstDigit(move);
                case RegexType.FirstDigitAfterChoice:
                    return ParseMoveFirstDigitAfterChoice(move);
                default:
                    return -1;
            }
        }
        
        private int ParseMoveFirstDigit(string response)
        {
            Match match = Regex.Match(response, @"\b[0-8]\b");

            if (match.Success)
            {
                return int.Parse(match.Value);
            }

            return -1; // No valid move found
        }
        
        private int ParseMoveFirstDigitAfterChoice(string response)
        {
            // Case-insensitive RegexOptions to match "CHOICE:" followed by optional spaces and a digit 1-9
            Match match = Regex.Match(response, @"CHOICE:\s*([0-8])", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success && match.Groups.Count > 1)
            {
                return int.Parse(match.Groups[1].Value);
            }

            return -1; // No valid move found
        }
    }
}