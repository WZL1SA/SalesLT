using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SalesLT
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private OdbcConnection connection;
        private const string connectionString = "Driver={ODBC Driver 13 for SQL Server};Server=tcp:wzl-training.database.windows.net,1433;Database=wzl;Uid=su@wzl-training;Pwd=Cokolwiek#1;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;";
        //"Driver={ODBC Driver 13 for SQL Server};Server=tcp:wzl-training.database.windows.net,1433;Database=wzl;Uid=su@wzl-training;Pwd=Cokolwiek#1;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;";

        public ObservableCollection<GridDataContext> GridData { get; set; }

        private int rowToChange;

        private bool deleted;


        public MainWindow()
        {
            InitializeComponent();
            connection = new OdbcConnection(connectionString);
            GridData = new ObservableCollection<GridDataContext>();
            dataGrid.ItemsSource = GridData;

            dataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
            dataGrid.RowEditEnding += new EventHandler<DataGridRowEditEndingEventArgs>(RowEditingDone); //zdarzenie które emituje data grid

            dataGrid.KeyUp += DataGrid_KeyUp;

            GridData.CollectionChanged += GridData_CollectionChanged;
            
        }

        private void GridData_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(deleted)
            {
                if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach(var item in e.OldItems)
                    {
                        int result = DeleteItem(item);
                        StatusText.Text = string.Format("Usunięto {0} rekodrów", result);
                        deleted = false;
                    }
                }
            }
        }

        private int DeleteItem(object item)
        {
            CheckAndEstablishConnection();
            var command = connection.CreateCommand();
            var itemDc = (GridDataContext)item;
            command.CommandText = "DELETE FROM SalesLT.Customer WHERE CustomerID=?";
            command.Parameters.AddWithValue("@CustomerID", itemDc.CustomerID);
            var result = command.ExecuteNonQuery();
            return result;
        }

        private void DataGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
            {
                deleted = true;
            }
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if(rowToChange >0)
            {
                var rowItemColl = from p in GridData where p.CustomerID == rowToChange select p;
                var rowItem = rowItemColl.First();
                int result = UpdateItem(rowItem);
                StatusText.Text = string.Format("Zmodyfikowano {0} rekordów!", result);
                rowToChange = 0;
            }
        }

        private int UpdateItem(GridDataContext rowItem)
        {
            CheckAndEstablishConnection();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE SalesLT.Customer SET FirstName=?, LastName=?, Title=? WHERE CustomerID=?";
            command.Parameters.AddWithValue("@FirstName", rowItem.FirstName);
            command.Parameters.AddWithValue("@LastName", rowItem.LastName);
            command.Parameters.AddWithValue("@Title", rowItem.Title);
            command.Parameters.AddWithValue("@CustomerID", rowItem.CustomerID);
            var result = command.ExecuteNonQuery();
            return result;
        }

        private void RowEditingDone(object sender, DataGridRowEditEndingEventArgs e)
        {
            rowToChange = 0;
            if (e.EditAction == DataGridEditAction.Commit) //wykonywane gdy ktoś zatwierdzi enterem
            {
                if(MessageBox.Show("Czy chcesz zapisać zmiany?", "Na pewno?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                    GridDataContext rowItem = (GridDataContext)e.Row.Item; //dostajemy wiersz w którym ktoś edytował
                    rowToChange = rowItem.CustomerID; //jest ustawiane gdy ktoś wciśnie enter               
                }
                
            }

           
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            CheckAndEstablishConnection();

            //string commandString = "INSERT INTO SalesLT.Customer" +
            //    "(Title, FirstName, LastName, PasswordHash, PasswordSalt, NameStyle)" +
            //    "VALUES (";

            string commandString = "INSERT INTO SalesLT.Customer" +
                "(Title, FirstName, LastName, PasswordHash, PasswordSalt, NameStyle)" +
                "VALUES (?, ?, ?, ?, ?, ?);";



            string title = titleTb.Text;
            string firstName = nameTb.Text;
            string lastName = lastNameTb.Text;
            string password = passwordTb.Text;

            //commandString = commandString + "'" + title + "',";
            //commandString = commandString + "'" + firstName + "',";
            //commandString = commandString + "'" + lastName + "',";
            //commandString = commandString + "'" + password + "',";
            //commandString = commandString + "'" + password + "',";
            //commandString = commandString + "'False');";

            Debug.WriteLine("Commandstring " + commandString);

            OdbcCommand command = new OdbcCommand(commandString, connection); //obiekt polecenie do bazy danych

            command.Parameters.AddWithValue("@Title", title);  //lepsze rozwiązanie dodawania do bazy dane są zwalidowane i przesyłane jako parametr
            command.Parameters.AddWithValue("@FirstName", firstName); // dane te nie są kodem wykonywanym
            command.Parameters.AddWithValue("@LastName", lastName); // zabezpieczenie przed SQL injetion
            command.Parameters.AddWithValue("@PasswordHash", password);
            command.Parameters.AddWithValue("@PasswordSalt", password);
            command.Parameters.AddWithValue("@NameStyle", "False");


            //wykonanie zapytania do bazy
            var result = command.ExecuteNonQuery(); // metoda zwraca odpowiedź z bazy jako liczbę wierszy dotkniętych zapytaniem
            if (result > 0)
            {
                StatusText.Text = "Zapisano " + result + " wierszy!";
            }


        }

        private void CheckAndEstablishConnection()
        {
            if (!(connection.State == System.Data.ConnectionState.Open))
            {
                connection.Open();
                //jeśli kpołączenie nie jest otwarte trzeba je utworzyć
            }
        }

        private void readButton_Click(object sender, RoutedEventArgs e)
        {
            CheckAndEstablishConnection();

            var command = connection.CreateCommand();//twożymy obiekt połączenia do bazy

            var commandString = "SELECT FirstName, Title, CustomerID FROM SalesLT.Customer WHERE LastName=? ORDER BY customerid DESC";
            var lastName = lastNameTb.Text;

            command.CommandText = commandString;
            command.Parameters.AddWithValue("@LastName", lastName);

            var dataReader = command.ExecuteReader(System.Data.CommandBehavior.KeyInfo);
            if(dataReader.HasRows)
            {
                //dataReader.Read(); // przesówa nas do kolejnego wiersza

                ////var name = dataReader.GetString(dataReader.GetSchemaTable().Columns["FirstName"].Ordinal);
                ////var title = dataReader.GetString(dataReader.GetSchemaTable().Columns["Title"].Ordinal);


                //var name = dataReader.GetString(0);
                //var title = dataReader.GetString(1);

                GridData.Clear();

                //nameTb.Text = name;
                //titleTb.Text = title;

                var k = 0;
                while(dataReader.Read())
                {
                    k++;
                    GridData.Add(new GridDataContext()
                    {
                        LastName = lastName,
                        FirstName = dataReader.GetString(0),
                        Title = dataReader.GetString(1),
                        CustomerID = dataReader.GetInt32(2)
                    }
                    ); 
                }
                StatusText.Text = "Odczytano " + k + " wierszy!";
            }
            dataReader.Close();
        }
    }
}
