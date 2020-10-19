using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static BankLibrary.BankClassLibrary;

namespace Bank.ViewModel
{
    /// <summary>
    /// Клас обрбабатывает входные данные нового клиента
    /// </summary>
    class ClientViewModel : MainVM
    {
        #region Поля

        /// <summary>
        /// Делегат события при поступлении денег на счет
        /// </summary>
        /// <param name="message"></param>
        public delegate void TransferDel(string message);
        /// <summary>
        /// Событие, возникающее при переводе денег на какой-либо счет
        /// </summary>
        public static event TransferDel TransferEvent = message => MessageBox.Show($"{message}");
        /// <summary>
        /// Поле коллекция, содержащая всех добавленных клиентов
        /// </summary>
        private static ObservableCollection<Client> allClients = new ObservableCollection<Client>();
        /// <summary>
        /// Поле счета клиента
        /// </summary>
        private int colBills;
        /// <summary>
        /// поле фамилии клиента
        /// </summary>
        private string lastName;
        /// <summary>
        /// поле имени клиента
        /// </summary>
        private string name;
        /// <summary>
        /// Поле отчества клиента
        /// </summary>
        private string patronymic;
        /// <summary>
        /// Поле статуса клиента
        /// </summary>
        private string clientStatus;
        /// <summary>
        /// Поле коллекция счетов клиента
        /// </summary>
        private ObservableCollection<InclusionModel> inclusions = new ObservableCollection<InclusionModel>();
        /// <summary>
        /// Поле выбранно клиента в таблице
        /// </summary>
        private Client selectedClient;
        /// <summary>
        /// Поле счета на который производится перевод
        /// </summary>
        private static int bill;
        /// <summary>
        /// Полу суммы перевода
        /// </summary>
        private static double newTransferSum;
        /// <summary>
        /// Поле статуса выполнения сохранения
        /// </summary>
        private static string progressBarStatus;
        /// <summary>
        /// Поле запуска\остановки прогресс бара
        /// </summary>
        private static bool progressBarStartStop;

        #endregion

        #region Свойства 

        public bool RunApp { get; set; }

        /// <summary>
        /// Свойство коллекция всех добавленных клиентов
        /// </summary>
        public ObservableCollection<Client> AllClients
        {
            get { return allClients; }
            set { allClients = value; OnPropertyChange(nameof(AllClients)); }
        }

        /// <summary>
        /// Свойство счета клиента
        /// </summary>
        public int ColBills
        {
            get { return colBills; }
            set { colBills = value; OnPropertyChange(nameof(ColBills)); }
        }

