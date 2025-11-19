using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using System.Linq;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("8215824761:AAHRsjNwjLrs8oakTeeEyOMT9u_9kuVLh10", cancellationToken: cts.Token);
var me = await bot.GetMe();

// Список всех учеников
string[] students = {
    "Адхамов Мухаммаджон",
    "Артемьев Роман",
    "Байрам Ариана",
    "Белосохов Дмитрий",
    "Борзов Ярослав",
    "Бырыкин Степан",
    "Выставкин Владислав",
    "Глухов Андрей",
    "Горбань Никита",
    "Гузев Никита",
    "Дерменжи Николай",
    "Иванова Полина",
    "Корабельников Ярослав",
    "Кудинов Иван",
    "Макурин Кирилл",
    "Медведева Мирра",
    "Роган Кирилл",
    "Самусенко Александр",
    "Серкова Александрина",
    "Спичкин Иван",
    "Хренов Вадим",
    "Челушкин Роман",
    "Череваткин Кирилл",
    "Чумаков Владислав"
};

// Словари для хранения состояний, дежурных и присутствующих
Dictionary<long, string> states = new();
Dictionary<long, List<string>> duties = new();
Dictionary<long, List<string>> presents = new();

// Функция для отправки главного меню (чтобы избежать дублирования)
async Task SendMainMenu(ChatId chatId)
{
    var keyboard = new InlineKeyboardMarkup(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("Ученики", "students") },
        new[] { InlineKeyboardButton.WithCallbackData("Дежурные", "duty") }
    });

    await bot.SendMessage(chatId, $"Хуллоу ворд,{me.LastName} выбери раздел:", replyMarkup: keyboard);
}

// Функция для отправки списка учеников с кнопкой "Выход в меню"
async Task SendStudents(ChatId chatId)
{
    var keyboard = new InlineKeyboardMarkup(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("Выход в меню", "exit") }
    });

    await bot.SendMessage(chatId, string.Join("\n", students),
        replyMarkup: keyboard);
}

// Функция для отправки списка дежурных с кнопками
async Task SendDuty(ChatId chatId)
{
    long chatIdLong = chatId.Identifier ?? throw new InvalidOperationException("Идентификатор чата не является числовым");

    if (!duties.ContainsKey(chatIdLong)) duties[chatIdLong] = new List<string>();
    var dutyList = duties[chatIdLong];
    string msg = dutyList.Any() ? "Дежурные на сегодня:\n" + string.Join("\n", dutyList) : "Список дежурных на сегодня пуст";

    var keyboard = new InlineKeyboardMarkup(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("Добавить отсутствующих", "add_absents") },
        new[] { InlineKeyboardButton.WithCallbackData("Сбросить дежурных", "reset_duties") },
        new[] { InlineKeyboardButton.WithCallbackData("Выход в меню", "exit") }
    });

    await bot.SendMessage(chatId, msg, replyMarkup: keyboard);
}

// method that handle messages received by the bot:
async Task OnMessage2(Message msg, UpdateType type)
{
    if (msg.Text == "/start")
    {
        await SendMainMenu(msg.Chat);  // Вызываем функцию для главного меню
    }
}

// Обработка сообщений для состояний
async Task OnMessage4(Message msg, UpdateType type)
{
    long chatId = msg.Chat.Id;
    if (states.ContainsKey(chatId))
    {
        if (states[chatId] == "adding_absents")
        {
            // Ожидаем ввод номеров отсутствующих
            var absents = msg.Text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim()) - 1)
                .Where(i => i >= 0 && i < students.Length)
                .Distinct()
                .ToList();
            var present = students.Where((s, i) => !absents.Contains(i)).ToList();
            presents[chatId] = present;
            states[chatId] = "selecting_duties";
            await bot.SendMessage(msg.Chat, "Присутствующие:\n" + string.Join("\n", present.Select((s, i) => $"{i + 1}. {s}")) +
                "\n\nВведите номера двух дежурных через запятую (1-" + present.Count + ")");
        }
        else if (states[chatId] == "selecting_duties")
        {
            // Ожидаем ввод номеров дежурных
            var indices = msg.Text.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim()) - 1)
                .Where(i => i >= 0 && i < presents[chatId].Count)
                .Distinct()
                .Take(2)
                .ToList();
            if (indices.Count == 2)
            {
                duties[chatId] = indices.Select(i => presents[chatId][i]).ToList();
                states.Remove(chatId);
                presents.Remove(chatId);
                await bot.SendMessage(msg.Chat, "Дежурные назначены:\n" + string.Join("\n", duties[chatId]));
                await SendMainMenu(msg.Chat);  // Возвращаемся к меню
            }
            else
            {
                await bot.SendMessage(msg.Chat, "Пожалуйста, введите ровно два номера.");
            }
        }
    }
}

// method that handle other types of updates received by the bot:
async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query })
    {
        await bot.AnswerCallbackQuery(query.Id);  // Подтверждаем нажатие

        long chatId = query.Message!.Chat.Id;
        switch (query.Data)
        {
            case "students":
                await SendStudents(query.Message!.Chat);  // Прямой переход к списку учеников
                break;
            case "duty":
                await SendDuty(query.Message!.Chat);  // Прямой переход к списку дежурных
                break;
            case "add_absents":
                states[chatId] = "adding_absents";
                await bot.SendMessage(query.Message!.Chat, "Введите номера отсутствующих учеников через запятую (1-24):\n" +
                    string.Join("\n", students.Select((s, i) => $"{i + 1}. {s}")));
                break;
            case "reset_duties":
                if (duties.ContainsKey(chatId)) duties[chatId].Clear();
                await bot.SendMessage(query.Message!.Chat, "Дежурные сброшены.");
                await SendDuty(query.Message!.Chat);  // Обновляем
                break;
            case "exit":
                // Возвращаемся к главному меню
                await SendMainMenu(query.Message!.Chat);
                break;
            default:
                await bot.SendMessage(query.Message!.Chat, "Неизвестная кнопка.");
                break;
        }
    }
}

bot.OnError += OnError;
bot.OnMessage += OnMessage1;
bot.OnMessage += OnMessage2;
bot.OnMessage += OnMessage3;
bot.OnMessage += OnMessage4;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"Кто то зашел в бота");
Console.ReadLine();
cts.Cancel();  // stop the bot

// method to handle errors in polling or in your OnMessage/OnUpdate code
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception);  // just dump the exception to the console
}

async Task OnMessage1(Message msg, UpdateType type)
{
    if (msg.Text == "/uch")
    {
        await SendStudents(msg.Chat);  // Вызываем функцию для списка учеников
    }
}

async Task OnMessage3(Message msg, UpdateType type)
{
    if (msg.Text == "/dezh")
    {
        await SendDuty(msg.Chat);  // Вызываем функцию для списка дежурных
    }
}
