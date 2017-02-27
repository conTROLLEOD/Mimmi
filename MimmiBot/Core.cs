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
        List<string> UsersToKick = new List<string>();
        List<string> Bannedusers = new List<string>();
        private readonly DiscordClient _bot;
        private readonly CommandService _commands;
        public ulong OutroChannelId;
        private string userId;
        private int kickrate = 10;

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
            userId = ("<@" + e.User.Id + ">").Replace("!", "");
            if (!(Getwarnings(userId, true) == 1))
                Setwarnings(userId, 0, false);
        }

        public void Bot_UserLeft(object sender, UserEventArgs e)
        {
            try
            {
                var logChannel = e.Server.GetChannel(OutroChannelId);
                logChannel.SendMessage($"{e.User.Name} has left the magical Ember of Dreams (Gaming Community) Server. :sleepy: ");
            }
            catch
            {
                Console.WriteLine("You should install an outro channel with the \".setoutro\" command");
            }
        }

        public void Bot_MessageReceived(object sender, MessageEventArgs e)
        {
            /*
            Setwarnings(userId, 0, false);
            UsersToKick.Remove(("<@" + e.User.Id + ">").Replace("!", ""));
            Bannedusers.Remove(("<@" + e.User.Id + ">").Replace("!", ""));
            */
            int messagetosend, newPoints;
            bool isUserAdmin;
            string[] str;
            Setup(e, out messagetosend, out isUserAdmin, out str);

            if (Bannedusers.Contains(("<@" + e.User.Id + ">").Replace("!", "")))
            {
                e.User.Kick();
                try
                {
                    var logChannel = e.Server.GetChannel(OutroChannelId);
                    logChannel.SendMessage($"{e.User.Name} has been auto kicked from the magical Ember of Dreams (Gaming Community) Server.");
                }
                catch
                {
                    Console.WriteLine("You should install an outro channel with the \".setoutro\" command");
                }
            }

            else if (UsersToKick.Contains(("<@" + e.User.Id + ">").Replace("!", "")))
            {
                e.User.Kick();
                UsersToKick.Remove(("<@" + e.User.Id + ">").Replace("!", ""));
                Setwarnings(userId, 0, true);
                try
                {
                    var logChannel = e.Server.GetChannel(OutroChannelId);
                    logChannel.SendMessage($"{e.User.Name} has been auto kicked from the magical Ember of Dreams (Gaming Community) Server.");
                }
                catch
                {
                    Console.WriteLine("You should install an outro channel with the \".setoutro\" command");
                }
            }

            else if ((e.Message.RawText.ToLower().StartsWith(".ae") && str.Length == 3 && isUserAdmin) || (e.Message.RawText.ToLower().StartsWith(".ae") && str.Length == 3 && Admins.Contains("@" + e.User.Name + "#" + e.User.Discriminator)))
            {
                userId = str[Convert.ToInt32(Command.Uid)];
                var pointsToAdd = Convert.ToInt32(str[Convert.ToInt32(Command.Points)]);
                newPoints = CalcNewpoints(GetPoints(userId), Convert.ToInt32(str[Convert.ToInt32(Command.Points)]), true);
                if (pointsToAdd == 0)
                {
                    Send0message(e, messagetosend);
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

            else if ((e.Message.RawText.ToLower().StartsWith(".re") && str.Length == 3 && isUserAdmin) || (e.Message.RawText.ToLower().StartsWith(".re") && str.Length == 3 && Admins.Contains("@" + e.User.Name + "#" + e.User.Discriminator)))
            {
                Random rand_ = new Random();
                userId = str[Convert.ToInt32(Command.Uid)];
                var pointsToAdd = Convert.ToInt32(str[Convert.ToInt32(Command.Points)]);
                newPoints = CalcNewpoints(GetPoints(userId), Convert.ToInt32(str[Convert.ToInt32(Command.Points)]), false);
                if (pointsToAdd == 0)
                {
                    Send0message(e, messagetosend);
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

            else if (e.Message.RawText.ToLower().StartsWith(".refresh"))
            {
                userId = "@" + e.User.Name + "#" + e.User.Discriminator;
                if (!Admins.Contains(userId) && isUserAdmin)
                {
                    Admins.Add(userId);
                    e.Channel.SendMessage("You are succesfully added to my list of Admins!");
                }
                else
                {
                    if (isUserAdmin || Admins.Contains(userId))
                        e.Channel.SendMessage("Oh great master, you are already part of my list of Admins.");
                    else
                        e.Channel.SendMessage("Sorry, but I can't give you the 'Ember keeper' role.");
                }
            }

            else if ((e.Message.RawText.StartsWith(".remove") && isUserAdmin) || (e.Message.RawText.StartsWith(".remove") && Admins.Contains("@" + e.User.Name + "#" + e.User.Discriminator)))
            {
                userId = str[Convert.ToInt32(Command.Uid)];
                if (Admins.Contains(userId))
                {
                    Admins.Remove(userId);
                    e.Channel.SendMessage("Succesfully removed " + userId + " from the Admin list.");
                }
                else if (isUserAdmin || Admins.Contains("<@" + e.User.Id.ToString() + ">"))
                    e.Channel.SendMessage("That user is not an Admin for me.");
                else
                    e.Channel.SendMessage("You need to be an Admin to use this command. Try '.refresh' if you are.");
            }

            else if (e.Message.RawText.ToLower().StartsWith(".embers"))
            {
                userId = "@" + e.User.Name + "#" + e.User.Discriminator;
                e.Channel.SendMessage(e.User.Mention + " Your current points balance is " + GetPoints(userId));
            }

            else if (e.Message.RawText.ToLower().StartsWith(".warnings"))
            {
                userId = ("<@" + e.User.Id + ">").Replace("!", "");
                bool iskickeduser = false;
                if (Getwarnings(userId, true) == 1)
                {
                    iskickeduser = true;
                }
                if (!iskickeduser)
                {
                    try
                    {
                        Getwarnings(userId, false);
                    }
                    catch
                    {
                        Setwarnings(userId, 0, false);
                    }
                    if (Getwarnings(userId, false) > kickrate)
                        Setwarnings(userId, kickrate, false);
                    var warningsleft = kickrate - Getwarnings(userId, false);
                    if (warningsleft == 0)
                        e.Channel.SendMessage(e.User.Mention + " You have one more chance... If you don't change your behaviour you will be kicked from the server!");
                    else
                        e.Channel.SendMessage(e.User.Mention + " You have been warned " + Getwarnings(userId, false) + " times. After " + warningsleft + " more, you will be kicked from the server!");
                }
                else
                {
                    if (Getwarnings(userId, false) > kickrate)
                        Setwarnings(userId, kickrate, false);
                    var warningsleft = kickrate - Getwarnings(userId, false);
                    if (warningsleft == 0)
                        e.Channel.SendMessage(e.User.Mention + " You have one more chance... If you don't change your behaviour you will be banned from the server!");
                    else
                        e.Channel.SendMessage(e.User.Mention + " You have been warned " + (Getwarnings(userId, false) + 10) + " times. After " + warningsleft + " more, you will be banned from the server!");
                }
            }

            else if (e.Message.RawText.ToLower().StartsWith(".setoutro") && isUserAdmin)
            {
                OutroChannelId = e.Channel.Id;
                e.Channel.SendMessage("#" + e.Server.GetChannel(OutroChannelId) + " is the new outro channel.");
            }

            else if (e.Message.RawText.ToLower().StartsWith(".setkickrate") && isUserAdmin)
            {
                kickrate = Convert.ToInt32(str[1]);
                e.Channel.SendMessage(e.User.Mention + " Succesfully set the kickrate to " + kickrate + " warnings.");
            }

            else if ((e.Message.RawText.StartsWith(".warn") && isUserAdmin) || (e.Message.RawText.StartsWith(".warn") && Admins.Contains("@" + e.User.Name + "#" + e.User.Discriminator)))
            {
                userId = str[Convert.ToInt32(Command.Uid)];
                bool iskickeduser = false;
                if (Getwarnings(userId, true) == 1)
                {
                    iskickeduser = true;
                }
                try
                {
                    Getwarnings(userId, false);
                }
                catch
                {
                    Setwarnings(userId, 0, iskickeduser);
                }
                if (Getwarnings(userId, false) > kickrate)
                    Setwarnings(userId, kickrate, iskickeduser);
                if (!(Getwarnings(userId, false) == kickrate))
                    Setwarnings(userId, Getwarnings(userId, false) + 1, iskickeduser);
                else
                {
                    Setwarnings(userId, kickrate, iskickeduser);
                    if (!iskickeduser)
                    {
                        if (!UsersToKick.Contains(userId))
                        {
                            UsersToKick.Add(userId);
                        }
                    }
                    else
                    {
                        if (!Bannedusers.Contains(userId))
                        {
                            Bannedusers.Add(userId);
                        }
                    }
                }
            }
        }

        private void Setup(MessageEventArgs e, out int messagetosend, out bool isUserAdmin, out string[] str)
        {
            string currentuser = e.User.Name;
            Random rand = new Random();
            messagetosend = rand.Next(0, 3);
            isUserAdmin = false;
            str = e.Message.RawText.Split(Convert.ToChar(" "));
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
        }

        private static void Send0message(MessageEventArgs e, int messagetosend)
        {
            if (messagetosend == 0)
                e.Channel.SendMessage("We thought it was implied... but it needs to be a number greater than 0 ^.^");
            else if (messagetosend == 1)
                e.Channel.SendMessage("Are you a hacher? Please do not try to break my code!");
            else if (messagetosend == 2)
                e.Channel.SendMessage("Please do not make me feel that I'm useless. :cry: ");
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

        private int Getwarnings(string uid, bool kickedusersearch)
        {
            try
            {
                if (!Directory.Exists("Data/")) Directory.CreateDirectory("Data/");
                if (!File.Exists("Data/Warnings.txt"))
                {
                    using (var sw = new StreamWriter("Data/Warnings.txt"))
                    {
                    }
                }
                var lineArr = File.ReadAllLines("Data/Warnings.txt");

                foreach (var line in lineArr)
                {
                    var userAttributes = line.Split(Convert.ToChar(";"));
                    if (userAttributes[0] != uid) continue;
                    if (kickedusersearch)
                        return Convert.ToInt32(userAttributes[2]);
                    else
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

        private void Setwarnings(string uid, int Warnings, bool kickeduserset)
        {
            if (!Directory.Exists("Data/")) Directory.CreateDirectory("Data/");
            if (!File.Exists("Data/Warnings.txt"))
            {
                using (var sw = new StreamWriter("Data/Warnings.txt"))
                {

                }
            }
            var lines = File.ReadAllLines("Data/Warnings.txt").ToList();

            var lineFound = false;

            foreach (var line in lines)
            {
                if (!line.Contains(uid)) continue;
                var lineArr = lines.ToArray();
                var lineNumber = Array.FindIndex(lineArr, row => row.Contains(uid));
                if (line.Contains(uid))
                {
                    if (kickeduserset)
                        lineArr[lineNumber] = uid + ";" + Warnings + ";" + 1;
                    else
                        lineArr[lineNumber] = uid + ";" + Warnings;
                }
                lines = lineArr.ToList();
                lineFound = true;
            }

            if (lineFound)
            {
                File.WriteAllLines("data/Warnings.txt", lines);
            }
            else
            {
                lines.Add(uid + ";" + 0);
                File.WriteAllLines("data/Warnings.txt", lines);
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
