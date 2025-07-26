using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;

public class InputDialog : Form
{
    private TextBox textBoxInput;
    private Button buttonOk;
    private Label labelPrompt;

    public string InputValue => textBoxInput.Text;

    public InputDialog(string prompt)
    {
        Width = 400;
        Height = 150;
        Text = "Eingabe erforderlich";

        labelPrompt = new Label { Left = 10, Top = 10, Width = 360, Text = prompt };
        textBoxInput = new TextBox { Left = 10, Top = 40, Width = 360 };
        buttonOk = new Button { Text = "OK", Left = 290, Top = 70, Width = 80 };

        buttonOk.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

        Controls.Add(labelPrompt);
        Controls.Add(textBoxInput);
        Controls.Add(buttonOk);

        AcceptButton = buttonOk;
        StartPosition = FormStartPosition.CenterParent;
    }
}
