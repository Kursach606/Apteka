using System.Data.SqlClient;
using System.Windows;


namespace FOR
{
    /// <summary>
    /// Логика взаимодействия для AddMedicationWindow.xaml
    /// </summary>
    public partial class AddMedicationWindow : Window
    {
        private string connectionString;

        public AddMedicationWindow(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
        }

        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            // Get medication information from form fields
            string name = nameTextBox.Text;
            string manufacturer = manufacturerTextBox.Text;
            string activeSubstance = activeSubstanceTextBox.Text;
            string usageInstructions = usageInstructionsTextBox.Text;
            string sideEffects = sideEffectsTextBox.Text;
            string interactions = interactionsTextBox.Text;

            // Check that all fields are filled in
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(manufacturer) || string.IsNullOrEmpty(activeSubstance) || string.IsNullOrEmpty(usageInstructions) || string.IsNullOrEmpty(sideEffects) || string.IsNullOrEmpty(interactions))
            {
                MessageBox.Show("Заполните все поля", "Ошибка");
                return;
            }

            // Сохранение медикамента в базе данных
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Проверяем, существует ли медикамент с таким названием
                SqlCommand countCommand = new SqlCommand("SELECT COUNT(*) FROM Medications WHERE Name=@name", connection);
                countCommand.Parameters.AddWithValue("@name", name);
                int count = (int)countCommand.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("Медикамент с таким названием уже существует", "Ошибка");
                    return;
                }

                // Добавляем новый медикамент в базу данных
                SqlCommand insertCommand = new SqlCommand("INSERT INTO Medications (Name, Manufacturer, ActiveSubstance, UsageInstructions, SideEffects, Interactions) VALUES (@name, @manufacturer, @activeSubstance, @usageInstructions, @sideEffects, @interactions)", connection);
                insertCommand.Parameters.AddWithValue("@name", name);
                insertCommand.Parameters.AddWithValue("@manufacturer", manufacturer);
                insertCommand.Parameters.AddWithValue("@activeSubstance", activeSubstance);
                insertCommand.Parameters.AddWithValue("@usageInstructions", usageInstructions);
                insertCommand.Parameters.AddWithValue("@sideEffects", sideEffects);
                insertCommand.Parameters.AddWithValue("@interactions", interactions);

                int result = insertCommand.ExecuteNonQuery();
                if (result > 0)
                {
                    MessageBox.Show("Медикамент добавлен", "Успех");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении медикамента", "Ошибка");
                }
            }
        }

                private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
                {
                    this.Close();
                }

    }
}