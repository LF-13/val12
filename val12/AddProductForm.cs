using System;
using System.Globalization;
using System.Windows.Forms;

namespace val12
{
    public partial class AddProductForm : Form
    {
        public string ProductName => txtName.Text.Trim();
        public decimal ProductPrice => decimal.TryParse(txtPrice.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var p) ? p : 0;

        public AddProductForm()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара.", "Внимание");
                return;
            }
            if (ProductPrice <= 0)
            {
                MessageBox.Show("Введите корректную цену (больше 0).", "Внимание");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
