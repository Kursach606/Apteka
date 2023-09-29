using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace FOR
{
    public partial class RegistrationWindow : Window
    {
        private SqlConnection _connection;
        private string _captchaText;

        public RegistrationWindow(SqlConnection connection)
        {
            InitializeComponent();

            _connection = connection;

            // Обработчик события клика по кнопке "Refresh Captcha"
            btnRefreshCaptcha.Click += (sender, args) => GenerateCaptcha();

            // Обработчик события клика по кнопке "Register"
            btnRegister.Click += (sender, args) => Register();
        }

        // Метод для генерации Captcha
        private void GenerateCaptcha()
        {
            var random = new Random();
            _captchaText = "";
            for (int i = 0; i < 6; i++)
                _captchaText += (char)(random.Next(48, 58) + random.Next(0, 2) * 7);

            txtCaptcha.Text = _captchaText;
        }

        // Метод для проверки правильности ввода Captcha
        private bool CheckCaptcha()
        {
            return txtCaptcha.Text == _captchaText;
        }

        // Метод для проверки правильности заполнения формы
        private bool ValidateForm()
        {
            // Проверяем, что все поля заполнены
            if (string.IsNullOrWhiteSpace(txtLogin.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword.Password))
                
            {
                lblError.Content = "Please fill in all fields";
                lblError.Visibility = Visibility.Visible;
                return false;
            }

            // Проверяем, что пароли совпадают
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                lblError.Content = "Passwords do not match";
                lblError.Visibility = Visibility.Visible;
                return false;
            }

            // Проверяем правильность ввода Captcha
            if (string.IsNullOrWhiteSpace(txtCaptcha.Text) || txtCaptcha.Text != _captchaText)
            {
                lblError.Content = "Invalid Captcha";
                lblError.Visibility = Visibility.Visible;
                return false;
            }

            // Если все проверки пройдены успешно, возвращаем true
            return true;
        }

        // Метод для регистрации нового пользователя
        private void Register()
        {
            // Проверяем правильность заполнения формы
            if (!ValidateForm())
                return;

            try
            {
                // Хешируем пароль
                var passwordHash = HashPassword(txtPassword.Password);

                using (var connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Apteka;Integrated Security=True;"))
                {
                    connection.Open();

                    // Создаем команду для выполнения SQL-запроса на добавление нового пользователя
                    using (var cmd = new SqlCommand("INSERT INTO Users (Login, Password) VALUES (@login, @password)", connection))
                    {
                        cmd.Parameters.AddWithValue("@login", txtLogin.Text);
                        cmd.Parameters.AddWithValue("@password", passwordHash);
                        cmd.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                // Очищаем поля формы и генерируем новую Captcha
                txtLogin.Clear();
                txtPassword.Clear();
                txtConfirmPassword.Clear();
                txtCaptcha.Clear();
                GenerateCaptcha();

                // Отображаем сообщение об успешной регистрации
                MessageBox.Show("Регистрация прошла успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
            catch (Exception ex)
            {
                // Если произошла ошибка при выполнении запроса, отображаем ее пользователю
                lblError.Content = ex.Message;
                lblError.Visibility = Visibility.Visible;
            }
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

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}