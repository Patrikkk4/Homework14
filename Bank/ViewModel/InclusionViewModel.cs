using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using static BankLibrary.BankClassLibrary;

namespace Bank.ViewModel
{
    class InclusionViewModel : MainVM
    {
        #region Поля

        /// <summary>
        /// Делегат транзакции
        /// </summary>
        /// <param name="msg"></param>
        delegate void InclusionMoneyDel(string msg);
        /// <summary>
        /// Событие изменение суммы вклада
        /// </summary>
        static event InclusionMoneyDel InclusionMoneyEvent = mes => MessageBox.Show($"{mes}");
        /// <summary>
        /// Поле коллекция статусов клиентов
        /// </summary>
        private List<string> сlientStatusList = new List<string>() { "Обычный", "VIP", "Корпоративный"};
        /// <summary>
        /// Поле статуса клиента
        /// </summary>
        private string clientStatus;
        /// <summary>
        /// Поле Статуса вклада
        /// </summary>
        private string statusInclusion = "Активен";
        /// <summary>
        /// Поле суммы вклада
        /// </summary>
        private double inclusion;
        /// <summary>
        /// Поле процентной ставки
        /// </summary>
        private double percents;
        /// <summary>
        /// Поле итоговой суммы по окончании вклада
        /// </summary>
        private double sum;     
        /// <summary>
        /// Поле переданного объекта клиента
        /// </summary>
        private Client incModel;
        /// <summary>
        ///  Поле указывающее на капитализацию вклада
        /// </summary>
        private bool capitalize;
        /// <summary>
        /// Поле окончания вклада
        /// </summary>
        private DateTime dateEndInclusion;
        /// <summary>
        /// Поле коллекция, содержащая все вклады клиента
        /// </summary>
        private ObservableCollection<InclusionModel> inclusions = new ObservableCollection<InclusionModel>();
        /// <summary>
        /// Поле выбранного счета
        /// </summary>
        private InclusionModel selectInclusion;
        /// <summary>
        /// Поле вносимой суммы
        /// </summary>
        private double addMoney;
        /// <summary>
        /// поле внесения денег
        /// </summary>
        private bool radioAdd;
        /// <summary>
        /// поле снятия денег
        /// </summary>
        private bool radioWithdraw;

        #endregion

        #region Свойства

        /// <summary>
        /// Свойство коллекция содержащая все вклады клиента
        /// </summary>
        public ObservableCollection<InclusionModel> Inclusions
        {
            get { return inclusions; }
            set { inclusions = value; OnPropertyChange(nameof(Inclusions)); }
        }

        /// <summary>
        /// Свойство коллекция статусов клиентов
        /// </summary>
        public List<string> ClientStatusList
        {
            get { return сlientStatusList; }
            set { сlientStatusList = value; OnPropertyChange(nameof(ClientStatusList)); }
        }

        /// <summary>
        /// Свойство статуса вклада
        /// </summary>
        public string StatusInclusion
        {
            get { return statusInclusion; }
            set { statusInclusion = value; OnPropertyChange(nameof(StatusInclusion)); InclusionStatus(); }
        }

        /// <summary>
        /// свойство статуса клиента
        /// </summary>
        public string ClientStatus
        {
            get { return clientStatus; }
            set 
            { 
                clientStatus = value; 
                OnPropertyChange(nameof(ClientStatus));
            }
        }

        /// <summary>
        /// Свойство суммы вклада
        /// </summary>
        public double Inclusion
        {
            get { return inclusion; }
            set { inclusion = value; OnPropertyChange(nameof(Inclusion)); }
        }

        /// <summary>
        /// свойство процентной ставки
        /// </summary>
        public double Percents
        {
            get { return percents; }
            set { percents = value; OnPropertyChange(nameof(Percents)); }
        }

        /// <summary>
        /// Свойство итоговой суммы по окончании вклада
        /// </summary>
        public double Sum
        {
            get { return sum; }
            set { sum = value; OnPropertyChange(nameof(Sum)); }
        }

