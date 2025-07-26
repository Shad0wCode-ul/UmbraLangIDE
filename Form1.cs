
using System;
using System.IO;
using System.Windows.Forms;

namespace SCIDE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCompile_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "main.scsl");
            File.WriteAllText(path, txtEditor.Text);
            Compiler.CompileScript(txtEditor.Text, path);
        }

        private void btnNewProject_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string targetPath = Path.Combine(fbd.SelectedPath, "main.scsl");
                    File.WriteAllText(targetPath, "");
                    foreach (string file in Directory.GetFiles("Compiler"))
                    {
                        File.Copy(file, Path.Combine(fbd.SelectedPath, Path.GetFileName(file)), true);
                    }
                }
            }
        }
    }
}
