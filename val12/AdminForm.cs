using System;
using System.Data;
using System.Windows.Forms;

namespace val12
{
    public partial class AdminForm : Form
    {
        public AdminForm()
        {
            InitializeComponent();
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            lblAdmin.Text = $"Админ: {Session.CurrentUsername}";
            LoadUsers();
            LoadProducts();
        }

        private void LoadUsers()
        {
            try
            {
                var table = Database.GetUsers();
                dgvUsers.DataSource = table;
                if (dgvUsers.Columns.Contains("UserID")) dgvUsers.Columns["UserID"].HeaderText = "UserID";
                if (dgvUsers.Columns.Contains("FullName")) dgvUsers.Columns["FullName"].HeaderText = "ФИО";
                if (dgvUsers.Columns.Contains("Email")) dgvUsers.Columns["Email"].HeaderText = "Email";
                if (dgvUsers.Columns.Contains("Login")) dgvUsers.Columns["Login"].HeaderText = "Логин";
                if (dgvUsers.Columns.Contains("Password")) dgvUsers.Columns["Password"].HeaderText = "Пароль";
                if (dgvUsers.Columns.Contains("Role")) dgvUsers.Columns["Role"].HeaderText = "Роль";
                if (dgvUsers.Columns.Contains("IsActive")) dgvUsers.Columns["IsActive"].HeaderText = "Активен";
                if (dgvUsers.Columns.Contains("CreatedAt")) dgvUsers.Columns["CreatedAt"].HeaderText = "Создан";
                if (dgvUsers.Columns.Contains("LastLogin")) dgvUsers.Columns["LastLogin"].HeaderText = "Последний вход";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message, "Ошибка");
            }
        }

        private int? GetSelectedUserId()
        {
            if (dgvUsers.CurrentRow == null) return null;
            var rowView = dgvUsers.CurrentRow.DataBoundItem as DataRowView;
            if (rowView == null) return null;
            return (int)rowView["UserID"];
        }

        private void btnBlock_Click(object sender, EventArgs e)
        {
            var userId = GetSelectedUserId();
            if (userId == null)
            {
                MessageBox.Show("Выберите пользователя.", "Внимание");
                return;
            }

            try
            {
                Database.SetUserBlocked(userId.Value, true);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка блокировки пользователя: " + ex.Message, "Ошибка");
            }
        }

        private void btnUnblock_Click(object sender, EventArgs e)
        {
            var userId = GetSelectedUserId();
            if (userId == null)
            {
                MessageBox.Show("Выберите пользователя.", "Внимание");
                return;
            }

            try
            {
                Database.SetUserBlocked(userId.Value, false);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка разблокировки пользователя: " + ex.Message, "Ошибка");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadProducts()
        {
            try
            {
                var table = Database.GetProducts();
                dgvProducts.DataSource = table;
                if (dgvProducts.Columns.Contains("Id")) dgvProducts.Columns["Id"].HeaderText = "ID";
                if (dgvProducts.Columns.Contains("Name")) dgvProducts.Columns["Name"].HeaderText = "Название";
                if (dgvProducts.Columns.Contains("Price")) dgvProducts.Columns["Price"].HeaderText = "Цена, ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message, "Ошибка");
            }
        }

        private int? GetSelectedProductId()
        {
            if (dgvProducts.CurrentRow == null) return null;
            var rowView = dgvProducts.CurrentRow.DataBoundItem as DataRowView;
            if (rowView == null) return null;
            return (int)rowView["Id"];
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            using (var form = new AddProductForm())
            {
                if (form.ShowDialog() != DialogResult.OK) return;
                try
                {
                    Database.AddProduct(form.ProductName, form.ProductPrice);
                    MessageBox.Show("Товар добавлен.", "OK");
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления товара: " + ex.Message, "Ошибка");
                }
            }
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            var productId = GetSelectedProductId();
            if (productId == null)
            {
                MessageBox.Show("Выберите товар.", "Внимание");
                return;
            }
            if (MessageBox.Show("Удалить выбранный товар?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                Database.DeleteProduct(productId.Value);
                MessageBox.Show("Товар удалён.", "OK");
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления товара: " + ex.Message, "Ошибка");
            }
        }

        private void btnRefreshProducts_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Session.CurrentUsername = null;
            Session.IsAdmin = false;

            var loginForm = new Form1();
            loginForm.Show();
            this.Close();
        }
    }
}