        /// <summary>
        /// Свойтсво переданного объекта клиента
        /// </summary>
        public Client IncModel
        {
            get { return incModel; }
            set { incModel = value; OnPropertyChange(nameof(IncModel)); }
        }

        /// <summary>
        /// Свойство, указывающее на капитализацию вклада
        /// </summary>
        public bool Capitalize
        {
            get { return capitalize; }
            set { capitalize = value; OnPropertyChange(nameof(Capitalize)); }
        }

        /// <summary>
        /// Свойство даты окончания вклада
        /// </summary>
        public DateTime DateEndInclusion
        {
            get { return dateEndInclusion; }
            set { dateEndInclusion = value; OnPropertyChange(nameof(DateEndInclusion)); }
        }

        /// <summary>
        /// Свойство копия даты окончания вклада
        /// </summary>
        public static string DateEndInclusionCopy { get; set; }

        /// <summary>
        /// Свойство выбранного вклада
        /// </summary>
        public InclusionModel SelectInclusion
        {
            get { return selectInclusion; }
            set { selectInclusion = value; OnPropertyChange(nameof(SelectInclusion)); }
        }

        /// <summary>
        /// Свойство вносимой суммы
        /// </summary>
        public double AddMoney
        {
            get { return addMoney; }
            set { addMoney = value; OnPropertyChange(nameof(AddMoney)); }
        }

        /// <summary>
        /// Свойство внесения денег
        /// </summary>
        public bool RadioAdd
        {
            get { return radioAdd; }
            set { radioAdd = value; OnPropertyChange(nameof(RadioAdd)); }
        }

        /// <summary>
        /// Свойство снятия денег
        /// </summary>
        public bool RadioWithdraw
        {
            get { return radioWithdraw; }
            set { radioWithdraw = value; OnPropertyChange(nameof(RadioWithdraw)); }
        }

        #endregion

        #region Команды

        /// <summary>
        /// Команда добавления нового вклада
        /// </summary>
        public ICommand AddNewInclusion
        {
            get 
            {
                return new RelayCommand<object>(obj =>
                {
                    // Заполнение конструкора вклада
                    InclusionModel inclusionModel = new InclusionModel(IncModel.ClientStatus, Inclusion, incModel.Percents, Capitalize, DateEndInclusion.ToString("dd.MM.yyyy"), StatusInclusion);

                    // добавление в коллекцию вкладов клиента
                    IncModel.Inclusions.Add(inclusionModel);

                    // Изменение количества вкладов клиента
                    IncModel.ColBills = IncModel.ColBills + 1;
                });
            }
        }

        /// <summary>
        /// Команда внесения суммы
        /// </summary>
        public ICommand AddMoneyCommand 
        {
            get 
            {
                return new RelayCommand<object>(obj =>
                {
                    ChangeMoneyMethod();                  
                });
            }
        }

