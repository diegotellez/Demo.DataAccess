using Demo.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teste.Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //string connString = "data source=DESKTOP-D8M2AFL;initial catalog=demo;integrated security=true";
            //SqlServer sql = new SqlServer(connString);

            SqlServer sql = new SqlServer("DESKTOP-D8M2AFL", "demo");

            //var data = sql.GetStoreProcedureTable("usuariosGet");
            var data = sql.GetFromStoreProcedureAsObjects<Usuario>("usuariosGet");
            dataGridView1.DataSource = data;
        }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int Indetificacion { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }
}
