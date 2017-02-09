using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Discord;
using Discord.Commands;
using MimmiBot.Properties;
using System.Collections.Generic;

namespace MimmiBot
{
    internal class Core
    {
        List<string> Admins = new List<string>();
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
            
            string currentuser = e.User.Name;
            Random rand = new Random();
            int messagetosend;
            messagetosend = rand.Next(0, 3);
            var isUserAdmin = false;
            var str = e.Message.RawText.Split(Convert.ToChar(" "));
            foreach (var role in e.User.Roles)
            {
                if (role.ToString() == "Ember Keeper")
                {
                    isUserAdmin = true;
                }
            }

            bool Contains = false;
            Contains = Admins.Any(s => e.User.Name.Contains(s));
            if (Contains)
            {
                isUserAdmin = true;
            }
            int newPoints;
            if ((e.Message.RawText.ToLower().StartsWith(".ae") && str.Length == 3 && isUserAdmin) || (e.Message.RawText.ToLower().StartsWith(".ae") && str.Length == 3 && Admins.Contains(str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@!"))))
            {
                var userId = str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@");
                var pointsToAdd = Convert.ToInt32(str[Convert.ToInt32(Command.Points)]);
                newPoints = CalcNewpoints(GetPoints(userId), Convert.ToInt32(str[Convert.ToInt32(Command.Points)]), true);
                if (pointsToAdd == 0)
                {
                    if (messagetosend == 0)
                        e.Channel.SendMessage("We thought it was implied... but it needs to be a number greater than 0 ^.^");
                    else if (messagetosend == 1)
                        e.Channel.SendMessage("Are you a hacher? Please do not try to break my code!");
                    else if (messagetosend == 2)
                        e.Channel.SendMessage("Please do not make me feel that I'm useless. :cry: ");
                }
                else
                {
                    SetPoints(userId, newPoints);
                    if (pointsToAdd == 1)
                        e.User.SendMessage(pointsToAdd + " Ember added to " + userId);
                    else
                        e.User.SendMessage(pointsToAdd + " Embers added to " + userId);
                }
            }

            else if ((e.Message.RawText.ToLower().StartsWith(".re") && str.Length == 3 && isUserAdmin) || (e.Message.RawText.ToLower().StartsWith(".re") && str.Length == 3 && Admins.Contains(str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@!"))))
            {
                Random rand_ = new Random();
                int messagetosend_;
                messagetosend_ = rand_.Next(0, 3);
                var userId = str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@");
                var pointsToAdd = Convert.ToInt32(str[Convert.ToInt32(Command.Points)]);
                newPoints = CalcNewpoints(GetPoints(userId), Convert.ToInt32(str[Convert.ToInt32(Command.Points)]), false);
                if (pointsToAdd == 0)
                {
                    if (messagetosend_ == 0)
                        e.Channel.SendMessage("We thought it was implied... but it needs to be a number greater than 0 ^.^");
                    else if (messagetosend_ == 1)
                        e.Channel.SendMessage("Are you a hacher? Please do not try to break my code!");
                    else if (messagetosend == 2)
                        e.Channel.SendMessage("Please do not make me feel that I'm useless. :cry: ");
                }
                else
                {
                    if (newPoints < 0)
                    {
                        pointsToAdd = pointsToAdd - (pointsToAdd + newPoints);
                        e.Channel.SendMessage("Insufficient funds... ");
                        if (pointsToAdd == 1)
                            e.Channel.SendMessage("You need " + pointsToAdd + " more Ember for this.");
                        else
                            e.Channel.SendMessage("You need " + pointsToAdd + " more Embers for this.");
                    }
                    else
                    {
                        SetPoints(userId, newPoints);
                        if (pointsToAdd == 1)
                            e.User.SendMessage(pointsToAdd + " Ember removed from " + userId);
                        else
                            e.User.SendMessage(pointsToAdd + " Embers removed from " + userId);
                    }
                }
            }

            else if (e.Message.RawText.ToLower().StartsWith(".ae"))
            {
                string userId;
                if ((str.Length > 1 && isUserAdmin) || (str.Length > 1 && Admins.Contains(str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@"))))
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

                if (isUserAdmin)
                    e.Channel.SendMessage(userId + " Type just \".ae\" in the chat to add embers.");
                else
                    e.Channel.SendMessage(userId + " You need to have the 'Admin' rank to use this command.");
            }

            else if (e.Message.RawText.ToLower().StartsWith(".refresh"))
            {
                string userId;
                if ((str.Length > 1 && isUserAdmin) || (str.Length > 1 && Admins.Contains(str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@!"))))
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
                if (!Admins.Contains(userId) && isUserAdmin)
                {
                    Admins.Add(userId);
                    e.User.SendMessage("You are succesfully added to my list of Admins!");
                }
                else
                {
                    if (isUserAdmin || Admins.Contains(userId))
                        e.User.SendMessage("Oh great master, you are already part of my list of Admins.");
                    else
                        e.Channel.SendMessage("Sorry, but I can't give you the 'Ember keeper' role.");
                }
            }

            else if ((e.Message.RawText.StartsWith(".remove") && isUserAdmin) || (e.Message.RawText.StartsWith(".remove") && Admins.Contains(str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@!"))))
            {
                var userId = str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@!");
                if (Admins.Contains(userId))
                {
                    Admins.Remove(userId);
                    e.User.SendMessage("Succesfully removed " + userId + " from the Admin list.");
                }
                else if (isUserAdmin || Admins.Contains(userId))
                    e.User.SendMessage("That user is not an Admin for me.");
                else
                    e.Channel.SendMessage("You need to be an Admin to use this command. Try '.refresh' if you are.");
            }

            else if (e.Message.RawText.ToLower().StartsWith(".re"))
            {
                string userId;
                if ((str.Length > 1 && isUserAdmin) || (str.Length > 1 && Admins.Contains(str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@"))))
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

                if (isUserAdmin)
                    e.Channel.SendMessage(userId + " Type just \".re\" in the chat to remove embers.");
                else
                    e.Channel.SendMessage(userId + " You need to have the 'Admin' rank to use this command.");
            }

            else if (e.Message.RawText.ToLower().StartsWith(".embers"))
            {
                string userId;
                if ((str.Length > 1 && isUserAdmin) || (str.Length > 1 && Admins.Contains(str[Convert.ToInt32(Command.Uid)].Replace("<@!", "<@"))))
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
                    await e.User.SendMessage(points + " embers added to " + e.User.NicknameMention);
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
