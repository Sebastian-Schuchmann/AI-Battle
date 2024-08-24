using System;
using UnityEngine;

namespace LLMs.Src
{
    [CreateAssetMenu(fileName = "Random", menuName = "LLMs/Random", order = 0)]
    public class Random : LanguageModel
    {
        private Action<string> currentCallback;
        
        public override void Generate(string prompt, Action<string> callback)
        {
            var TicTacToe = FindObjectOfType<TicTacToe>();
            var availableMoves = TicTacToe.GetAvailableMovesAsArray();
            
            //select random index
            var randomIndex = UnityEngine.Random.Range(0, availableMoves.Length);
            var randomMove = availableMoves[randomIndex];

            callback?.Invoke($"CHOICE:{randomMove}");
        }
        
    }
}