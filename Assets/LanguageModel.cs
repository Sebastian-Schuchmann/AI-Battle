using UnityEngine;

public abstract class LanguageModel : ScriptableObject
{
    public abstract string Prompt(string message);
}