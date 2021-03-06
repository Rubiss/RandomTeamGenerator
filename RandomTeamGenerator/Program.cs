﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RandomTeamGenerator
{
    public class Program
    {
        public static Random Rnd = new Random();

        public static void Main(string[] args)
        {
            var isValid = false;

            var numPlayers = 0;
            while (!isValid)
            {
                Console.WriteLine("Enter a number of players:");
                var inPlayers = Console.ReadLine();

                if (inPlayers != null)
                {
                    isValid = int.TryParse(inPlayers, out numPlayers);
                }

            }

            isValid = false;

            var numMatches = 0;
            while (!isValid)
            {
                Console.WriteLine("Enter number of matches:");
                var inMatches = Console.ReadLine();

                if (inMatches != null)
                {
                    isValid = int.TryParse(inMatches, out numMatches);
                }
            }

            isValid = false;

            var numPlayersPerTeam = 0;
            while (!isValid)
            {
                var canParse = false;
                Console.WriteLine("Enter a number of players per team (2 or 3):");
                var inPlayersPerTeam = Console.ReadLine();
                if (inPlayersPerTeam != null)
                    canParse = int.TryParse(inPlayersPerTeam, out numPlayersPerTeam);

                if (canParse && (numPlayersPerTeam == 3 || numPlayersPerTeam == 2))
                {
                    isValid = true;
                }
            }
            

            var retry = "r";

            while (retry == "r")
            {
                var players = new List<Player>();
                for (var x = 1; x <= numPlayers; x++)
                {
                    players.Add(new Player
                    {
                        Id = x,
                        GamesPlayed = 0,
                        GamesPlayedAgainst = new int[numPlayers + 1],
                        GamesPlayedWith = new int[numPlayers + 1]
                    });
                }

                var matches = new List<Match>();
                while (matches.Count < numMatches)
                {
                    var team1 = new List<int>();
                    var team2 = new List<int>();
                    var match = new Match
                    {
                        Team1 = team1,
                        Team2 = team2
                    };

                    team1.Add(GetPlayer(1, match, matches, players));
                    match.Team1 = team1;
                    team1.Add(GetPlayer(1, match, matches, players));
                    match.Team1 = team1;
                    if (numPlayersPerTeam == 3)
                    {
                        team1.Add(GetPlayer(1, match, matches, players));
                        match.Team1 = team1;
                    }

                    team2.Add(GetPlayer(2, match, matches, players));
                    match.Team2 = team2;
                    team2.Add(GetPlayer(2, match, matches, players));
                    match.Team2 = team2;
                    if (numPlayersPerTeam == 3)
                    {
                        team2.Add(GetPlayer(2, match, matches, players));
                        match.Team2 = team2;
                    }

                    matches.Add(new Match { Team1 = team1, Team2 = team2 });
                }

                foreach (var match in matches)
                {
                    Console.WriteLine(match.ToString(numPlayersPerTeam));
                }

                Console.WriteLine("Enter 'r' to retry:");
                retry = Console.ReadLine();
            }
        }

        public class Match
        {
            public List<int> Team1 { get; set; }
            public List<int> Team2 { get; set; }

            public string ToString(int numPlayers)
            {
                var result = "";

                if (numPlayers == 3)
                {
                    result = result + Team1.First() + "," + Team1[1] + "," + Team1.Last() +
                             ",v," + Team2.First() + "," + Team2[1] + "," + Team2.Last();
                }

                if (numPlayers == 2)
                {
                    result = result + Team1.First() + "," + Team1.Last() +
                             ",v," + Team2.First() + "," + Team2.Last();
                }

                return result;
            }
        }

        public class Player
        {
            public int Id { get; set; }
            public int GamesPlayed { get; set; }

            public int[] GamesPlayedWith { get; set; }

            public int[] GamesPlayedAgainst { get; set; }
        }

        public static int GetPlayer(int team, Match match, List<Match> matches, List<Player> players)
        {
            var retPlayer = 0;
            var r = Rnd.Next(3);

            if (r == 0)
            {
                retPlayer = GetPlayerMethod1(team, match, matches, players);
            }

            if (r == 1)
            {
                retPlayer = GetPlayerMethod2(team, match, matches, players);
            }

            if (r == 2)
            {
                retPlayer = GetPlayerMethod3(team, match, matches, players);
            }

            //Increment a game played
            players.Find(x => x.Id == retPlayer).GamesPlayed++;

            return retPlayer;
        }

        public static int GetPlayerMethod1(int team, Match match, List<Match> matches, List<Player> players)
        {
            var retPlayer = 0;

            if (team == 1)
            {
                //We have not assigned a player yet
                if (match.Team1.Count == 0)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;
                }

                if (match.Team1.Count == 1)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.First()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id))
                        .ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team1.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.First()]).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the two players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.First()]++;
                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedWith[retPlayer]++;
                }

                if (match.Team1.Count == 2)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.First()])
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.Last()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team1.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.First()])
                            .Where(x => x.GamesPlayedWith[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.Last()]).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the three players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.Last()]++;
                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedWith[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedWith[retPlayer]++;
                }

            }

            if (team == 2)
            {
                if (match.Team2.Count == 0)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;

                }

                if (match.Team2.Count == 1)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team2.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the two players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.First()]++;
                    players.Find(x => x.Id == match.Team2.First()).GamesPlayedWith[retPlayer]++;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;
                }

                if (match.Team2.Count == 2)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.First()])
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.Last()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team2.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.First()])
                            .Where(x => x.GamesPlayedWith[match.Team2.Last()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.Last()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the three players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.Last()]++;
                    players.Find(x => x.Id == match.Team2.First()).GamesPlayedWith[retPlayer]++;
                    players.Find(x => x.Id == match.Team2.Last()).GamesPlayedWith[retPlayer]++;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;
                }
            }

            return retPlayer;
        }

        public static int GetPlayerMethod2(int team, Match match, List<Match> matches, List<Player> players)
        {
            var retPlayer = 0;

            if (team == 1)
            {
                //We have not assigned a player yet
                if (match.Team1.Count == 0)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;
                }

                if (match.Team1.Count == 1)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.First()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id))
                        .ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team1.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.First()]).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the two players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.First()]++;
                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedWith[retPlayer]++;
                }

                if (match.Team1.Count == 2)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.Last()])
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.First()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team1.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.First()])
                            .Where(x => x.GamesPlayedWith[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.Last()]).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the three players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.Last()]++;
                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedWith[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedWith[retPlayer]++;
                }

            }

            if (team == 2)
            {
                if (match.Team2.Count == 0)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;

                }

                if (match.Team2.Count == 1)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team2.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the two players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.First()]++;
                    players.Find(x => x.Id == match.Team2.First()).GamesPlayedWith[retPlayer]++;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;
                }

                if (match.Team2.Count == 2)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.First()])
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.Last()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team2.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.First()])
                            .Where(x => x.GamesPlayedWith[match.Team2.Last()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.Last()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the three players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.Last()]++;
                    players.Find(x => x.Id == match.Team2.First()).GamesPlayedWith[retPlayer]++;
                    players.Find(x => x.Id == match.Team2.Last()).GamesPlayedWith[retPlayer]++;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;
                }
            }

            return retPlayer;
        }

        public static int GetPlayerMethod3(int team, Match match, List<Match> matches, List<Player> players)
        {
            var retPlayer = 0;

            if (team == 1)
            {
                //We have not assigned a player yet
                if (match.Team1.Count == 0)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;
                }

                if (match.Team1.Count == 1)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.First()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id))
                        .ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team1.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.First()]).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the two players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.First()]++;
                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedWith[retPlayer]++;
                }

                if (match.Team1.Count == 2)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.First()])
                        .ThenBy(x => x.GamesPlayedWith[match.Team1.Last()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team1.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.First()])
                            .Where(x => x.GamesPlayedWith[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedWith[match.Team1.Last()]).ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the three players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team1.Last()]++;
                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedWith[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedWith[retPlayer]++;
                }

            }

            if (team == 2)
            {
                if (match.Team2.Count == 0)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;

                }

                if (match.Team2.Count == 1)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team2.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the two players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.First()]++;
                    players.Find(x => x.Id == match.Team2.First()).GamesPlayedWith[retPlayer]++;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;
                }

                if (match.Team2.Count == 2)
                {
                    var playerList = players.OrderBy(x => x.GamesPlayed)
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.Last()])
                        .ThenBy(x => x.GamesPlayedWith[match.Team2.First()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.Last()])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1[1]])
                        .ThenBy(x => x.GamesPlayedAgainst[match.Team1.First()])
                        .Where(x => !match.Team1.Contains(x.Id))
                        .Where(x => !match.Team2.Contains(x.Id)).ToList();

                    var minPlayer = playerList.First();

                    var playersToRandomize =
                        playerList.Where(x => x.GamesPlayed ==
                                              minPlayer.GamesPlayed)
                            .Where(x => x.GamesPlayedWith[match.Team2.First()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.First()])
                            .Where(x => x.GamesPlayedWith[match.Team2.Last()] ==
                                        minPlayer.GamesPlayedWith[match.Team2.Last()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.First()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.First()])
                            .Where(x => x.GamesPlayedAgainst[match.Team1[1]] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1[1]])
                            .Where(x => x.GamesPlayedAgainst[match.Team1.Last()] ==
                                        minPlayer.GamesPlayedAgainst[match.Team1.Last()])
                            .ToList();

                    var r = Rnd.Next(playersToRandomize.Count);

                    retPlayer = playersToRandomize[r].Id;

                    //Increment played with for the three players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedWith[match.Team2.Last()]++;
                    players.Find(x => x.Id == match.Team2.First()).GamesPlayedWith[retPlayer]++;
                    players.Find(x => x.Id == match.Team2.Last()).GamesPlayedWith[retPlayer]++;

                    //Increment played against for the players that just got paired
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.First()]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1[1]]++;
                    players.Find(x => x.Id == retPlayer).GamesPlayedAgainst[match.Team1.Last()]++;

                    players.Find(x => x.Id == match.Team1.First()).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1[1]).GamesPlayedAgainst[retPlayer]++;
                    players.Find(x => x.Id == match.Team1.Last()).GamesPlayedAgainst[retPlayer]++;
                }
            }

            return retPlayer;
        }
    }
}
