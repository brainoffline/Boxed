using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boxed.Common;
using Boxed.ViewModels;
using PropertyChanged;

namespace Boxed.DataModel
{
    [ImplementPropertyChanged]
    public class GameManager
    {
        public static GamePackVM GetGamePack(string packName)
        {
            var gamePack = Current.GamePacks.FirstOrDefault(gp => gp.Name == packName);
            if (gamePack != null)
                return new GamePackVM(gamePack);
            return null;
        }

        public static GameDefinition GetGameDefinition(GameStartData startData)
        {
            var gamePack = Current.GamePacks.FirstOrDefault(gp => gp.Name == startData.PackName);
            var gameSet = gamePack.GameSets.FirstOrDefault(gs => gs.Name == startData.SetName);
            var gameDefinition = gameSet.Games[startData.Index];

            return gameDefinition;
        }


        public static GameHighScore GetHighScore(GameDefinition definition)
        {
            return GameData.Current.GetGameHighScore(definition);
        }


        private static GameManager _current;

        public static GameManager Current
        {
            get { return _current ?? (_current = new GameManager()); }
        }

        public List<GamePack> GamePacks { get; set; }
        public List<GameSet> AllGameSets { get; set; } 

        public GamePack SelectedGamePack { get; set; }
        public GameSet SelectedGameSet { get; set; }
        public int SelectedGameIndex { get; set; }



        private GameManager()
        {
            GamePacks = new List<GamePack>();
        }

        public async Task LoadKnownGamePacks()
        {
            var packNames = new List<string> {"PackA.json", "PackB.json"};

            foreach (var packName in packNames)
            {
                var gamePack = await ResourceUtil.Read<GamePack>("Packs", packName);

                GamePacks.Add(gamePack);

                foreach (var filename in gamePack.GameSetFilenames)
                {
                    var gameSet = await ResourceUtil.Read<GameSet>("Packs", filename);
                    gameSet.GamePack = gamePack;

                    for (int i = 0; i < gameSet.Games.Count; i++)
                    {
                        var definition = gameSet.Games[i];
                        definition.GameSet = gameSet;
                        definition.GamePack = gamePack;
                        definition.Index = i;
                    }

                    gamePack.GameSets.Add(gameSet);
                }
            }

            AllGameSets = new List<GameSet>();
            foreach (var gamePack in GamePacks)
                AllGameSets.AddRange(gamePack.GameSets);
        }

        public int GamesPlayedForPack(string packName)
        {
            return GameData.Current.GamesPlayedForGamePack(packName);
        }

    }
}
