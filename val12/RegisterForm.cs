using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace val12
{
    public partial class RegisterForm : Form
    {
        private readonly Form1 _parentForm;
        private string _currentCaptcha;

        public RegisterForm()
        {
            InitializeComponent();
            GenerateCaptcha();
        }

        public RegisterForm(Form1 parentForm)
            : this()
        {
            _parentForm = parentForm;
        }

        private void GenerateCaptcha()
        {
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var sb = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }

            _currentCaptcha = sb.ToString();
            lblCaptchaValue.Text = _currentCaptcha;
            txtCaptcha.Clear();
        }

        private void btnRefreshCaptcha_Click(object sender, EventArgs e)
        {
            GenerateCaptcha();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            var fullName = txtFullName.Text.Trim();
            var email = txtEmail.Text.Trim();
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;
            var confirm = txtConfirmPassword.Text;
            var captchaInput = txtCaptcha.Text.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Заполните все поля (ФИО, Email, логин, пароль).", "Внимание");
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка");
                return;
            }

            if (!string.Equals(captchaInput, _currentCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Капча введена неверно.", "Ошибка");
                GenerateCaptcha();
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен быть не короче 4 символов.", "Внимание");
                return;
            }

            try
            {
                if (Database.RegisterUser(fullName, email, username, password, out var error))
                {
                    MessageBox.Show("Пользователь успешно зарегистрирован.", "OK");
                    GoBack();
                }
                else
                {
                    MessageBox.Show(error ?? "Не удалось зарегистрировать пользователя.", "Ошибка");
                    GenerateCaptcha();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message, "Ошибка");
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            if (_parentForm != null)
            {
                _parentForm.Show();
            }

            this.Close();
        }
    }
}

