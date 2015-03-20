using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Boxed.DataModel;
using Brain.Extensions;
using Newtonsoft.Json;
using PropertyChanged;

namespace Boxed.ViewModels
{
    public interface IRunnerView
    {
        void OnGameOver();
    }


    [ImplementPropertyChanged]
    public class GameRunner
    {
        public GameDefinition Definition { get; set; }
        public IRunnerView View { get; set; }

        public GameHighScore Best { get; set; }

        public string Level { get; set; }

        protected GameDefinition.GameShape SelectedShape { get; set; }

        protected int SelectedLeft { get; set; }
        protected int SelectedTop { get; set; }
        protected int SelectedRight { get; set; }
        protected int SelectedBottom { get; set; }

        public SquareDataViewModel[,] Grid { get; set; }

        public DateTime StartTime { get; set; }
        public TimeSpan CompletedTime { get; set; }
        public bool IsGameOver { get; set; }

        public string HighScore { get; set; }
        public bool IsHighScore { get; set; }
        public string PlayTime { get; set; }
        public string PlayTimeMinutes { get; set; }
        public string PlayTimeSeconds { get; set; }
        public Brush Color { get; set; }

        public GameRunner(IRunnerView view)
        {
            View = view;
        }

        public void SetGameDefinition(GameDefinition gameDefinition)
        {
            IsGameOver = false;
            IsHighScore = false;
            Definition = gameDefinition;

            StartTime = DateTime.Now;
            Grid = new SquareDataViewModel[Definition.Width, Definition.Height];
            Color = Definition.GameSet.Color.ToColorBrush();

            for (int x = 0; x < Definition.Width; x++)
            {
                for (int y = 0; y < Definition.Height; y++)
                {
                    Grid[x, y] = new SquareDataViewModel
                    {
                        BorderVisible = true,
                        HighColor = Color,
                        MedColor = new SolidColorBrush(Colors.LightBlue),
                    };
                }
            }

            foreach (var gameShape in Definition.GameShapes)
            {
                var x = gameShape.X + gameShape.CountX;
                var y = gameShape.Y + gameShape.CountY;

                var square = Grid[x, y];
                square.Fixed = true;
                square.Text = gameShape.CellCount.ToString();
                //square.TouchState = "1";
                square.Shape = gameShape;
            }

            Level = string.Format("Level {0}", Definition.Index + 1);

            GetHighScore();
        }

        public void GetHighScore()
        {
            Best = GameManager.GetHighScore(Definition);
            if (Best != null)
                HighScore = string.Format(@"{0:m\:ss}", Best.TimeTaken);
        }

        public void StartGame()
        {
            StartTime = DateTime.Now;
            IsGameOver = false;
        }

        public void UpdateCells(GameDefinition.GameShape selectedShape)
        {
            for (int x = 0; x < Definition.Width; x++)
            {
                for (int y = 0; y < Definition.Height; y++)
                {
                    var square = Grid[x, y];
                    if (square.Shape != selectedShape) continue;

                    var width = Definition.Width;
                    var height = Definition.Height;

                    var left = (x == 0) ? null : Grid[x - 1, y];
                    var top = (y == 0) ? null : Grid[x, y - 1];
                    var right = (x == width - 1) ? null : Grid[x + 1, y];
                    var bottom = (y == height - 1) ? null : Grid[x, y + 1];

                    square.LeftVisible = (left != null) && left.Shape == square.Shape;
                    square.TopVisible = (top != null) && top.Shape == square.Shape;
                    square.RightVisible = (right != null) && right.Shape == square.Shape;
                    square.BottomVisible = (bottom != null) && bottom.Shape == square.Shape;

                    var leftTop = ((x == 0) || (y == 0)) ? null : Grid[x - 1, y - 1];
                    var rightTop = ((x == width - 1) || (y == 0)) ? null : Grid[x + 1, y - 1];
                    var leftBottom = ((x == 0) || (y == height - 1)) ? null : Grid[x - 1, y + 1];
                    var rightBottom = ((x == width - 1) || (y == height - 1)) ? null : Grid[x + 1, y + 1];

                    square.LeftTopVisible = (leftTop != null) && leftTop.Shape == square.Shape;
                    square.RightTopVisible = (rightTop != null) && rightTop.Shape == square.Shape;
                    square.LeftBottomVisible = (leftBottom != null) && leftBottom.Shape == square.Shape;
                    square.RightBottomVisible = (rightBottom != null) && rightBottom.Shape == square.Shape;

                    square.BorderVisible = !(
                        square.LeftVisible || square.RightVisible || square.TopVisible || square.BottomVisible);

                    square.View.Update();
                }
            }
        }

        public bool TouchStart(int x, int y)
        {
            if (IsGameOver) return false;

            var cell = Grid[x, y];
            if (cell.Shape == null)
                return false;
            if (cell.TouchState == "2")
            {
                SelectedShape = cell.Shape;
                ClearSelectedShape();

                //if (string.IsNullOrWhiteSpace(cell.Text))
                if (!cell.Fixed)
                    return false;
            }

            SelectedShape = cell.Shape;
            SelectedLeft = x;
            SelectedTop = y;
            SelectedRight = x;
            SelectedBottom = y;
            cell.TouchState = "1";

            return true;
        }

