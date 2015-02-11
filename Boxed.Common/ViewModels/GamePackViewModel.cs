using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Boxed.DataModel;
using Brain.Extensions;
using PropertyChanged;

namespace Boxed.ViewModels
{

    [ImplementPropertyChanged]
    public class GamePackVM
    {
        public GamePack Data { get; set; }

        public List<GameSetVM> GameSets { get; set; }

        public GamePackVM(GamePack gamePack)
        {
            Data = gamePack;
            GameSets = new List<GameSetVM>();

            foreach (var gameSet in gamePack.GameSets)
                GameSets.Add(new GameSetVM(gameSet));
        }
    }

    [ImplementPropertyChanged]
    public class GameSetVM
    {
        public GameSet Data { get; set; }
        public List<GameVM> Games { get; set; }

        public Brush Color
        {
            get { return Data.Color.ToColorBrush(); }
        }

        public GameSetVM(GameSet gameSet)
        {
            Data = gameSet;
            Games = new List<GameVM>(gameSet.Games.Count);
            foreach (var game in gameSet.Games)
                Games.Add(new GameVM(game));
        }
    }

    [ImplementPropertyChanged]
    public class GameVM
    {
        public GameDefinition Definition { get; private set; }

        public string HighScore { get; set; }
        public Brush Color { get; set; }

        public GameVM(GameDefinition gameDefinition)
        {
            Definition = gameDefinition;

            UpdateHighScore();

            Color = Definition.GameSet.Color.ToColorBrush();
        }

        public void UpdateHighScore()
        {
            var highScore = GameManager.GetHighScore(Definition);
            if (highScore != null)
            {
                HighScore = string.Format(@"{0:m\:ss}", highScore.TimeTaken);
                HasHighScore = true;
            }
        }

        public int Level { get { return Definition.Index + 1; } }
        public bool HasHighScore { get; set; }
        public double BackgroundOpacity { get { return HasHighScore ? 1.0 : 0.2; } }

    }
}
