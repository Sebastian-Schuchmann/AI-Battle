using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

namespace LLMs.Src
{
    [CreateAssetMenu(fileName = "ReplayLLM", menuName = "LLMs/ReplayLLM", order = 0)]
    public class ReplayLLM : LanguageModel
    {
        public TextAsset replayFile;
        public float timeBetweenSteps = 1f;
        
        public Replay replay;
        private int currentIndex = 0;
        
        public void Parse()
        {
            replay = JsonConvert.DeserializeObject<Replay>(replayFile.text);
            currentIndex = 0;
        }
        

        public override void Generate(string prompt, Action<string> callback)
        {
            if (replay == null)
            {
                Parse();
            }
            Debug.Log(currentIndex);
            CoroutineRunner.Instance.StartCoroutine(DoStep(callback));
        }

        IEnumerator DoStep(Action<string> callback)
        {
            yield return new WaitForSeconds(timeBetweenSteps);
            
            if (currentIndex >= replay.steps.Count)
            {
                callback?.Invoke(null);
                yield break;
            }
            
            var step = replay.steps[currentIndex];
            callback?.Invoke(step.response);
            currentIndex++;
        }
    }
}