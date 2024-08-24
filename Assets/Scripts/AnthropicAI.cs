using System;
using System.Collections;
using System.Collections.Generic;
using LLMs.Src;
using UnityEngine;
using UnityEngine.Networking;

namespace LLMs.Src
{
    [CreateAssetMenu(fileName = "Anthropic", menuName = "LLMs/AnthropicAI", order = 0)]
    public class AnthropicAI : LanguageModel
    {
    public ApiKey apiKey;
    public string model;
    public string API_URL = "https://api.anthropic.com/v1/messages";
    public int maxTokens = 1024;


    public override void Generate(string prompt, Action<string> callback)
    {
        CoroutineRunner.Instance.StartCoroutine(GenerateCoroutine(prompt, callback));
    }

    private IEnumerator GenerateCoroutine(string prompt, Action<string> callback)
    {
        var messages = new List<Message>();
        messages.AddRange(systemPrompt.messages);
        messages.Add(new Message {role = "user", content = systemPrompt.prompt + ". Okay let's start! " + prompt});

        AntrophicRequestBody requestBody = new AntrophicRequestBody
        {
            model = model,
            messages = messages.ToArray(),
            max_tokens = 1024
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log($"Sending request: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("x-api-key",  apiKey.key);
            request.SetRequestHeader("anthropic-version", "2023-06-01");
            request.SetRequestHeader("content-type", "application/json");

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
                var responseBody = JsonUtility.FromJson<AntrophicResponseBody>(request.downloadHandler.text);
                string generatedText = responseBody.content[0].text;
                callback?.Invoke(generatedText);
            }
        }
    }
    }
    
    [Serializable]
    public class AntrophicRequestBody
    {
        public string model;
        public int max_tokens;
        public Message[] messages;
    } 
    
    [Serializable]
    public class Content
    {
        public string text;
        public string type;
    }

    [Serializable]
    public class AntrophicResponseBody
    {
        public Content[] content;
    }
    
    /*
     * curl https://api.anthropic.com/v1/messages \
            --header "x-api-key: $ANTHROPIC_API_KEY" \
            --header "anthropic-version: 2023-06-01" \
            --header "content-type: application/json" \
            --data \
       '{
           "model": "claude-3-5-sonnet-20240620",
           "max_tokens": 1024,
           "messages": [
               {"role": "user", "content": "Hello, world"}
           ]
       }'
     */
    
    /*
     * Example Response:
     *
     * {
         "content": [
           {
             "text": "Hi! My name is Claude.",
             "type": "text"
           }
         ],
         "id": "msg_013Zva2CMHLNnXjNJJKqJ2EF",
         "model": "claude-3-5-sonnet-20240620",
         "role": "assistant",
         "stop_reason": "end_turn",
         "stop_sequence": null,
         "type": "message",
         "usage": {
           "input_tokens": 2095,
           "output_tokens": 503
         }
       }
     */
}