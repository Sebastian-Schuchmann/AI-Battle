using System;
using System.Collections;
using System.Collections.Generic;
using LLMs.Src;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class Message
{
    public string role;
    [TextArea(2, 10)] public string content;
}

[Serializable]
public class ResponseBody
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Message message;
}

[Serializable]
public class RequestBody
{
    public string model = "gpt-3.5-turbo";
    public Message[] messages;
}


[CreateAssetMenu(fileName = "OpenAI", menuName = "LLMs/OpenAI", order = 0)]
public class OpenAI : LanguageModel
{
    public ApiKey apiKey;
    public string model;
    public string API_URL = "https://api.openai.com/v1/chat/completions";


    public override void Generate(string prompt, Action<string> callback)
    {
        CoroutineRunner.Instance.StartCoroutine(GenerateCoroutine(prompt, callback));
    }

    private IEnumerator GenerateCoroutine(string prompt, Action<string> callback)
    {
        var messages = new List<Message>
        {
            new() {role = "system", content = systemPrompt.prompt}
        };
        messages.AddRange(systemPrompt.messages);
        messages.Add(new Message {role = "user", content = prompt});

        RequestBody requestBody = new RequestBody
        {
            model = model,
            messages = messages.ToArray()
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log($"Sending request: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey.key);

            yield return request.SendWebRequest();


            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                //if code 429, wait for a few seconds
                if (request.responseCode == 429)
                {
                    yield return new WaitForSeconds(10);
                }
                callback?.Invoke(null);
            }
            else
            {
                ResponseBody responseBody = JsonUtility.FromJson<ResponseBody>(request.downloadHandler.text);
                string generatedText = responseBody.choices[0].message.content;
                callback?.Invoke(generatedText);
            }
        }
    }

    

}


// Utility class to run coroutines from non-MonoBehaviour classes