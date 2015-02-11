using System.Collections.Generic;
using Newtonsoft.Json;

namespace Boxed.DataModel
{
    public class GameSet
    {
        public string Name { get; set; }
        public string Color { get; set; }

        public List<GameDefinition> Games { get; set; }

        [JsonIgnore]
        public GamePack GamePack { get; set; }

        public int GameCount
        {
            get { return Games != null ? Games.Count : 0; }
        }

        public int GamesPlayed
        {
            get { return GameData.Current.GamesPlayedForGameSet(this); }
        }

        public string DisplayGamesPlayed
        {
            get
            {
                return string.Format("{0} / {1}",
                    GameData.Current.GamesPlayedForGameSet(this),
                    GameCount);
            }
        }
    }
}