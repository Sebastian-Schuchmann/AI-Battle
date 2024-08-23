using UnityEngine;

namespace LLMs.Src
{
    [CreateAssetMenu(fileName = "API-Key", menuName = "LLM/API-Key", order = 0)]
    public class ApiKey : ScriptableObject
    {
        public string key;
    }
}