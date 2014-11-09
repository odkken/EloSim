using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moserware.Skills;

namespace ELO
{
    public class Game
    {
        public List<Player> Team1;
        public List<Player> Team2;
        public double PersonalModifier;
        public double PersonalPower;
        public bool Joinable { get { return Team1.Count < Program.PlayersPerTeam || Team2.Count < Program.PlayersPerTeam; } }
        public Game(double personalMod, double personalPow)
        {
            PersonalModifier = personalMod;
            PersonalPower = personalPow;
            Team1 = new List<Player>();
            Team2 = new List<Player>();
        }


        public void AddPlayer(Player player)
        {
            var team = Util.NextDouble() < .5 ? 1 : 2;
            if (Team2.Count == Program.PlayersPerTeam || (team == 1 && Team1.Count < Program.PlayersPerTeam))
                Team1.Add(player);
            else
                Team2.Add(player);
            averageRating = Team1.Concat(Team2).Average(a => a.Rating.Mean);
        }

        private double averageRating;
        public double AverageRating
        {
            get { return averageRating; }
        }

        public void Run(int gameLength)
        {
            var team1Skill = Team1.Average(a => a.Skill);
            var team2Skill = Team2.Average(a => a.Skill);
            var oddsTeam1Winning = .5 * team1Skill / team2Skill;
            var team1Wins = oddsTeam1Winning > .5;//  Util.NextDouble() < oddsTeam1Winning;
            var rankArray = team1Wins ? new[] { 1, 2 } : new[] { 2, 1 };
            var gameInfo = GameInfo.DefaultGameInfo;
            //gameInfo.Beta = 10;
            gameInfo.InitialMean = 50;


            //var concattedTeams = Team1.Concat(Team2).ToList();
            //var tsTeam1 = new Team();
            //var tsTeam2 = new Team();
            //foreach (var player in Team1)
            //{
            //    tsTeam1.AddPlayer(player, player.Rating);
            //}
            //foreach (var player in Team2)
            //{
            //    tsTeam2.AddPlayer(player, player.Rating);
            //}
            switch (Program.SkillToUse)
            {

                //case Program.SkillType.TrueSkill:
                //    var tsRatings = TrueSkillCalculator.CalculateNewRatings(gameInfo, Teams.Concat(tsTeam1, tsTeam2), rankArray);
                //    foreach (var newRating in tsRatings)
                //    {
                //        concattedTeams.First(a => a.Id == newRating.Key.Id).Rating = newRating.Value;
                //    }
                //    break;
                //case Program.SkillType.GaussianElo:
                //    var gaussCalculator = new Moserware.Skills.Elo.GaussianEloCalculator();
                //    foreach (var player in Team1)
                //    {
                //        var onePlayerTeam = new Team(player, player.Rating);
                //        var newRatings = gaussCalculator.CalculateNewRatings(gameInfo, Teams.Concat(onePlayerTeam, tsTeam2), rankArray);
                //        player.Rating = newRatings.First(a => a.Key.Id == player.Id).Value;
                //    }
                //    foreach (var player in Team2)
                //    {
                //        var onePlayerTeam = new Team(player, player.Rating);
                //        var newRatings = gaussCalculator.CalculateNewRatings(gameInfo, Teams.Concat(tsTeam1, onePlayerTeam), rankArray);
                //        player.Rating = newRatings.First(a => a.Key.Id == player.Id).Value;
                //    }
                //    break;
                case Program.SkillType.Ns2:
                    var team1Elo = Team1.Average(a => a.Rating.Mean);
                    var team2Elo = Team2.Average(a => a.Rating.Mean);
                    foreach (var player in Team1)
                    {
                        var ratingChange = Util.EloChange(player.Rating.Mean, team2Elo, team1Wins ? 1 : 0, Util.KFactor(player.Rating.Mean));
                        if ((player.Skill > team1Skill && ratingChange > 0) ||
                            (player.Skill < team1Skill && ratingChange < 0))
                        {
                            ratingChange *= Math.Pow(Util.Clamp(1 + PersonalModifier * Math.Abs(player.Skill - team1Skill) / team1Skill, 0, 2), PersonalPower);
                        }
                        else
                        {
                            ratingChange *= Math.Pow(Util.Clamp(1 - PersonalModifier * Math.Abs(player.Skill - team1Skill) / team1Skill, 0, 2), PersonalPower);
                        }
                        player.Rating = new Rating(Util.Clamp(player.Rating.Mean + ratingChange, 0, 3000), player.Rating.StandardDeviation);
                    }
                    foreach (var player in Team2)
                    {
                        var ratingChange = Util.EloChange(player.Rating.Mean, team1Elo, team1Wins ? 0 : 1, Util.KFactor(player.Rating.Mean));
                        if ((player.Skill > team2Skill && ratingChange > 0) || (player.Skill < team2Skill && ratingChange < 0))
                        {
                            ratingChange *= Math.Pow(Util.Clamp(1 + PersonalModifier * Math.Abs(player.Skill - team2Skill) / team2Skill, 0, 2), PersonalPower);
                        }
                        else
                        {
                            ratingChange *= Math.Pow(Util.Clamp(1 - PersonalModifier * Math.Abs(player.Skill - team2Skill) / team2Skill, 0, 2), PersonalPower);
                        }

                        player.Rating = new Rating(Util.Clamp(player.Rating.Mean + ratingChange, 0, 3000), player.Rating.StandardDeviation);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


    }
}