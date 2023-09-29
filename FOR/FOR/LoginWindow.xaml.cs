using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;


namespace FOR
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void btnReg_Click(object sender, RoutedEventArgs e)
        {
            // создаем SqlConnection для передачи в конструктор RegistrationWindow
            SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Apteka;Integrated Security=True;");

            RegistrationWindow regWindow = new RegistrationWindow(connection);
            regWindow.Show();
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Password;

            // Хешируем введенный пароль
            var hashedPassword = HashPassword(password);

            // Проверяем логин и пароль в базе данных
            SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Apteka;Integrated Security=True;");
            SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Login=@login AND Password=@password", connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", hashedPassword); // Сравниваем с хешем пароля

            connection.Open();

            int result = (int)command.ExecuteScalar();

            if (result > 0)
            {
                MessageBox.Show("Аутентификация прошла успешно!");
                MainWindow mainWindow = new MainWindow(login);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль.");
            }

            connection.Close();
        }

        // Метод для хеширования пароля
        private string HashPassword(string password)
        {
            var sha256 = SHA256.Create();
            var salt = Encoding.UTF8.GetBytes("MySaltValue123"); // Здесь можно использовать свое значение соли
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var saltedPasswordBytes = new byte[salt.Length + passwordBytes.Length];
            Array.Copy(salt, 0, saltedPasswordBytes, 0, salt.Length);
            Array.Copy(passwordBytes, 0, saltedPasswordBytes, salt.Length, passwordBytes.Length);

            var hash = sha256.ComputeHash(saltedPasswordBytes);
            return Convert.ToBase64String(hash);
        }

    }
}
