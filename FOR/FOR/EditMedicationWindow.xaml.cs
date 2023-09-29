
using System.Windows;

using static FOR.MainWindow;

namespace FOR
{
    /// <summary>
    /// Логика взаимодействия для EditMedicationWindow.xaml
    /// </summary>
    public partial class EditMedicationWindow : Window
    {
        private Medication medication;
        private string connectionString;

        public EditMedicationWindow(Medication medication, string connectionString)
        {
            InitializeComponent();

            this.medication = medication;
            this.connectionString = connectionString;

            // Устанавливаем DataContext окна на текущий объект Medication,
            // чтобы поля TextBox автоматически заполнились текущими значениями
            DataContext = medication;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            // Обновляем значение свойств Medication из полей TextBox
            medication.Manufacturer = manufacturerTextBox.Text;
            medication.ActiveSubstance = activeSubstanceTextBox.Text;
            medication.UsageInstructions = usageInstructionsTextBox.Text;
            medication.SideEffects = sideEffectsTextBox.Text;
            medication.Interactions = interactionsTextBox.Text;

            // Сохраняем новые значения в базу данных
            medication.UpdateInDatabase();

            // Закрываем окно
            Close();
        }
    }
}
