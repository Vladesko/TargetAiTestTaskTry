using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = Host.CreateApplicationBuilder();

builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434"), "llama3"));

var app = builder.Build();

var chatClient = app.Services.GetRequiredService<IChatClient>();

var chatHistory = new List<ChatMessage>();

while (true)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Your prompt: ");
    var userPromt = Console.ReadLine();

    chatHistory.Add(new ChatMessage(ChatRole.User, userPromt));

    Console.WriteLine("Waiting for answer...");

    var responce = await chatClient.GetResponseAsync(chatHistory);
    
    Console.WriteLine("\nAi responce:");
    Console.ForegroundColor = ConsoleColor.Green;
    PrintTextWithCode(responce.Text);
    Console.WriteLine();
}

void PrintTextWithCode(string text)
{
    int count = 0;

    for (int i = 0; i < text.Length; i++)
    {
        if (text[i] == '`')
            count++;
        
        else
            count = 0;


        if (count == 3 && Console.ForegroundColor != ConsoleColor.DarkBlue)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            count = 0;
        }

        else if (count == 3 && Console.ForegroundColor == ConsoleColor.DarkBlue)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            count = 0;
        }

        Console.Write(text[i]);
    }
}






