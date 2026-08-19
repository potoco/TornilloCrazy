namespace AgenteUIProducto
{
    partial class Central
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            administrarToolStripMenuItem = new ToolStripMenuItem();
            proveedoresToolStripMenuItem = new ToolStripMenuItem();
            productosToolStripMenuItem = new ToolStripMenuItem();
            datosBaseToolStripMenuItem = new ToolStripMenuItem();
            listaDePrecioToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { administrarToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(982, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // administrarToolStripMenuItem
            // 
            administrarToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { productosToolStripMenuItem, toolStripMenuItem1, proveedoresToolStripMenuItem });
            administrarToolStripMenuItem.Name = "administrarToolStripMenuItem";
            administrarToolStripMenuItem.Size = new Size(100, 24);
            administrarToolStripMenuItem.Text = "Administrar";
            // 
            // proveedoresToolStripMenuItem
            // 
            proveedoresToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { datosBaseToolStripMenuItem, listaDePrecioToolStripMenuItem });
            proveedoresToolStripMenuItem.Name = "proveedoresToolStripMenuItem";
            proveedoresToolStripMenuItem.Size = new Size(174, 26);
            proveedoresToolStripMenuItem.Text = "Proveedores";
            // 
            // productosToolStripMenuItem
            // 
            productosToolStripMenuItem.Name = "productosToolStripMenuItem";
            productosToolStripMenuItem.Size = new Size(174, 26);
            productosToolStripMenuItem.Text = "Productos";
            // 
            // datosBaseToolStripMenuItem
            // 
            datosBaseToolStripMenuItem.Name = "datosBaseToolStripMenuItem";
            datosBaseToolStripMenuItem.Size = new Size(224, 26);
            datosBaseToolStripMenuItem.Text = "Datos Base";
            // 
            // listaDePrecioToolStripMenuItem
            // 
            listaDePrecioToolStripMenuItem.Name = "listaDePrecioToolStripMenuItem";
            listaDePrecioToolStripMenuItem.Size = new Size(224, 26);
            listaDePrecioToolStripMenuItem.Text = "Lista de Precio";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(171, 6);
            // 
            // Central
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(982, 505);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Central";
            Text = "Central";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem administrarToolStripMenuItem;
        private ToolStripMenuItem productosToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem proveedoresToolStripMenuItem;
        private ToolStripMenuItem datosBaseToolStripMenuItem;
        private ToolStripMenuItem listaDePrecioToolStripMenuItem;
    }
}