        /// <summary>
        /// Свойство фамилии клиента
        /// </summary>
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; OnPropertyChange(nameof(LastName)); }
        }

        /// <summary>
        /// Свойство имени клиента
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChange(nameof(Name)); }
        }

        /// <summary>
        /// Свойство отчества клиента
        /// </summary>
        public string Patronymic
        {
            get { return patronymic; }
            set { patronymic = value; OnPropertyChange(nameof(Patronymic)); }
        }

        /// <summary>
        /// свойство статуса клиента
        /// </summary>
        public string ClientStatus
        {
            get { return clientStatus; }
            set { clientStatus = value; OnPropertyChange(nameof(ClientStatus)); }
        }

        /// <summary>
        /// Свойство коллекция, содержащая все счета клиента
        /// </summary>
        public ObservableCollection<InclusionModel> Inclusions
        {
            get { return inclusions; }
            set { inclusions = value; OnPropertyChange(nameof(Inclusions)); }
        }

        /// <summary>
        /// Свойство выбранного клиента в таблице
        /// </summary>
        public Client SelectedClient
        {
            get { return selectedClient; }
            set { selectedClient = value; OnPropertyChange(nameof(SelectedClient)); }
        }

        /// <summary>
        /// Свойство статуса выполнения сохранения
        /// </summary>
        public string ProgressBarStatus
        {
            get { return progressBarStatus; }
            set { progressBarStatus = value; OnPropertyChange(nameof(ProgressBarStatus)); }
        }

        /// <summary>
        /// Свойство запуска\остановки прогресс бара
        /// </summary>
        public bool ProgressBarStartStop
        {
            get { return progressBarStartStop; }
            set { progressBarStartStop = value; OnPropertyChange(nameof(ProgressBarStartStop)); }
        }

        #endregion

        #region Команды

        /// <summary>
        /// Команда добавления клиента
        /// </summary>
        public ICommand AddClient
        {
            get
            {
                return new RelayCommand<object>(obj =>
                {
                    // Инициализация класса добавления клиента
                    Client client = new Client();

                    // Добавление нового клиента
                    var addClient = new Client(lastName, Name, Patronymic, ClientStatus);

                    // Добавление клиента в коллекцию
                    AllClients.Add(addClient);
                });
            }
        }

        /// <summary>
        /// Команда открытия окна управления счетами
        /// </summary>
        public ICommand OpenControl
        {
            get
            {
                return new RelayCommand<object>(obj =>
                {
                    try
                    {
                        // Условие определяет произошол ли выбор клиента в таблице
                        if (SelectedClient != null)
                        {
                            // Передача выбранного объекта клиента для использования в окне управления счетами 
                            InclusionViewModel inclusionModel = new InclusionViewModel(SelectedClient);

                            // определение контекста данных для использования и редактирования переданного экзепляра объекта
                            ControlWindow controlWindow = new ControlWindow()
                            {
                                DataContext = inclusionModel
                            };

                            // Открытие окна управления счетами
                            controlWindow.Show();

                            // Вызов события которое возникает при совпадении номера счета
                            foreach (var t in SelectedClient.Inclusions)
                            {
                                if (t.Bill == bill)
                                {
                                    TransferEvent?.Invoke($"На ваш счет {bill} поступила сумма {newTransferSum}");
                                }
                            }

                            bill = 0;
                        }
                        else
                        {
                            throw new BankException(1, "Не выбран клиент", "Выберите клиента из таблицы справа");
                        }
                    }
                    catch (BankException e) when (e.Error == 1)
                    {
                        MessageBox.Show($"Код ошибки = {e.Error}. {e.Message}. {e.Recommendations}");
                    }
                });
            }
        }

        /// <summary>
        /// Команда сохранения файла всех клиентов
        /// </summary>
        public ICommand SaveFile
        {
            get
            {
                return new RelayCommand<object>
                    (obg =>
                    {
                        // Вызов метода сохранения файла
                        Task.Run(() => SaveJsonFile());
                    });
            }
        }

        /// <summary>
        /// Команда загрузки файла
        /// </summary>
        public ICommand LoadFile
        {
            get
            {
                return new RelayCommand<object>
                    (obg =>
                    {
                        // вызов метода загрузки файла
                        Task.Run(() => LoadJsonFile());
                    });
            }
        }

        #endregion

        #region Конструкторы

        /// <summary>
        /// пустой конструктор
        /// </summary>
        public ClientViewModel()
        {
            StartApp();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Метод загрузки сохраненного файла
        /// </summary>
        /// <returns></returns>
        private void LoadJsonFile()
        {
            // Инициализация экземпляра класса диалогового окна загрузки файла
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Установка расширений файлов, которые можно загрузить
            openFileDialog.Filter = "Файл Json (*.json)|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                ProgressBarStatus = "Загрузка файла";

                ProgressBarStartStop = true;

                // Считывание файла
                string jsonConvert = File.ReadAllText($"{openFileDialog.FileName}");

                // Десериализация файла
                AllClients = JsonConvert.DeserializeObject<ObservableCollection<Client>>(jsonConvert);

                ProgressBarStatus = $"Файл загружен в {DateTime.Now.ToString("HH:mm")}";

                ProgressBarStartStop = false;
            }
        }

        /// <summary>
        /// Метод сохранения файла
        /// </summary>
        private void SaveJsonFile()
        {
            ProgressBarStatus = "Сохранение файла";

            ProgressBarStartStop = true;

            // Сериализация общей коллекции
            string json = JsonConvert.SerializeObject(allClients);

            // запись данных в файл
            File.WriteAllText(SaveFileDialogMethod("Clients"), json);

            ProgressBarStatus = "Сохранение файла завершено";

            ProgressBarStartStop = false;
        }

        /// <summary>
        /// Метод передает коллекцию клиентов для перевода денег
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<Client> AllClientsForTransfer()
        {
            return allClients;
        }

        /// <summary>
        /// Метод осуществляет перевод между счетами
        /// </summary>
        /// <param name="fromBill"></param>
        /// <param name="forBill"></param>
        /// <param name="transferSum"></param>
        public static void Transfer(int fromBill, int forBill, double transferSum)
        {
            // Цикл перебора всех клиентов
            foreach (var t in allClients)
            {
                // Цикл перебора счетов клиента
                foreach (var x in t.Inclusions)
                {
                    // Условие снятия денег со счета отправки
                    if (x.Bill == fromBill)
                    {
                        x.Inclusion -= transferSum;
                    }
                    // Условие зачисления денег на счет
                    if (x.Bill == forBill)
                    {
                        x.Inclusion += transferSum;
                    }
                }
            }

            try
            {
                if (forBill != 0)
                {
                    // Временное присвоение счета получателя
                    bill = forBill;
                }
                else
                {
                    throw new BankException(3, "Не выбран клиент или счет пополнения", "Выберите счет на который будет осучествляться перевод");
                }

                if (transferSum != 0)
                {
                    // Временное присвоение суммы перевода
                    newTransferSum = transferSum;
                }
                else
                {
                    throw new BankException(10, "Не введена сумма перевода", "Введите сумму, которую хотите перевести");
                }
            }
            catch (BankException e) when (e.Error == 3)
            {
                MessageBox.Show($"Код ошибки = {e.Error}. {e.Message}. {e.Recommendations}");
            }
            catch (BankException e) when (e.Error == 10)
            {
                MessageBox.Show($"Код ошибки = {e.Error}. {e.Message}. {e.Recommendations}");
            }
        }

        /// <summary>
        /// Метод изменения статуса выполнения создания файла
        /// </summary>
        async void StartApp()
        {
            ProgressBarStatus = "Создание тестового файла клиентов";

            ProgressBarStartStop = true;

            RunApp = await Task.Run(() => SaveMillionClients());

            if (RunApp == true)
            {
                ProgressBarStartStop = false;
            }
            else
            {
                ProgressBarStatus = "Создание файла отменено";

                ProgressBarStartStop = false;
            }
        }

        /// <summary>
        /// Метод написан для эмуляции большого числа клиентов и закрепления темы многопоточности
        /// </summary>
        Task<bool> SaveMillionClients()
        {
            Random random = new Random();

            List<string> LastNames = new List<string>() { "Иванов", "Петров", "Сидоров", "Кузнецов", "Попов", "Бобров", "Лопатов", "Семенов", "Смирнов" };
            List<string> Names = new List<string>() { "Иван", "Владимир", "Олег", "Сергей", "Виктор", "Андрей", "Ярослав", "Дмитрий", "Семен"};
            List<string> Patronymics = new List<string>() { "Евгеньевич", "Андреевич", "Александрович", "Иванович", "Семенович", "Сергеевич", "Викторович", "Дмитриевич", "Геннадьевич"};
            List<string> ClientStatus = new List<string>() { "Обычный", "VIP", "Корпоративный" };

            List<Client> clients = new List<Client>();

            for (int i = 0; i < 1_000_000; i++)
            {
                // Создание тестовой коллекции экземпляров классов
                Client client = new Client()
                {
                    Name = Names[random.Next(0, 9)],
                    LastName = LastNames[random.Next(0, 9)],
                    Patronymic = Patronymics[random.Next(0, 9)],
                    ClientStatus = ClientStatus[random.Next(0, 3)],
                    RegistrationDate = DateTime.Now.AddDays(-random.Next(1, 32)).AddMonths(-random.Next(1, 37)).ToString("dd.MM.yyyy")
                };

                // Добавление клиента в тестовую коллекцию
                clients.Add(client);
            }
            try
            {
                string StandartPath = $@"C:\MillionClients.json";

                // Сериализация общей коллекции
                string json = JsonConvert.SerializeObject(clients);

                // запись данных в файл
                File.WriteAllText(StandartPath, json);

                ProgressBarStatus = $@"Создание файла завершено. Место сохранения: {StandartPath}";

                return Task.FromResult(true);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"{ex.Message} Выберите место сохранения");

                string temp = SaveFileDialogMethod("MillionClients");

                if (temp != null)
                {
                    // Сериализация общей коллекции
                    string json = JsonConvert.SerializeObject(clients);

                    // запись данных в файл
                    File.WriteAllText(temp, json);
                }
                else 
                {
                    // Возврашение значения при отмене сохранения
                    return Task.FromResult(false);
                }

                ProgressBarStatus = $@"Создание файла завершено. Место сохранения: {temp}";

                // Возвращение значения при удачном сохранении
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Метод запуска диалогового окна сохранения файла
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        private string SaveFileDialogMethod(string Name)
        {
            // Инициализация экземпляра класса диалогового окна сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Установка расширений файлов, которые можно сохранить
            saveFileDialog.Filter = "Файл Json (*.json)|*.json";

            // Установка стандартного имени файла
            saveFileDialog.FileName = $"{Name}.json";

            if (saveFileDialog.ShowDialog() == true)
            {
                // Возвращение значения при удачном сохранении                
                return saveFileDialog.FileName;
            }
            else 
            {
                // Возврашение значения при отмене сохранения
                return null; 
            }
        }

        #endregion
    }
}
