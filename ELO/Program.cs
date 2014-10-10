using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace ELO
{

    class Program
    {

        public static bool Parallel = true;
        public static int PlayersPerTeam = 2;
        public static int NumConcurrentGames = 50;
        public static int NumIterations = 20000; //total # games = numConcurrentGames * numIterations
        public static int GameLength = 100; //
        public static double StrategyMultiplier = .25;
        static void Main(string[] args)
        {
            var totalNumPlayers = PlayersPerTeam * 2 * NumConcurrentGames;
            var allPlayers = new List<Player>();
            for (int i = 0; i < totalNumPlayers; i++)
            {
                var player = new Player();
                player.Initialize();
                allPlayers.Add(player);
            }

            //at this point you have a random list of players, each with 1200 elo
            for (int i = 0; i < NumIterations; i++)
            {
                allPlayers.Shuffle();
                //have players join games, creating new ones when necessary
                var games = new List<Game>();
                foreach (var player in allPlayers)
                {
                    var playerRating = player.Rating;
                    var gameToJoin =
                        games.FirstOrDefault(a => a.team1.Count < PlayersPerTeam || a.team2.Count < PlayersPerTeam && Math.Abs(a.AllPlayers.Average(b => b.Rating) - playerRating) < 50);
                    if (gameToJoin.team1 != null)
                    {
                        gameToJoin.AddPlayer(player);
                    }
                    else
                    {
                        if (games.Count < NumConcurrentGames)
                        {
                            var newGame = new Game
                            {
                                team1 = new List<Player>(),
                                team2 = new List<Player>()
                            };
                            newGame.AddPlayer(player);
                            games.Add(newGame);
                        }
                        else
                        {
                            var bestMatchGame = games.Where(a => a.team1.Count < PlayersPerTeam || a.team2.Count < PlayersPerTeam).MinBy(a => Math.Abs(a.AllPlayers.Average(b => b.Rating) - playerRating));
                            bestMatchGame.AddPlayer(player);
                        }
                    }
                }
                if (Parallel)
                    System.Threading.Tasks.Parallel.ForEach(games, game => game.Run(GameLength));
                else
                    foreach (var game in games)
                    {
                        game.Run(GameLength);
                    }


            }
            var linesToWrite = new List<string> { "ELO,Accuracy,Evasiveness,Support,Strategy" };
            linesToWrite.AddRange(allPlayers.Select(player => player.Rating + ", " + player.Accuracy + ", " + player.Evasiveness + ", " + player.Support + ", " + player.Strategy));
            File.WriteAllLines(@"c:\temp\elo.csv", linesToWrite);

        }

    }

    public struct Game
    {
        public List<Player> team1;
        public List<Player> team2;

        public IEnumerable<Player> AllPlayers
        {
            get { return team1.Concat(team2); }
        }

        public void AddPlayer(Player player)
        {
            player.ClearScore();
            if (team1.Count > team2.Count)
                team2.Add(player);
            else
                team1.Add(player);
        }


        public void Run(int gameLength)
        {
            var team1Support = team1.Average(a => a.Support);
            var team2Support = team2.Average(a => a.Support);
            for (int i = 0; i < gameLength; i++)
            {
                foreach (var player in team1)
                {
                    var opposingPlayer = team2[Util.Rng.Next(team2.Count)];
                    player.Fight(opposingPlayer, team1Support, team2Support);
                }
            }


            //team elo calc

            var team1Wins = team1.Sum(a => a.kills) * (1 + Program.StrategyMultiplier * team1.Average(a => a.Strategy)) >
                            team2.Sum(a => a.kills) * (1 + Program.StrategyMultiplier * team2.Average(a => a.Strategy));

            var team1Elo = team1.Average(a => a.Rating);
            var team2Elo = team2.Average(a => a.Rating);


            foreach (var player in team1)
            {
                player.UpdateElo(team2Elo, team1Wins);
            }
            foreach (var player in team2)
            {
                player.UpdateElo(team1Elo, !team1Wins);
            }
        }


    }

}
