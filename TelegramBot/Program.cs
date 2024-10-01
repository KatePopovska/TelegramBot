using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;

    static async Task Main()
    {
        ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        IConfiguration configuration = configurationBuilder.AddUserSecrets<Program>().Build();
        string token = configuration.GetSection("bot")["bot_token"];
        _botClient = new TelegramBotClient(token);
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message, 
            },

            ThrowPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();

        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"{me.FirstName} started!");

        await Task.Delay(-1);
    }


    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:

                    var message = update.Message;
                    var user = message.From;

                    Console.WriteLine($"{user.FirstName} ({user.Id}) send message: {message.Text}");

                    var chat = message.Chat;
                    switch (message.Type)
                    {
                        case MessageType.Text:

                            if (message.Text == "/start")
                            {
                                var inlineKeyboard = new InlineKeyboardMarkup(
                                   new List<InlineKeyboardButton[]>()
                                   {
                                       new InlineKeyboardButton[]
                                       {
                                           InlineKeyboardButton.WithCallbackData("Портфолио", "button1"),
                                           InlineKeyboardButton.WithCallbackData("Прайс", "button2")
                                       }
                                   });

                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Select",
                                    replyMarkup: inlineKeyboard);

                                return;
                            }
                            return;

                        default:
                            {
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Используй только текст!");
                                return;
                            }
                    }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