        /// <summary>
        /// Команда открытия окна перевода средств
        /// </summary>
        public ICommand OpenTransfer
        {
            get
            {
                return new RelayCommand<object>(obj =>
                {
                    try
                    {
                        // Условие определяет произошел ли выбор клиента в таблице
                        if (SelectInclusion != null)
                        {
                            // Передача выбранного объекта клиента для использования в окне управления счетами 
                            TransferViewModel transferViewModel = new TransferViewModel(SelectInclusion);

                            // определение контекста данных для использования и редактирования переданного экзепляра объекта
                            TransferWindow transferWindow = new TransferWindow()
                            {
                                DataContext = transferViewModel
                            };

                            // Открытие окна управления счетами
                            transferWindow.Show();
                        }
                        else
                        {
                            throw new BankException(2, "Не выбран счет с которого будет осуществляться перевод.", @"Выберите счет");
                            //MessageBox.Show("Выберите счет с которого будет осуществляться перевод.");
                        }
                    }
                    catch(BankException e) when (e.Error == 2)
                    {
                        MessageBox.Show($"Код ошибки = {e.Error}. {e.Message}. {e.Recommendations}");
                    }
                });
            }
        }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Консруктор, принимающий переданный объект клиента
        /// </summary>
        /// <param name="incModel"></param>
        public InclusionViewModel(Client incModel)
        {
            InclusionModel inclusionModel = new InclusionModel();

            // Расчет итоговой суммы по всем вкладам при открытии окна управления
            foreach (var t in incModel.Inclusions)
            {
                t.Sum = inclusionModel.Calculate(t.DateInclusion, t.DateEndInclusion, t.Inclusion, t.Percents);
            }

            IncModel = incModel;

            Percents = IncModel.Percents;

            this.inclusions = IncModel.Inclusions;

            DateEndInclusion = DateTime.Today.AddMonths(12);

            // Вызов метода определения статуса вклада
            InclusionStatus();
        }

        /// <summary>
        /// пустой конструктор
        /// </summary>
        public InclusionViewModel()
        {
        }

        #endregion

        #region Методы

        /// <summary>
        /// Метод определяет статус вклада
        /// </summary>
        public void InclusionStatus()
        {
            // Копия даты окончания вклада
            DateEndInclusionCopy = DateEndInclusion.ToShortDateString();

            // Цикл, проверяющий дату окончания каждого вклада
            foreach (var t in Inclusions)
            {
                var tempEndDate = Convert.ToDateTime(t.DateEndInclusion);

                // Условие устанавливает статус вклада в зависимости от даты окончания вклада (Если текущая дата больше чем дата окончания вклада, то устанавливается статус "Закончен")
                if (tempEndDate < DateTime.Today)
                {
                    t.StatusInclusion = "Закончен";
                }
                else
                {
                    t.StatusInclusion = "Активен";
                }
            }
        }

        /// <summary>
        /// Метод меняет сумму вклада в зависимости от действия совершенного со вкладом
        /// </summary>
        private void ChangeMoneyMethod()
        {
            try
            {
                if (SelectInclusion != null)
                {
                    if (RadioAdd == true)
                    {
                        // Прибавление вносимой суммы в выбранный вклад
                        SelectInclusion.Inclusion = SelectInclusion.Inclusion + AddMoney;

                        InclusionMoneyEvent?.Invoke($"Внесена сумма {AddMoney} на счет {SelectInclusion.Bill}");
                    }
                    if (RadioWithdraw == true && SelectInclusion.Inclusion >= AddMoney)
                    {
                        // Вычитание вносимой суммы в выбранный вклад
                        SelectInclusion.Inclusion = SelectInclusion.Inclusion - AddMoney;

                        InclusionMoneyEvent?.Invoke($"Сумма {AddMoney} снята со счета {SelectInclusion.Bill}");
                    }
                    else if (RadioWithdraw == true && SelectInclusion.Inclusion < AddMoney)
                    {
                        InclusionMoneyEvent?.Invoke($"На вашем счете недостаточно средств. Максимальная сума которую вы можете снять {SelectInclusion.Inclusion}");
                    }

                    InclusionModel inclusionModel = new InclusionModel();

                    // Расчет итоговой суммы внесении дополнительной суммы
                    SelectInclusion.Sum = inclusionModel.Calculate(SelectInclusion.DateInclusion, SelectInclusion.DateEndInclusion, SelectInclusion.Inclusion, SelectInclusion.Percents);
                }
                else
                {
                    // Исключение вызывается если отсутствует фокус на один из счетов
                    throw new BankException(2, "Не выбран счет", @"Выберите счет пополнения\списания");
                }
            }
            catch(BankException e) when (e.Error == 2)
            {
                MessageBox.Show($"Код ошибки = {e.Error}. {e.Message}. {e.Recommendations}");
            }
        }        

        #endregion
    }
}
