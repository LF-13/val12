using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace val12
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<int, int> _cart = new Dictionary<int, int>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            lblCurrentUser.Text = $"Пользователь: {Session.CurrentUsername}";
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                var table = Database.GetProducts();
                dgvProducts.DataSource = table;
                if (dgvProducts.Columns.Contains("Id"))
                {
                    dgvProducts.Columns["Id"].Visible = false;
                }
                dgvProducts.Columns["Name"].HeaderText = "Товар";
                dgvProducts.Columns["Price"].HeaderText = "Цена, ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message, "Ошибка");
            }
        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Выберите товар.", "Внимание");
                return;
            }

            var qty = (int)nudQuantity.Value;
            if (qty <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0.", "Внимание");
                return;
            }

            var row = dgvProducts.CurrentRow;
            var productId = (int)((DataRowView)row.DataBoundItem)["Id"];
            var name = (string)((DataRowView)row.DataBoundItem)["Name"];
            var price = (decimal)((DataRowView)row.DataBoundItem)["Price"];

            if (_cart.ContainsKey(productId))
                _cart[productId] += qty;
            else
                _cart[productId] = qty;

            RefreshCartDisplay();
        }

        private void RefreshCartDisplay()
        {
            lstCart.Items.Clear();
            decimal total = 0;

            var table = (DataTable)dgvProducts.DataSource;

            foreach (var kvp in _cart)
            {
                var productId = kvp.Key;
                var qty = kvp.Value;

                var rows = table.Select($"Id = {productId}");
                if (rows.Length == 0) continue;

                var name = (string)rows[0]["Name"];
                var price = (decimal)rows[0]["Price"];
                var lineTotal = price * qty;
                total += lineTotal;

                lstCart.Items.Add($"{name} x{qty} = {lineTotal:0.00} ₽");
            }

            lblTotal.Text = $"Итого: {total:0.00} ₽";
        }

        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста.", "Внимание");
                return;
            }

            try
            {
                Database.PlaceOrder(Session.CurrentUsername, _cart);
                MessageBox.Show("Заказ оформлен! Спасибо за покупку.", "Успех");
                _cart.Clear();
                RefreshCartDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении заказа: " + ex.Message, "Ошибка");
            }
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

