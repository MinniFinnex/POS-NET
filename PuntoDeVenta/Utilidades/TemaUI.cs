using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVenta.Utilidades
{
    public static class TemaUI
    {
        public static readonly Color AzulMarino = Color.FromArgb(15, 23, 42);      // #0F172A
        public static readonly Color AzulMarinoHover = Color.FromArgb(30, 41, 59); // #1E293B
        public static readonly Color AzulMarinoDown = Color.FromArgb(10, 15, 30);  // #0A0F1E

        public static readonly Color FondoClaro = Color.FromArgb(248, 250, 252);
        public static readonly Color Blanco = Color.White;
        public static readonly Color Borde = Color.FromArgb(226, 232, 240);
        public static readonly Color TextoOscuro = Color.FromArgb(15, 23, 42);
        public static readonly Color TextoSutil = Color.FromArgb(100, 116, 139);

        public static void AplicarBotonAzul(Button btn, Font? fuente = null)
        {
            btn.BackColor = AzulMarino;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AzulMarinoHover;
            btn.FlatAppearance.MouseDownBackColor = AzulMarinoDown;
            btn.Cursor = Cursors.Hand;
            if (fuente != null)
            {
                btn.Font = fuente;
            }
        }

        public static void AplicarBotonSecundario(Button btn, Font? fuente = null)
        {
            btn.BackColor = Color.White;
            btn.ForeColor = AzulMarino;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 245, 249);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(226, 232, 240);
            btn.Cursor = Cursors.Hand;
            if (fuente != null)
            {
                btn.Font = fuente;
            }
        }

        public static void AplicarEstiloGrid(DataGridView dgv)
        {
            dgv.BackgroundColor = Blanco;
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.GridColor = Borde;

            dgv.ColumnHeadersHeight = 36;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = AzulMarino;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgv.RowTemplate.Height = 34;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.ForeColor = TextoOscuro;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
            dgv.DefaultCellStyle.SelectionForeColor = AzulMarino;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }
    }
}
