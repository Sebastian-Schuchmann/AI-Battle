using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


[Serializable]
public class ReplayStep
{
    public string boardStateBefore;
    public string boardStateAfter;
    public string prompt;
    public string response;
    public bool isXTurn;
    public string modelname;    
}

public class Replay
{
    public string modelnameX;
    public string modelnameO;
    public List<ReplayStep> steps = new List<ReplayStep>();
}

public class TTT_ReplayManager : MonoBehaviour
{
    public TicTacToe TicTacToe;
    public bool recordOnStart;

    private Replay replay;

    private void Awake()
    {
        if (recordOnStart)
        {
            StartRecording();
        }
    }

    public void StartRecording()
    {
        replay = new Replay
        {
            modelnameX = TicTacToe.languageModelX.name,
            modelnameO = TicTacToe.languageModelO.name
        };
        
        TicTacToe.OnMove += OnMove;
    }
    
    private void OnMove(ReplayStep step)
    {
        Debug.Log($"Move from {step.modelname} Recorded");
        replay.steps.Add(step);
    }
    
    public void StopRecording()
    {
        if (replay == null || replay.steps.Count == 0)
        {
            Debug.LogWarning("No recording data to save.");
            return;
        }

        TicTacToe.OnMove -= OnMove;

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string modelXName = SanitizeFileName(replay.modelnameX);
        string modelOName = SanitizeFileName(replay.modelnameO);
        string fileName = $"{modelXName}_vs_{modelOName}_{timestamp}.json";
        string filePath = System.IO.Path.Combine(Application.dataPath, "Recordings", fileName);

        // Ensure the directory exists
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));

        // Serialize and save the data
        string jsonData = JsonConvert.SerializeObject(replay, Formatting.Indented);
        System.IO.File.WriteAllText(filePath, jsonData);

        Debug.Log($"Game recording saved to: {filePath}");

        // Reset replay
        replay = null;
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters from the file name
        return string.Join("_", fileName.Split(System.IO.Path.GetInvalidFileNameChars()));
    }

    private void OnDisable()
    {
        StopRecording();
    }
}