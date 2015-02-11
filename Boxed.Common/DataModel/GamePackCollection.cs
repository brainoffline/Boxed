using System.Collections.Generic;
using System.Linq;

namespace Boxed.DataModel
{
    public class GamePack
    {
        public string Name { get; set; }
        public List<string> GameSetFilenames { get; set; }

        public List<GameSet> GameSets { get; set; }

        public GamePack()
        {
            GameSets = new List<GameSet>();
        }

        public int GameCount
        {
            get
            {
                return GameSets.Sum(pack => pack.Games.Count);
            }
        }
    
    }
}