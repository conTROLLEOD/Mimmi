using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Discord;
using Discord.Commands;
using MimmiBot.Properties;

namespace MimmiBot
{
    internal class Core
    {
        private readonly DiscordClient _bot;
        private readonly CommandService _commands;

        private enum Command
        {
            /*
                        MainCommand = 0,
            */
            Uid = 1,
            Points = 2
        }

        public Core()
        {
            _bot = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            _bot.UsingCommands(x =>
            {
                x.PrefixChar = '.';
                x.AllowMentionPrefix = true;
            });

            _commands = _bot.GetService<CommandService>();

            _bot.MessageReceived += Bot_MessageReceived;
            _bot.UserJoined += Bot_UserJoined;
            _bot.UserLeft += Bot_UserLeft;
            var serverList = _bot.Servers.ToList();

            foreach (var server in serverList)
            {

            }

            _bot.ExecuteAndWait(async () =>
            {
                await _bot.Connect(Settings.Default.token, TokenType.Bot);
                _bot.SetGame("with Fire!");
            });

        }

        public void Bot_UserJoined(object sender, UserEventArgs e)
        {

        }

        public void Bot_UserLeft(object sender, UserEventArgs e)
        {

        }

        public void Bot_MessageReceived(object sender, MessageEventArgs e)
        {
            var isUserAdmin = false;
            foreach (var role in e.User.Roles)
            {
                if (role.ToString() == "Ember Keeper") isUserAdmin = true;
            }

            var str = e.Message.RawText.Split(Convert.ToChar(" "));
            int newPoints;
            if (e.Message.RawText.ToLower().StartsWith(".ae") && str.Length == 3 && isUserAdmin)
            {
                var userId = str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@");
                var pointsToAdd = Convert.ToInt32(str[Convert.ToInt32(Command.Points)]);
                newPoints = CalcNewpoints(GetPoints(userId), Convert.ToInt32(str[Convert.ToInt32(Command.Points)]), true);
                SetPoints(userId, newPoints);
                e.Channel.SendMessage(pointsToAdd + " Embers added to " + userId);
            }
            else if (e.Message.RawText.ToLower().StartsWith(".re") && isUserAdmin)
            {
                var userId = str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@");
                var pointsToAdd = Convert.ToInt32(str[Convert.ToInt32(Command.Points)]);
                newPoints = CalcNewpoints(GetPoints(userId), Convert.ToInt32(str[Convert.ToInt32(Command.Points)]), false);
                SetPoints(userId, newPoints);
                e.Channel.SendMessage(pointsToAdd + " Embers removed from " + userId);
            }
            else if (e.Message.RawText.ToLower().StartsWith(".embers"))
            {
                string userId;
                if (str.Length > 1 && isUserAdmin)
                {
                    userId = str[1];
                }
                else if (str.Length == 1)
                {
                    userId = e.User.NicknameMention;
                }
                else
                {
                    return;
                }

                userId = userId.Replace("<@!", "<@");
                e.Channel.SendMessage(userId + " Your current points balance is " + GetPoints(userId));
            }
        }

        private void DeleteLastMessage()
        {

        }

        private void AddPoints()
        {
            _commands.CreateCommand("addEmbers")
                .Do(async e =>
                {
                    var message = e.Message.RawText.Split(Convert.ToChar(";"));

                    var points = GetPoints(e.User.Id.ToString());
                    //test
                    var getResult = from role in e.User.Roles
                                    where role.Name == "testRole"
                                    select role;

                    foreach (var role in e.User.Roles)
                    {


                    }
                    SetPoints(e.User.Id.ToString(), CalcNewpoints(points, Convert.ToInt32(e.Message.RawText.Split(Convert.ToChar(";"))[2]), true));
                    await e.Channel.SendMessage(points + " embers added to " + e.User.NicknameMention);
                });
        }

        private int GetPoints(string uid)
        {
            try
            {
                if (!Directory.Exists("Data/")) Directory.CreateDirectory("Data/");
                if (!File.Exists("Data/Points.txt"))
                {
                    using (var sw = new StreamWriter("Data/Points.txt"))
                    {
                    }
                }
                var lineArr = File.ReadAllLines("Data/Points.txt");

                foreach (var line in lineArr)
                {
                    var userAttributes = line.Split(Convert.ToChar(";"));
                    if (userAttributes[0] != uid) continue;
                    return Convert.ToInt32(userAttributes[1]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return 0;
        }

        private void SetPoints(string uid, int points)
        {
            if (!Directory.Exists("Data/")) Directory.CreateDirectory("Data/");
            if (!File.Exists("Data/Points.txt"))
            {
                using (var sw = new StreamWriter("Data/Points.txt"))
                {
                }
            }
            var lines = File.ReadAllLines("Data/Points.txt").ToList();

            var lineFound = false;

            foreach (var line in lines)
            {
                if (!line.Contains(uid)) continue;
                var lineArr = lines.ToArray();
                var lineNumber = Array.FindIndex(lineArr, row => row.Contains(uid));
                if (line.Contains(uid))
                {
                    lineArr[lineNumber] = uid + ";" + points;
                }
                lines = lineArr.ToList();
                lineFound = true;
            }

            if (lineFound)
            {
                File.WriteAllLines("data/points.txt", lines);
            }
            else
            {
                lines.Add(uid + ";" + points);
                File.WriteAllLines("data/points.txt", lines);
            }
        }

        private int CalcNewpoints(int curPoints, int pointsToAddSub, bool add)
        {
            if (add)
            {
                curPoints += pointsToAddSub;
            }
            else
            {
                curPoints -= pointsToAddSub;
            }
            return curPoints;
        }

        private static void Log(object sender, LogMessageEventArgs e)
        {
            if (!Directory.Exists("logs/")) Directory.CreateDirectory("logs/");

            using (var sw = new StreamWriter("logs/log.txt"))
            {
                sw.WriteLine(e.Message);
            }
        }

    }
}
