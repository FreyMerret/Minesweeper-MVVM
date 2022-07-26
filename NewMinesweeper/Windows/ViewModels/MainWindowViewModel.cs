using NewMinesweeper.Commands;
using NewMinesweeper.Controls;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace NewMinesweeper.Windows.ViewModels
{
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }

    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IDictionary<DifficultyLevel, Func<GameData>> _difficultySettings;


        private readonly Timer _timer;


        /// <summary> Текущий уровень сложности </summary>
        private DifficultyLevel _currentDifficultyLevel = DifficultyLevel.Easy;
        public DifficultyLevel CurrentDifficultyLevel
        {
            get => _currentDifficultyLevel;
            set => Set(ref _currentDifficultyLevel, value);
        }


        /// <summary> Игровые данные (размер поля, массив мин и т.д.) </summary>
        private GameData _currentGameData;
        public GameData CurrentGameData
        {
            get => _currentGameData;
            set => Set(ref _currentGameData, value);
        }


        /// <summary> Секунд прошло с начала игры </summary>
        private int _secondsGone;
        public int SecondsGone
        {
            get => _secondsGone;
            set => Set(ref _secondsGone, value);
        }


        /// <summary> Осталось обнаружить мин </summary>
        private int _minesLeft;
        public int MinesLeft
        {
            get => _minesLeft;
            set => Set(ref _minesLeft, value);
        }


        #region ------------------Commands------------------

        /// <summary>Комманда для закрытия приложения </summary>
        public ICommand CloseApplicationCommand { get; }

        private bool CanCloseApplicationCommandExecuted(object p) => true;

        private void OnCloseApplicationCommandExecuted(object p) => Application.Current.Shutdown();



        public ICommand NewGameCommand { get; }

        private bool CanNewGameCommandExecuted(object p) => true;

        private void OnNewGameCommandExecuted(object p) => NewGame();



        public ICommand ChangeDifficultyCommand { get; }

        private bool CanChangeDifficultyCommandExecuted(object p) => true;

        private void OnChangeDifficultyCommandExecuted(object p)
        {
            if (p is DifficultyLevel)   //чисто символическая проверка))) 
                ChangeDifficulty((DifficultyLevel)p);
        }

        #endregion 



        public MainWindowViewModel()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += (sender, args) => SecondsGone++;


            CloseApplicationCommand = new RelayCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecuted);
            NewGameCommand = new RelayCommand(OnNewGameCommandExecuted, CanNewGameCommandExecuted);
            ChangeDifficultyCommand = new RelayCommand(OnChangeDifficultyCommandExecuted, CanChangeDifficultyCommandExecuted);
            _difficultySettings = new Dictionary<DifficultyLevel, Func<GameData>>
            {
                { DifficultyLevel.Easy, () => CreateMinesField(9, 9, 10) },
                { DifficultyLevel.Normal, () => CreateMinesField(16, 16, 40) },
                { DifficultyLevel.Hard, () => CreateMinesField(16, 30, 90) }
            };

            NewGame();
        }

        private GameData CreateMinesField(int width, int height, int minesCount)
        {
            return new GameData(width, height, minesCount, NotificationMainWindowAboutUpdatingCurrentGameStage, NotificationMainWindowAboutUpdatingRemainingMines);
        }

        private void ChangeDifficulty(DifficultyLevel newDifficultyLevel)
        {
            _currentDifficultyLevel = newDifficultyLevel;
            NewGame();
        }

        private void NewGame()
        {
            _timer.Stop();
            SecondsGone = 0;
            CurrentGameData = _difficultySettings[_currentDifficultyLevel]();       //запускаем генерацию поля в зависимости от уровня сложности
        }

        private void NotificationMainWindowAboutUpdatingCurrentGameStage(object sender, NotifyMainWindowAboutUpdatingCurrentGameStageArgs args)
        {
            if (args.NewGameStage == GameStage.FirstButtonPressed)
            {
                _timer.Start();
            }
            else if (args.NewGameStage == GameStage.GameOver)
            {
                _timer.Stop();
            }
        }

        private void NotificationMainWindowAboutUpdatingRemainingMines(object sender, NotificationMainWindowAboutUpdatingRemainingMinesArgs args)
        {
            MinesLeft = args.MinesLeft;
        }
    }

    public class NotifyMainWindowAboutUpdatingCurrentGameStageArgs : EventArgs
    {
        public GameStage NewGameStage { get; set; }
    }

    public class NotificationMainWindowAboutUpdatingRemainingMinesArgs : EventArgs
    {
        public int MinesLeft { get; set; }
    }
}
