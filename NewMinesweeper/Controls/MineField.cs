using NewMinesweeper.Commands;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NewMinesweeper.Controls
{
    public class MineField : Grid
    {
        /// <summary>Дефолтный размер клетки </summary>
        private const int DefaultCellSize = 25;

        /// <summary> Количество неоткрытых ячеек </summary>
        private int _notOpenedCells;
        public int NotOpenedCells
        {
            get => _notOpenedCells;
            set
            {
                _notOpenedCells = value;
                if (_notOpenedCells == Data.MinesCount)
                    Win();
            }

        }

        /// <summary>Свойство, отвечающее за размер ячеек на поле </summary>
        public static readonly DependencyProperty CellSizeProperty = DependencyProperty.Register(
            nameof(CellSize),
            typeof(int),
            typeof(MineField),
            new PropertyMetadata(DefaultCellSize));

        /// <summary> Свойство, отвечающее за привязку к данным минного поля (кнопкам) </summary>
        public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register(
            nameof(Data),
            typeof(GameData),
            typeof(MineField),
            new PropertyMetadata(
                (sourceObject, args) =>
                {
                    var mineField = sourceObject as MineField;
                    mineField?.CurrentGameDataChanged((GameData)args.OldValue, (GameData)args.NewValue);
                }));

        public int CellSize
        {
            get => (int)GetValue(CellSizeProperty);
            set => SetValue(CellSizeProperty, value);
        }

        public GameData Data
        {
            get => (GameData)GetValue(DataProviderProperty);
            set => SetValue(DataProviderProperty, value);

        }

        private MineButton[,] _buttons;



        public MineField()
        {
            LeftButtonClickCommand = new RelayCommand(OnLeftButtonClickCommandExecuted, CanLeftButtonClickCommandExecuted);
            RightButtonClickCommand = new RelayCommand(OnRightButtonClickCommandExecuted, CanRightButtonClickCommandExecuted);
        }


        private void CurrentGameDataChanged(GameData oldValue, GameData newValue)
        {
            RemoveUnactualField(oldValue);
            DrawNewField();
            NotOpenedCells = newValue.Height * newValue.Width;
        }

        private void RemoveUnactualField(GameData oldValue)
        {
            if (oldValue == null)
                return;

            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
            Children.Clear();
            _buttons = null;
        }

        private void DrawNewField()
        {
            var provider = Data;
            _buttons = new MineButton[provider.Width, provider.Height];

            for (int i = 0; i < provider.Width; i++)
                RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });


            for (int j = 0; j < provider.Height; j++)
                ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });


            for (int i = 0; i < provider.Width; i++)
            {
                for (int j = 0; j < provider.Height; j++)
                {
                    var button = new MineButton(i, j) { Width = CellSize, Height = CellSize };
                    button.InputBindings.Add(new InputBinding(LeftButtonClickCommand, new MouseGesture(MouseAction.LeftClick))
                    {
                        CommandParameter = button
                    });
                    button.InputBindings.Add(new InputBinding(RightButtonClickCommand, new MouseGesture(MouseAction.RightClick))
                    {
                        CommandParameter = button
                    });

                    Children.Add(button);
                    SetColumn(button, j);
                    SetRow(button, i);

                    _buttons[i, j] = button;
                }
            }
        }


        #region ------------------Команды------------------

        #region LeftButtonClickCommand

        public ICommand LeftButtonClickCommand { get; }

        private bool CanLeftButtonClickCommandExecuted(object p)
        {
            var button = p as MineButton;
            if (button != null)
                return button.CurrentCellType != CellType.Flagged;
            return false;
        }

        private void OnLeftButtonClickCommandExecuted(object p)
        {
            var button = p as MineButton;
            if (button != null)
                LeftButtonClick(button.X, button.Y);
        }

        #endregion

        #region RightButtonClickCommand

        public ICommand RightButtonClickCommand { get; }

        private bool CanRightButtonClickCommandExecuted(object p)
        {
            var button = p as MineButton;
            if (button != null)
                return button.CurrentCellType == CellType.Button || button.CurrentCellType == CellType.Flagged; //ПКМ работает только на ненажатых кнопках
            return false;
        }

        private void OnRightButtonClickCommandExecuted(object p)
        {
            var button = p as MineButton;
            if (button != null)
            {
                if (button.CurrentCellType == CellType.Button && Data.MinesLeft > 0)    //флаг можно установить только если их лимит не исчерпан
                {
                    button.CurrentCellType = CellType.Flagged;
                    Data.MinesLeft--;
                    return;
                }

                if(button.CurrentCellType == CellType.Flagged)
                {
                    button.CurrentCellType = CellType.Button;
                    Data.MinesLeft++;
                }
            }
        }

        #endregion

        #endregion

        #region -------------Методы для команд-------------

        /// <summary> Метод для обработки нажатия ЛКМ </summary>
        public void LeftButtonClick(int x, int y)
        {
            if(Data.CurrentGameStage == GameStage.Started)
                Data.CurrentGameStage = GameStage.FirstButtonPressed;

            if ((int)_buttons[x, y].CurrentCellType < 9)               //если ячейка вскрыта и вокруг есть мины (от 1 до 8 мин вокруг)
            {
                if (EnoughtFlagsAround(x, y))                          //если вокруг столько же флагов, сколько и мин
                    OpenAround(x, y);                                  //Тогда открываем оставшиеся ячейки вокруг
                return;
            }

            if (_buttons[x, y].CurrentCellType == CellType.Button)
                OpenCell(x, y);
        }

        /// <summary> Метод открытия ячейки с проверкой на существование такой ячейки </summary>
        public void OpenCell(int x, int y)
        {
            if (x < 0 || y < 0 || x > Data.Width - 1 || y > Data.Height - 1 || _buttons[x, y].CurrentCellType != CellType.Button)
                return;

            var button = _buttons[x, y];
            if (Data.MinesPositions[x, y])
            {
                button.CurrentCellType = CellType.PressedMine;
                GameOver();
            }
            else
            {
                int minesAround = CountMinesAround(x, y);
                button.CurrentCellType = (CellType)minesAround;
                NotOpenedCells--;
                if (minesAround == 0)
                    OpenAround(x, y);
            }
        }

        /// <summary> Метод для подсчета количества мин вокруг </summary>
        public int CountMinesAround(int x, int y)
        {
            int result = 0;
            bool[] mines =
            {
                CheckMine(x-1, y-1),
                CheckMine(x-1, y),
                CheckMine(x-1, y+1),
                CheckMine(x, y+1),
                CheckMine(x+1, y+1),
                CheckMine(x+1, y),
                CheckMine(x+1, y-1),
                CheckMine(x, y-1)
            };
            foreach (var item in mines)
                if (item)
                    result++;
            return result;
        }

        /// <summary> Проверка наличия мины на ячейке с координатами X,Y </summary>
        public bool CheckMine(int x, int y)
        {
            if (x < 0 || y < 0 || x > Data.Width - 1 || y > Data.Height - 1)
                return false;
            return Data.MinesPositions[x, y];
        }

        /// <summary> Метод для вызова "метода открытия ячейки" для всех ячеек вокруг </summary>
        public void OpenAround(int x, int y)
        {
            OpenCell(x - 1, y - 1);
            OpenCell(x - 1, y);
            OpenCell(x - 1, y + 1);
            OpenCell(x, y + 1);
            OpenCell(x + 1, y + 1);
            OpenCell(x + 1, y);
            OpenCell(x + 1, y - 1);
            OpenCell(x, y - 1);
        }

        /// <summary> Проверка равенства поставленных флагов и количества мин вокруг ячейки</summary>
        public bool EnoughtFlagsAround(int x, int y)
        {
            return (int)_buttons[x, y].CurrentCellType == CountFlagsAround(x, y);
        }

        /// <summary> Метод для подсчета количества флагов вокруг </summary>
        public int CountFlagsAround(int x, int y)
        {
            int result = 0;
            bool[] flags =
            {
                CheckFlag(x-1, y-1),
                CheckFlag(x-1, y),
                CheckFlag(x-1, y+1),
                CheckFlag(x, y+1),
                CheckFlag(x+1, y+1),
                CheckFlag(x+1, y),
                CheckFlag(x+1, y-1),
                CheckFlag(x, y-1)
            };
            foreach (var item in flags)
                if (item)
                    result++;
            return result;
        }

        /// <summary> Проверка наличия флага на ячейке с координатами X,Y </summary>
        public bool CheckFlag(int x, int y)
        {
            if (x < 0 || y < 0 || x > Data.Width - 1 || y > Data.Height - 1)
                return false;
            return _buttons[x, y].CurrentCellType == CellType.Flagged;
        }

        /// <summary> Метод для отображения мин и неправильно установленных флагов в конце любой игры</summary>
        public void GameOver()
        {
            Data.CurrentGameStage = GameStage.GameOver;

            for (int x = 0; x < Data.Width; x++)
                for (int y = 0; y < Data.Height; y++)
                {
                    _buttons[x, y].IsEnabled = false;

                    if (_buttons[x, y].CurrentCellType == CellType.PressedMine)
                        continue;

                    if (!Data.MinesPositions[x, y] && _buttons[x, y].CurrentCellType == CellType.Flagged)
                        _buttons[x, y].CurrentCellType = CellType.WasNoMine;
                    if (Data.MinesPositions[x, y])
                        _buttons[x, y].CurrentCellType = CellType.Mine;
                }
        }

        /// <summary> Метод для вывода MessageBox с поздравлениями в случае победы</summary>
        public void Win()
        {
            GameOver();
            (new Thread(()=> MessageBox.Show("Поздравляю! Вы победили!"))).Start();
        }

        #endregion
    }
}
