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

    var responce = await chatClient.GetResponseAsync(chatHistory);
    
    Console.WriteLine("\nAi responce:");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(responce.Text + "\n");
    Console.WriteLine();
}





