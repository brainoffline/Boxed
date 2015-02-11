using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Boxed.DataModel
{
    public class GameDefinition
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public List<GameShape> GameShapes { get; set; }
        public string Solution { get; set; }

        [JsonIgnore]
        public GamePack GamePack { get; set; }

        [JsonIgnore]
        public GameSet GameSet { get; set; }

        [JsonIgnore]
        public int Index { get; set; }

        public int DisplayIndex
        {
            get { return Index + 1; }
        }

        public string DisplayLevel
        {
            get { return string.Format("{0} / {1}", Index + 1, GameSet.GameCount); }
        }

        public string BackgroundColor
        {
            get
            {
                var score = GameManager.GetHighScore(this);
                if (score == null)
                    return "#33000000";
                return GameSet.Color;
            }
        }

        public string BestTime
        {
            get
            {
                var score = GameManager.GetHighScore(this);
                if ((score != null) && (score.TimeTaken > TimeSpan.Zero))
                {
                    var str = string.Format("{0}:{1:00}", 
                        Math.Round(score.TimeTaken.TotalMinutes, 0), 
                        score.TimeTaken.Seconds);
                    return str;
                }
                return "";
            }
        }

        public class GameShape
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int CellCount { get { return Width * Height; } }
            public int CountX { get; set; }
            public int CountY { get; set; }

            public override string ToString()
            {
                return string.Format("Shape [{0},{1}] [{2},{3}] [{4},{5}] {6}",
                    X, Y, Width, Height, CountX, CountY, CellCount);
            }
        }

        public string GetKey()
        {
            return string.Format("{0},{1},{2}", GamePack.Name, GameSet.Name, Index);
        }
    }
}