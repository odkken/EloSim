
namespace ELO
{
    public class Player
    {
        public static double TeamSupportFactor = .5; // 1 makes it equally weighted with evasiveness when determining the outcome of a fight
        public static double statsDeviation = .1;


        public double Rating;
        public double Accuracy { get; private set; }
        public double Evasiveness { get; private set; }
        public double Support { get; private set; }
        public double Strategy { get; private set; }

        public int kills;
        public int deaths;

        public void ClearScore()
        {
            kills = 0;
            deaths = 0;
        }

        public void UpdateElo(double opposingTeamElo, bool won)
        {
            var performance = (won ? 1 : 0); //50% due to winning or losing
            //+ (deaths == 0 ? .25 : .25 * (1 - 1 / ((kills / deaths) + 1))) //another 25% due to k/d ratio
            //               + .25 * Support; //finally factor in support for individual players
            Rating = Util.Clamp(Rating + Util.EloChange(Rating, opposingTeamElo, performance, Util.KFactor(Rating)), 0, 3000);
        }

        public void Initialize()
        {
            Accuracy = Util.Clamp(Util.GaussianRandom(.5, statsDeviation), 0, 1);
            Evasiveness = Util.Clamp(Util.GaussianRandom(.5, statsDeviation), 0, 1);
            Support = Util.Clamp(Util.GaussianRandom(.5, statsDeviation), 0, 1);
            Strategy = Util.Clamp(Util.GaussianRandom(.5, statsDeviation), 0, 1);
            Rating = 1200;
        }

        public void Fight(Player opponent, double myAvgTeamSupport, double theirAvgTeamSupport)
        {
            var myScore = (Accuracy / (opponent.Evasiveness + TeamSupportFactor * theirAvgTeamSupport));
            var theirScore = (opponent.Accuracy / (Evasiveness + TeamSupportFactor * myAvgTeamSupport));
            if (Accuracy > opponent.Accuracy)
            {
                kills++;
                opponent.deaths++;
            }
            else
            {
                deaths++;
                opponent.kills++;
            }
        }

    }
}