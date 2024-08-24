using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LLMTester : MonoBehaviour
{
    public LanguageModel languageModel;
    public string prompt;
    
    [ContextMenu("Generate")]
    public void Generate()
    {
        languageModel.Generate(prompt, Debug.Log);
    }
}
