using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("8215824761:AAHRsjNwjLrs8oakTeeEyOMT9u_9kuVLh10", cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
/*bot.OnUpdate += OnUpdate;*/

Console.WriteLine($"привет @{me.Username} это мой мой бот, это сообщение выводится в консоль");
Console.ReadLine();
cts.Cancel(); // stop the bot

// method to handle errors in polling or in your OnMessage/OnUpdate code
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception); // just dump the exception to the console
}

// method that handle messages received by the bot:
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        await bot.SendMessage(msg.Chat, "Привееет, я создал этого бота, вот его кнопки",
            replyMarkup: new InlineKeyboardButton[] { "Команды", "Ученики", "Дежурные" });
    }
}

async Task OnMessage1(Message msg, UpdateType type)
{
    if (msg.Text == "/aaa")
    {
        await bot.SendMessage(msg.Chat, "Список всех команд: -> ...",
            replyMarkup: new InlineKeyboardButton[] { "Выход в меню" });
    }
}

// method that handle other types of updates received by the bot:
/*async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
    {
        await bot.AnswerCallbackQuery(query.Id, $"говно {query.Data}");
        await bot.SendMessage(query.Message!.Chat, $"пользователь  {query.From} кликнул на {query.Data}");
    }
}*/