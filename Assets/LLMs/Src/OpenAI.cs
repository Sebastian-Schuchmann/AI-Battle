using LLMs.Src;
using UnityEngine;

[CreateAssetMenu(menuName = "LLMs/OpenAI")]
public class OpenAI : LanguageModel
{
    public ApiKey apiKey;
    
    public override string Prompt(string message)
    {
        throw new System.NotImplementedException();
    }
}