        public bool TouchCell(int x, int y)
        {
            if (IsGameOver) return false;
            if (SelectedShape == null) return false;

            var cell = Grid[x, y];
            if (cell.Shape != null)
                return false;

            var allCells = Grid.Flatten().ToList();
            int selectedCount = allCells.Count(s => s.TouchState == "1");

            if (selectedCount >= SelectedShape.CellCount)
                return false;

            int changeLeft = Math.Min(SelectedLeft, x);
            int changeRight = Math.Max(SelectedRight, x);
            int changeTop = Math.Min(SelectedTop, y);
            int changeBottom = Math.Max(SelectedBottom, y);

            int changeCount = (changeRight - changeLeft + 1) * (changeBottom - changeTop + 1);
            if (changeCount > SelectedShape.CellCount)
                return false;

            for (int j = changeLeft; j <= changeRight; j++)
            {
                for (int k = changeTop; k <= changeBottom; k++)
                {
                    var changeCell = Grid[j, k];
                    if ((changeCell.Shape != null) && (changeCell.Shape != SelectedShape))
                        return false;
                }
            }
            SelectedLeft = changeLeft;
            SelectedTop = changeTop;
            SelectedRight = changeRight;
            SelectedBottom = changeBottom;

            for (int j = changeLeft; j <= changeRight; j++)
            {
                for (int k = changeTop; k <= changeBottom; k++)
                {
                    var changeCell = Grid[j, k];
                    changeCell.Shape = SelectedShape;
                    changeCell.TouchState = "1";
                }
            }

            //cell.Shape = SelectedShape;
            //cell.TouchState = "1";
            return true;
        }

        public bool TouchFinish(int x, int y, out bool cleared)
        {
            cleared = false;
            if (IsGameOver) return false;
            if (SelectedShape == null) return false;

            var allCells = Grid.Flatten().ToList();
            int selectedCount = allCells.Count(s => s.TouchState == "1");

            if ((SelectedLeft == x) && (SelectedTop == y) && (selectedCount == 1))
            {
                cleared = true;
                ClearSelectedShape();
                return false;
            }

            if (selectedCount == SelectedShape.CellCount)
            {
                foreach (var square in allCells.Where(s => s.TouchState == "1"))
                    square.TouchState = "2";
                UpdateCells(SelectedShape);
                if (CheckComplete())
                    return false;
                return true;
            }

            ClearSelectedShape();
            return false;
        }

        private void ClearSelectedShape()
        {
            if (SelectedShape == null)
                return;

            var allCells = Grid.Flatten().ToList();
            foreach (var square in allCells.Where(s => s.Shape == SelectedShape))
            {
                square.TouchState = "";
                square.BorderVisible = true;
                //if (String.IsNullOrWhiteSpace(square.Text))
                if (!square.Fixed)
                    square.Shape = null;
            }
            SelectedShape = null;
            SelectedLeft = -1;
            SelectedTop = -1;
            SelectedRight = -1;
            SelectedBottom = -1;
        }

        public bool CheckComplete()
        {
            var allCells = Grid.Flatten().ToList();
            var count = allCells.Count(s => s.TouchState == "2");
            if (count == Definition.Width * Definition.Height)
            {
                CompletedTime = DateTime.Now - StartTime;
                GameData.Current.LevelPlayed(Definition, CompletedTime);

                GetHighScore();

                IsGameOver = true;
                IsHighScore = CompletedTime == Best.TimeTaken;
                View.OnGameOver();

                return true;
            }
            return false;
        }

        public bool CanNext
        {
            get { return Definition.Index < Definition.GameSet.Games.Count - 1; }
        }

        public void Next()
        {
            if (!CanNext)
                return;

            var definition = GameManager.GetGameDefinition(new GameStartData
            {
                PackName = Definition.GamePack.Name,
                SetName = Definition.GameSet.Name,
                Index = Definition.Index + 1
            });

            SetGameDefinition(definition);
        }

        public void Reset()
        {
            IsGameOver = false;
            StartTime = DateTime.Now;

            foreach (var square in Grid)
            {
                square.BorderVisible = true;
                square.TouchState = "";
                //if (string.IsNullOrWhiteSpace(square.Text))
                if (!square.Fixed)
                    square.Shape = null;
            }

        }

        public void Tick()
        {
            var playTime = (IsGameOver ? CompletedTime : (DateTime.Now - StartTime));
            PlayTime = playTime.ToString(@"m\:ss");
            PlayTimeMinutes = playTime.TotalMinutes.ToString("0");
            PlayTimeSeconds = playTime.Seconds.ToString("00");
        }

        public void SaveState(Dictionary<string, object> state)
        {
            state["time"] = DateTime.Now - StartTime;

            var allCells = Grid.Flatten().ToList();
            var cells = allCells.Select(c => c.TouchState);

            var json = JsonConvert.SerializeObject(cells);

            state["grid"] = json;
        }

        public void LoadState(Dictionary<string, object> state)
        {
            if (!state.ContainsKey("time") ||
                !state.ContainsKey("grid")) return;

            var span = (TimeSpan)state["time"];
            StartTime = DateTime.Now - span;

            var json = state["grid"].ToString();
            var grid = JsonConvert.DeserializeObject<List<string>>(json);

            var allCells = Grid.Flatten().ToList();

            int offset = 0;
            foreach (var square in grid)
            {
                allCells[offset++].TouchState = square;
            }
        }
    }
}
