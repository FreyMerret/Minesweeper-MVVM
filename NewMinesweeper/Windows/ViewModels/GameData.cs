using NewMinesweeper.Windows.ViewModels;
using System;
using System.Collections.Generic;

namespace NewMinesweeper.Controls
{
    /// <summary> Стадии игры </summary>
    public enum GameStage : int
    {
        Started = 0,
        FirstButtonPressed = 1,
        GameOver = 2
    }

    public class GameData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int MinesCount { get; set; }
        public bool[,] MinesPositions { get; set; }

        /// <summary> Количество мин, которое осталось найти и событие для уведомления MainWindowViewModel</summary>
        private int _minesLeft;
        public int MinesLeft
        {
            get => _minesLeft;
            set
            {
                _minesLeft = value;
                NotificationMainWindowAboutUpdatingRemainingMinesEvent.Invoke(this, new NotificationMainWindowAboutUpdatingRemainingMinesArgs() { MinesLeft = value });
            }
        }

        public event EventHandler<NotificationMainWindowAboutUpdatingRemainingMinesArgs> NotificationMainWindowAboutUpdatingRemainingMinesEvent;

        /// <summary> Текущая стадия игры и событие для уведомления MainWindowViewModel</summary>
        private GameStage _currentGameStage;
        public GameStage CurrentGameStage
        {
            get => _currentGameStage;
            set
            {
                if (value != _currentGameStage)
                {
                    _currentGameStage = value;
                    NotificationMainWindowAboutUpdatingCurrentGameStageEvent.Invoke(this, new NotifyMainWindowAboutUpdatingCurrentGameStageArgs() { NewGameStage = value });
                }
            }
        }

        public event EventHandler<NotifyMainWindowAboutUpdatingCurrentGameStageArgs> NotificationMainWindowAboutUpdatingCurrentGameStageEvent;





        public GameData(int width, int height, int minesCount, EventHandler<NotifyMainWindowAboutUpdatingCurrentGameStageArgs> CGSEvent, EventHandler<NotificationMainWindowAboutUpdatingRemainingMinesArgs> RMEvent)
        {
            NotificationMainWindowAboutUpdatingCurrentGameStageEvent += CGSEvent;   //CGSEvent - Current Game Stage Event
            NotificationMainWindowAboutUpdatingRemainingMinesEvent += RMEvent;      //RMEvent - Remaining Mines Event

            Width = width;
            Height = height;
            MinesCount = minesCount;
            CurrentGameStage = GameStage.Started;
            MinesLeft = minesCount;

            GenerateMinesField();
        }

        private void GenerateMinesField()
        {
            var random = new Random();
            MinesPositions = new bool[Width, Height];

            for (int i = 0; i < MinesCount; i++)
                do
                {
                    int X = random.Next(0, Width);
                    int Y = random.Next(0, Height);

                    if (!MinesPositions[X, Y])
                    {
                        MinesPositions[X, Y] = true;
                        break;
                    }
                } while (true);
        }
    }
}
