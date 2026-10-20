using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using TiendaCrud.Models;

namespace TiendaCrud
{
    public partial class Form1 : Form
    {
        private readonly TiendaContext _context = new();
        private int _productoIdSeleccionado = 0;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        CargarCategorias();
            CargarProductos(GetDgvProductos1());
        }
        // --- MÉTODOS DE SOPORTE ---
        private void CargarCategorias()
        {
            // Insertar datos de prueba en categorías si está vacío
            if (!_context.Categorias.Any())
            {
                _context.Categorias.AddRange(
                new Categoria { Nombre = "Electrónica" },
               new Categoria { Nombre = "Ropa" }
                );
                _context.SaveChanges();
            }
            cmbCategorias.DataSource = _context.Categorias.ToList();
            cmbCategorias.DisplayMember = "Nombre";
            cmbCategorias.ValueMember = "Id";
        }

        private DataGridView GetDgvProductos1()
        {
            return dgvProductos;
        }

        private void CargarProductos(DataGridView dgvProductos1)
        {
            // READ: Carga productos incluyendo los datos de la tabla relacionada Categorias
            var productos = _context.Productos
            .Include(p => p.Categorias)
            .Select(p => new
            {
                p.Id,
                Producto = p.Nombre,
                p.Precio,
                Categoria = p.Categorias!.Nombre,
                p.CategoriaId
            }).ToList();
            dgvProductos.DataSource = productos;
            // Ocultar columna de ID de categoría
            if (dgvProductos.Columns["CategoriaId"] != null)
                dgvProductos1.Columns["CategoriaId"]?.Visible = false;
        }
       
    private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtPrecio.Clear();
            cmbCategorias.SelectedIndex = 0;
            _productoIdSeleccionado = 0;
            btnGuardar.Text = "Guardar";
        }
        // --- ACCIONES CRUD ---
        // CREATE / UPDATE
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNombre.Text) ||
            !decimal.TryParse(txtPrecio.Text, out decimal precio))
            {
                MessageBox.Show("Por favor, rellene los campos correctamente.");
                return;
            }
            if (_productoIdSeleccionado == 0)
            {
                // CREATE
                var nuevoProducto = new Productos
                {
                    Nombre = txtNombre.Text,
                    Precio = precio,
                    CategoriaId = (int)Convert.ToInt32(cmbCategorias.SelectedValue)
                };
                _context.Productos.Add(nuevoProducto);
            }
            else
            {
                // UPDATE
                var productoExistente = _context.Productos.Find(_productoIdSeleccionado);
                if (productoExistente != null)
                {
                    productoExistente.Nombre = txtNombre.Text;
                    productoExistente.Precio = precio;

                productoExistente.CategoriaId = (int)Convert.ToInt32(cmbCategorias.SelectedValue);
                }
            }
            _context.SaveChanges();
            CargarProductos(GetDgvProductos1());
            LimpiarFormulario();
            MessageBox.Show("Operación realizada con éxito.");
        }
        // Seleccionar registro para editar desde el DataGridView
        private void DgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProductos.Rows[e.RowIndex];
                _productoIdSeleccionado = Convert.ToInt32(row.Cells["Id"].Value);
                txtNombre.Text = Convert.ToString(row!.Cells["Producto"].Value);
                txtPrecio.Text = Convert.ToString(row!.Cells["Precio"].Value);
                cmbCategorias.SelectedValue =
                Convert.ToInt32(row.Cells["CategoriaId"].Value);
                btnGuardar.Text = "Actualizar";
            }
        }
        // DELETE
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_productoIdSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un producto de la lista para eliminar.");
                return;
            }
            var confirm = MessageBox.Show("¿Está seguro de eliminar este producto ? ", "Confirmar", MessageBoxButtons.YesNo);
        if (confirm == DialogResult.Yes)
            {
                var producto = _context.Productos.Find(_productoIdSeleccionado);
                if (producto != null)
                {
                    _context.Productos.Remove(producto);
                    _context.SaveChanges();
                    CargarProductos(GetDgvProductos1());
                    LimpiarFormulario();
                    MessageBox.Show("Producto eliminado.");
                }
            }
        }
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }
    }
}