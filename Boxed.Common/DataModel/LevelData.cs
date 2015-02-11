using System;

namespace Boxed.DataModel
{
    public class GameHighScore
    {
        public string PackName { get; set; }
        public string SetName { get; set; }
        public int GameIndex { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }
}