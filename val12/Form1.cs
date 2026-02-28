using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace val12
{
    public partial class Form1 : Form
    {
        private string _currentCaptcha;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GenerateCaptcha();
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;
            var captchaInput = txtCaptcha.Text.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Внимание");
                return;
            }

            if (!string.Equals(captchaInput, _currentCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Капча введена неверно.", "Ошибка");
                GenerateCaptcha();
                return;
            }

            try
            {
                if (Database.ValidateUser(username, password, out var isAdmin, out var isBlocked))
                {
                    if (isBlocked)
                    {
                        MessageBox.Show("Ваш аккаунт заблокирован администратором.", "Доступ запрещён");
                        GenerateCaptcha();
                        return;
                    }

                    Session.CurrentUsername = username;
                    Session.IsAdmin = isAdmin;

                    MessageBox.Show("Успешный вход.", "OK");

                    Form nextForm = isAdmin ? (Form)new AdminForm() : new MainForm();
                    nextForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка");
                    GenerateCaptcha();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при авторизации: " + ex.Message, "Ошибка");
            }
        }

        private void btnGoToRegister_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm(this);
            registerForm.Show();
            this.Hide();
        }
    }
}
