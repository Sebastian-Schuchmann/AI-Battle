using UnityEngine;

namespace LLMs.Src
{
    [CreateAssetMenu(fileName = "API-Key", menuName = "LLMs/API-Key", order = 0)]
    public class ApiKey : ScriptableObject
    {
        public string key;
    }
}