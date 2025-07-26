
namespace SCIDE
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.RichTextBox txtEditor;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnNewProject;

        private void InitializeComponent()
        {
            this.txtEditor = new System.Windows.Forms.RichTextBox();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnNewProject = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.txtEditor.Location = new System.Drawing.Point(12, 12);
            this.txtEditor.Size = new System.Drawing.Size(760, 400);
            this.txtEditor.Name = "txtEditor";

            this.btnCompile.Text = "Kompilieren";
            this.btnCompile.Location = new System.Drawing.Point(12, 420);
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);

            this.btnNewProject.Text = "Neues Projekt";
            this.btnNewProject.Location = new System.Drawing.Point(120, 420);
            this.btnNewProject.Click += new System.EventHandler(this.btnNewProject_Click);

            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.txtEditor);
            this.Controls.Add(this.btnCompile);
            this.Controls.Add(this.btnNewProject);
            this.Text = "SCSL IDE";
            this.ResumeLayout(false);
        }
    }
}
