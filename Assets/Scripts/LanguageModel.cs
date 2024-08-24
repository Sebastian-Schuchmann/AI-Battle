using System;
using LLMs.Src;
using UnityEngine;

public abstract class LanguageModel : ScriptableObject
{
    public SystemPrompt systemPrompt;
    public abstract void Generate(string prompt, Action<string> callback);
}