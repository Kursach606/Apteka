using System;
using System.Data.SqlClient;
using System.Windows;
using System.IO;
using System.Text;

using Microsoft.Win32;

namespace FOR
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Apteka;Integrated Security=True;";
        private Medication selectedMedication;

        public MainWindow(string login)
        {
            InitializeComponent();
            LoadMedications();
           
        }

        private void LoadMedications()
        {
            medicationsListBox.Items.Clear(); // очистка списка перед загрузкой
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Name FROM Medications ORDER BY Name", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    medicationsListBox.Items.Add(new Medication(name, connectionString));
                }
            }
        }




        //"Редактировать":
        private void OnMedicationSelected(object sender, RoutedEventArgs e)
        {
            selectedMedication = (Medication)medicationsListBox.SelectedItem;
            if (selectedMedication != null)
            {
                medicationNameTextBlock.Text = selectedMedication.Name;
                manufacturerTextBlock.Text = $"Производитель: {selectedMedication.Manufacturer}";
                activeSubstanceTextBlock.Text = $"Действующее вещество: {selectedMedication.ActiveSubstance}";
                usageInstructionsTextBlock.Text = $"Инструкция по применению: {selectedMedication.UsageInstructions}";
                sideEffectsTextBlock.Text = $"Побочные эффекты: {selectedMedication.SideEffects}";
                interactionsTextBlock.Text = $"Взаимодействие с другими лекарствами: {selectedMedication.Interactions}";

                editButton.IsEnabled = true;
            }
            else
            {
                medicationNameTextBlock.Text = "";
                manufacturerTextBlock.Text = "";
                activeSubstanceTextBlock.Text = "";
                usageInstructionsTextBlock.Text = "";
                sideEffectsTextBlock.Text = "";
                interactionsTextBlock.Text = "";

                editButton.IsEnabled = false;
            }
        }

        private void ClearInformation()
        {
            medicationNameTextBlock.Text = "";
            manufacturerTextBlock.Text = "";
            activeSubstanceTextBlock.Text = "";
            usageInstructionsTextBlock.Text = "";
            sideEffectsTextBlock.Text = "";
            interactionsTextBlock.Text = "";
        }
        private void OnEditMedicationClicked(object sender, RoutedEventArgs e)
        {
            if (selectedMedication != null)
            {
                EditMedicationWindow editMedicationWindow = new EditMedicationWindow(selectedMedication, connectionString);
                editMedicationWindow.ShowDialog();

                // Reload medications list after editing a medication
                LoadMedications();
            }
        }
        private void OnAddMedicationClicked(object sender, RoutedEventArgs e)
        {
            AddMedicationWindow addMedicationWindow = new AddMedicationWindow(connectionString);
            addMedicationWindow.ShowDialog();

            // Reload medications list after adding a new medication
            LoadMedications();
        }

        private void OnDeleteMedicationClicked(object sender, RoutedEventArgs e)
        {
            if (selectedMedication != null)
            {
                selectedMedication.DeleteFromDatabase();
                ClearInformation();
                LoadMedications();
            }
        }
        private void OnSearchClicked(object sender, RoutedEventArgs e)
        {
            string searchTerm = searchTextBox.Text.Trim();
            SearchMedicationByName(searchTerm);
        }
        private void SearchMedicationByName(string searchTerm)
        {
            medicationsListBox.Items.Clear(); // очистка списка перед загрузкой
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT Name FROM Medications WHERE Name LIKE @searchTerm ORDER BY Name", connection);
                command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    medicationsListBox.Items.Add(new Medication(name, connectionString));
                }
            }
        }
        private void ExportMedications()
        {
            Medication selectedMedication = medicationsListBox.SelectedItem as Medication;

            if (selectedMedication == null)
            {
                MessageBox.Show("Выберите медикамент для экспорта", "Ошибка");
                return;
            }

            try
            {
                // Show save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
                saveFileDialog.DefaultExt = "txt";

                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // Write medication data to the file
                    using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        writer.WriteLine($"Название препарата: {selectedMedication.Name}");
                        writer.WriteLine($"Производитель: {selectedMedication.Manufacturer}");
                        writer.WriteLine($"Действующее вещество: {selectedMedication.ActiveSubstance}");
                        writer.WriteLine($"Инструкция по применению: {selectedMedication.UsageInstructions}");
                        writer.WriteLine($"Побочные эффекты: {selectedMedication.SideEffects}");
                        writer.WriteLine($"Взаимодействие с другими лекарствами: {selectedMedication.Interactions}");
                    }

                    MessageBox.Show("Экспорт успешно выполнен", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка");
            }
        }
        private void OnExportClicked(object sender, RoutedEventArgs e)
        {
            ExportMedications();
        }



        public class Medication
        {
            // Свойства, которые могут быть изменены
            public string Name { get; set; }
            public string Manufacturer { get; set; }
            public string ActiveSubstance { get; set; }
            public string UsageInstructions { get; set; }
            public string SideEffects { get; set; }
            public string Interactions { get; set; }

            private string connectionString;

            public Medication(string name, string connectionString)
            {
                Name = name;
                this.connectionString = connectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT * FROM Medications WHERE Name=@name AND Manufacturer IS NOT NULL AND ActiveSubstance IS NOT NULL AND UsageInstructions IS NOT NULL AND SideEffects IS NOT NULL AND Interactions IS NOT NULL", connection);
                    command.Parameters.AddWithValue("@name", name);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"));
                        ActiveSubstance = reader.GetString(reader.GetOrdinal("ActiveSubstance"));
                        UsageInstructions = reader.GetString(reader.GetOrdinal("UsageInstructions"));
                        SideEffects = reader.GetString(reader.GetOrdinal("SideEffects"));
                        Interactions = reader.GetString(reader.GetOrdinal("Interactions"));
                    }
                }
            }
           

            // Метод, сохраняющий новые значения свойств класса Medication в базу данных
            public void UpdateInDatabase()
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        SqlCommand command = new SqlCommand("UPDATE Medications SET Manufacturer=@manufacturer, ActiveSubstance=@activeSubstance, UsageInstructions=@usageInstructions, SideEffects=@sideEffects, Interactions=@interactions WHERE Name=@name", connection);
                        command.Parameters.AddWithValue("@name", Name);
                        command.Parameters.AddWithValue("@manufacturer", Manufacturer);
                        command.Parameters.AddWithValue("@activeSubstance", ActiveSubstance);
                        command.Parameters.AddWithValue("@usageInstructions", UsageInstructions);
                        command.Parameters.AddWithValue("@sideEffects", SideEffects);
                        command.Parameters.AddWithValue("@interactions", Interactions);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Медикамент изменен", "Успех");
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при изменении медикамента", "Ошибка");
                        }
                    }
                }
           


            public void DeleteFromDatabase()
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand command = new SqlCommand("DELETE FROM Medications WHERE Name=@name", connection);
                    command.Parameters.AddWithValue("@name", Name);

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Медикамент удален", "Успех");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении медикамента", "Ошибка");
                    }
                }
            } 
        }
    }
}

