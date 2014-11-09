using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using Moserware.Skills;

namespace ELO
{
    public class Program
    {
        public static bool Parallel = true;
        public enum SkillType
        {
            TrueSkill,
            GaussianElo,
            Ns2
        }
        public static SkillType SkillToUse = SkillType.Ns2;
        public static int PlayersPerTeam = 8;
        public double PersonalModifier = 0;
        public double PersonalPower = 0;
        public static int NumConcurrentGames = 100;
        public static int NumIterations = 100; //total # games = numConcurrentGames * numIterations
        public static double StrategyMultiplier = 0;
        public static int TotalNumPlayers = PlayersPerTeam * 2 * NumConcurrentGames;
        private static int numRuns;
        public static List<Point> Simulate(double personalMod, double personalPow, double matchmakingDelta, bool log)
        {
            var playersPerGame = PlayersPerTeam * 2;
            var totalNumPlayers = PlayersPerTeam * 2 * NumConcurrentGames;
            var allPlayers = new List<Player>();
            for (int i = 0; i < totalNumPlayers; i++)
            {
                var player = new Player(i) { Skill = Util.GaussianRandom(.5, .15) };
                player.Rating = new Rating(1500, 7);
                allPlayers.Add(player);
            }
            for (int i = 0; i < NumIterations; i++)
            {
                //have players join games, creating new ones when necessary
                allPlayers.Shuffle();

                var fullGames = new List<Game>();
                var fillingGames = new List<Game>();

                foreach (var player in allPlayers)
                {
                    if (fillingGames.Count > 0)
                    {
                        Game bestMatchGame = null;
                        double eloDelta = 9999;
                        foreach (var game in fillingGames)
                        {
                            var delta = Math.Abs(game.AverageRating - player.Rating.Mean);
                            if (delta < eloDelta)
                            {
                                eloDelta = delta;
                                bestMatchGame = game;
                                if (eloDelta < 10)
                                    break;
                            }
                        }
                        if (eloDelta < matchmakingDelta)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            bestMatchGame.AddPlayer(player);
                            if (!bestMatchGame.Joinable)
                            {
                                fillingGames.Remove(bestMatchGame);
                                fullGames.Add(bestMatchGame);
                            }
                            continue;
                        }
                    }
                    //if we're here, we need to make a new game

                    var newGame = new Game(personalMod, personalPow);
                    newGame.AddPlayer(player);
                    fillingGames.Add(newGame);
                }

                var gameLength = (int)Math.Pow(2, PlayersPerTeam - 1);
                if (Parallel)
                    System.Threading.Tasks.Parallel.ForEach(fullGames, game => game.Run(gameLength));
                else
                    foreach (var game in fullGames)
                    {
                        game.Run(gameLength);
                    }
            }
            var playerPoints = allPlayers.Select(player => new Point { X = player.Skill, Y = player.Rating.Mean }).ToList();
            if (log)
            {
                var linesToWrite = new List<string>(); // { "ELO,Accuracy,Evasiveness,Support,Strategy" };
                //linesToWrite.AddRange(allPlayers.Select(player => player.Rating + ", " + player.Accuracy + ", " + player.Evasiveness + ", " + player.Support + ", " + player.Strategy));
                linesToWrite.AddRange(allPlayers.Select(player => player.Skill + "," + player.Rating.Mean));
                var fileName = @"c:\temp\elo\" + "_" +
                    NumIterations + "_" +
                    personalMod + "_" +
                    personalPow + "_" +
                    matchmakingDelta + "_" +
                    ".csv";
                File.WriteAllLines(fileName, linesToWrite);
                Process.Start(@"C:\Program Files\Microsoft Office\Office15\EXCEL.EXE", fileName);
            }
            return playerPoints;
        }

        public struct Point
        {
            public double X;
            public double Y;
        }

        public static void LeastSquaresFitLinear(List<Point> points, out double m, out double b)
        {
            //Gives best fit of data to line Y = MC + B  
            int i;

            var x1 = 0.0;
            var y1 = 0.0;
            var xy = 0.0;
            var x2 = 0.0;

            for (i = 0; i < points.Count; i++)
            {
                x1 += points[i].X;
                y1 += points[i].Y;
                xy += points[i].X * points[i].Y;
                x2 += points[i].X * points[i].X;
            }

            double J = (points.Count * x2) - (x1 * x1);
            if (J != 0.0)
            {
                m = ((points.Count * xy) - (x1 * y1)) / J;
                b = ((y1 * x2) - (x1 * xy)) / J;
            }
            else
            {
                m = 0;
                b = 0;
            }
        }
    }
}
