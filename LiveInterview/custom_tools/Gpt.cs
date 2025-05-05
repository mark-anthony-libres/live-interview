using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class OpenAIClient
{
    private static readonly string ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    private HttpClient HttpClient = new HttpClient();
    private List<string> chatHistory = new List<string>();
    private string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIClient() {

        if(ApiKey == null)
        {
            throw new NotImplementedException("OPENAI_API_KEY is not set");
        }
        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        // OR if you're on .NET Core 3.1+ or .NET 5+
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }


    // Method to read file content
    static string ExtractTextFromFile(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            return string.Empty;
        }
    }

    // Method to get response from GPT and keep track of chat history
    public async Task<string> GetGPTResponseWithHistory(string question)
    {


        string fileContent = ExtractTextFromFile("./base.txt"); ;  // Replace with your actual file path

        string systemPrompt = $"Based on this information: {fileContent}\n" +
            "You are Mark Anthony Libres dont forget that. Answer confidently like you're speaking in an interview. " +
            "Broaden your explanation and don't add extra information, as if you're speaking. " +
            "use simple filipino words and easy to speak and make it casual and a bit professional" +
            "and please make your answer TAGLISH but not deep filipino/tagalog";

        Trace.WriteLine(systemPrompt);

        // Include previous chat history in the conversation
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        // Add all past chat history (messages) to the new request
        //foreach (var chatMessage in chatHistory)
        //{
        //    messages.Add(new { role = "user", content = chatMessage });
        //}

        // Add current question to the messages
        messages.Add(new { role = "user", content = $"please answer taglist(tagalog and english) but not so deep, here is the interview question: {question}" });

        var requestBody = new
        {
            model = "gpt-3.5-turbo",  // You can change to "gpt-4" if you have access
            messages = messages,
            temperature = 0.5
        };

        var jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await this.HttpClient.PostAsync(this.ApiUrl, content);
       
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
        string answer = jsonResponse.choices[0].message.content;

        // Add the question and answer to the chat history
        this.chatHistory.Add(question);
        this.chatHistory.Add(answer);

        Trace.WriteLine($"GPT Response: {answer}");

        return answer;

    }
}
