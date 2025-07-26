using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ScintillaNET;
using System.Windows;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text;
using System.Timers;
using System.Windows.Input; // WPF's Keyboard-Klasse (nicht WinForms!)

namespace SCIDE
{
    /*public class AutocompletePopup : Form
    {
        private ListBox listBox;
        public event Action<string> ItemSelected;

        public AutocompletePopup(Scintilla parent)
        {
            TopMost = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowInTaskbar = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.FromArgb(45, 45, 48);

            listBox = new ListBox
            {
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.None,
                BackColor = BackColor,
                ForeColor = Color.White,
                ItemHeight = 20
            };
            listBox.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    ItemSelected?.Invoke(listBox.SelectedItem.ToString());
                    Hide();
                }
            };
            listBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && listBox.SelectedItem != null)
                {
                    ItemSelected?.Invoke(listBox.SelectedItem.ToString());
                    Hide();
                }
            };

            Controls.Add(listBox);
            listBox.Dock = DockStyle.Fill;

            parent.KeyDown += (s, e) =>
            {
                if (Visible)
                {
                    if (e.KeyCode == Keys.Down) listBox.SelectedIndex = Math.Min(listBox.SelectedIndex + 1, listBox.Items.Count - 1);
                    else if (e.KeyCode == Keys.Up) listBox.SelectedIndex = Math.Max(listBox.SelectedIndex - 1, 0);
                    else if (e.KeyCode == Keys.Escape) Hide();
                }
            };
        }

        public void ShowAutocomplete(System.Collections.Generic.List<string> items, int x, int y)
        {
            listBox.Items.Clear();
            listBox.Items.AddRange(items.ToArray());
            listBox.SelectedIndex = 0;

            Width = 250;
            Height = Math.Min(items.Count * listBox.ItemHeight + 4, 200);
            Location = new Point(x + 10, y + 25); // fine-tuned offset

            Show();
            BringToFront();
            listBox.Focus();
        }
    }*/
    public partial class UmbraLangIDE : Form
    {
        private Scintilla shadowEditor;
        public static RichTextBox consoleOutput;
        private System.Windows.Forms.TextBox inputBox;
        private string stealthPath = "";

        private string currentScriptPath = "";
        private string waitingForInputName = null;
        private Queue<string> scriptQueue = new();
        private string variablesPath = "";

        private List<string> scriptLines;
        private int currentLine = 0;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.TextBox outputBox;
        private System.Windows.Forms.Button startButton;
        private GameWindow gameWindow;
        private static int playerX = 100;
        private static int playerY = 100;
        private int playerXNormal = 100;
        private int playerYNormal = 100;

        public static List<int> xs = new List<int> { playerX };
        public static List<int> ys = new List<int> { playerY };
        public static List<int> sxs = new List<int> { 64 };
        public static List<int> sys = new List<int> { 64 };

        private void InterpretCommand(string cmd)
        {
            outputBox.AppendText("> " + cmd + Environment.NewLine);

            if (cmd.Equals("MOVE LEFT", StringComparison.OrdinalIgnoreCase))
            {
                playerX -= 10;
            }
            else if (cmd.Equals("MOVE RIGHT", StringComparison.OrdinalIgnoreCase))
            {
                playerX += 10;
            }

            gameWindow.SetPlayerX(playerX);
        }

        string[] AllCode = new[]
            {
        "repeat", "white", "lwhite", "black", "storeInput", "shadow",
        "waitShadow", "code", "clearWhite", "import", "readFile", "writeFile",
        "delete", "exists", "mkdir", "now", "timestamp", "random", "getPath",
        "listDir", "if", "else", "while", "usingCode", "setupUmbra", "maxable",
        "addLabel", "addButton", "UmbraWindowLib", "addInput", "getTextOfInput",
        "changeTextOfLabel", "changeColorOfLabel", "quit", "isOpenndWindow",
        "setDebuging", "isDebuging", "isConnected", "getWindowSize", "setIcon",
        "getInputCount", "gteLabelCount", "addImage", "setBackgroundColor",
        "setBackgroundImage", "playAudio", "startGame", "UmbraGameLib", "getPlayerPos",
        "replacePlayerX", "replacePlayerY", "movePlayer", "quit", "Instantiate", "ifKeyPressed",
        "getKeyInput", "waitKeyPressed", "whileKeyPressed", "checkCollisionOfSomething"
    };

        private List<string> availableCommands = new List<string>
{
    "black", "storeInput", "repeat", "white", "lwhite", "code", "clearWhite",
    "if", "else", "waitShadow", "shadow", "import", "writeFile", "readFile",
    "exists", "delete", "now", "timestamp", "getPath", "listDir", "mkdir"
};


        /*private void SetupInputBox()
        {
            inputTextBox = new TextBox();
            inputTextBox.Dock = DockStyle.Bottom;
            inputTextBox.Font = new Font("Fira Code", 10);
            inputTextBox.Visible = false;
            inputTextBox.KeyDown += InputTextBox_KeyDown;
            Controls.Add(inputTextBox);
        }*/

        /*private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && waitingForInputName != null)
            {
                string input = inputTextBox.Text.Trim();
                inputTextBox.Visible = false;
                inputTextBox.Text = "";

                string variablesFile = Path.Combine(Path.GetDirectoryName(stealthPath), "Variables", "variables.var");
                SaveVariable(waitingForInputName, input, variablesFile);
                waitingForInputName = null;

                WriteConsole($"> {input}");
                ContinueScriptExecution(); // Skript fortsetzen
                e.SuppressKeyPress = true;
            }
        }*/

        private ContextMenuStrip shadowEditorContextMenu;

        public UmbraLangIDE()
        {
            InitializeEditor();
            InitializeUI();
            InitializeInputUI();
            StyleEditorHackerTheme();
            SetupEditorHackerStyle();

            this.KeyPreview = true;

            shadowEditor.PreviewKeyDown += ShadowEditor_PreviewKeyDown;
            shadowEditor.KeyPress += ShadowEditor_KeyPress;
            shadowEditor.KeyDown += Form1_KeyDown;

            shadowEditor.AutoCSeparator = ' ';
            shadowEditor.AutoCIgnoreCase = true;
            shadowEditor.AutoCMaxHeight = 10;

            shadowEditor.CharAdded += Editor_CharAdded;
            shadowEditor.CharAdded += shadowEditor_CharAdded;
            shadowEditor.KeyDown += shadowEditor_KeyDown;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 50;
            timer.Tick += (s, e) => CheckKeyActions();
            timer.Start();

            /*this.Text = "Umbra Game Engine Preview";
            this.Size = new Size(600, 300);

            outputBox = new System.Windows.Forms.TextBox
            {
                Location = new Point(20, 20),
                Size = new Size(540, 180),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };
            this.Controls.Add(outputBox);

            startButton = new System.Windows.Forms.Button
            {
                Text = "Start Preview",
                Location = new Point(20, 210),
                Size = new Size(120, 30)
            };
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            timer.Tick += Timer_Tick;

            // Neues Spiel-Fenster erstellen und anzeigen
            gameWindow = new GameWindow();
            gameWindow.StartPosition = FormStartPosition.Manual;
            gameWindow.Location = new Point(this.Right + 10, this.Top); // rechts neben Hauptfenster
            gameWindow.Show();
            
            // Script laden
            var dialog = new OpenFileDialog { Filter = "UmbraLang Files (*.umbras)|*.umbras|All Files (*.*)|*.*" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadScript(dialog.FileName);
            }*/

            /*popup.ItemSelected += item =>
            {
                int pos = shadowEditor.CurrentPosition;
                int start = shadowEditor.WordStartPosition(pos, true);
                shadowEditor.TargetStart = start;
                shadowEditor.TargetEnd = pos;
                shadowEditor.ReplaceTarget(item);
            };*/
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // STRG + S
            if (e.Control && e.KeyCode == Keys.S && !e.Shift)
            {
                SaveFile();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // STRG + UMSCHALT + S
            else if (e.Control && e.Shift && e.KeyCode == Keys.S)
            {
                SaveFileAs();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // STRG + O
            else if (e.Control && e.KeyCode == Keys.O)
            {
                OpenFile();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // STRG + N
            else if (e.Control && e.KeyCode == Keys.N)
            {
                SaveFileAs();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // F5
            else if (e.KeyCode == Keys.F5)
            {
                ExecuteCode();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void ShadowEditor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // Damit KeyPress überhaupt feuert
            if (e.Control && (e.KeyCode == Keys.S || e.KeyCode == Keys.O || e.KeyCode == Keys.N))
            {
                e.IsInputKey = true;
            }
        }

        private void ShadowEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Steuerzeichen blockieren: DC1–DC4, SI, SO etc.
            if ((int)e.KeyChar == 19 ||  // Ctrl+S → DC3
                (int)e.KeyChar == 15 ||  // Ctrl+O → SI
                (int)e.KeyChar == 14)    // Ctrl+N → SO
            {
                e.Handled = true; // Unterdrücken, NICHT einfügen
            }
        }

        private void InitializeEditor()
        {
            this.Text = "UmbraLang IDE";
            this.Width = 850;
            this.Height = 850;
            this.Icon = new Icon("icon.ico"); // Stelle sicher, dass du eine passende Icon-Datei hast

            shadowEditor = new Scintilla
            {
                Dock = DockStyle.Fill,
                WrapMode = WrapMode.None,
                Lexer = Lexer.Container,
            };

            shadowEditor.StyleResetDefault();
            shadowEditor.Styles[Style.Default].Font = "Consolas";
            shadowEditor.Styles[Style.Default].Size = 11;
            shadowEditor.StyleClearAll();

            // Farbstile definieren
            shadowEditor.Styles[0].ForeColor = Color.Black;       // Default
            shadowEditor.Styles[1].ForeColor = Color.Blue;        // Keywords
            shadowEditor.Styles[2].ForeColor = Color.Green;       // Strings
            shadowEditor.Styles[3].ForeColor = Color.OrangeRed;   // Numbers
            shadowEditor.Styles[4].ForeColor = Color.Yellow;   // Numbers

            shadowEditor.AutoCSeparator = ' ';
            shadowEditor.AutoCIgnoreCase = true;
            shadowEditor.AutoCMaxHeight = 8;

            shadowEditor.CharAdded += Editor_CharAdded;

            Controls.Add(shadowEditor);
        }

        private void Editor_CharAdded(object sender, CharAddedEventArgs e)
        {
            char c = (char)e.Char;

            if (!char.IsLetterOrDigit(c) && c != '(')
                return;

            int currentPos = shadowEditor.CurrentPosition;
            int wordStartPos = shadowEditor.WordStartPosition(currentPos, true);
            string currentWord = shadowEditor.GetTextRange(wordStartPos, currentPos - wordStartPos);

            if (string.IsNullOrWhiteSpace(currentWord) || currentWord.Length < 1)
                return;

            var suggestions = AllCode
                .Where(s => s.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (suggestions.Any())
            {
                string suggestionList = string.Join(" ", suggestions);
                shadowEditor.AutoCShow(currentWord.Length, suggestionList);
            }
        }

        private readonly Dictionary<char, char> autoPairs = new()
{
    { '"', '"' },
    { '\'', '\'' },
    { '(', ')' },
    { '[', ']' },
    { '{', '}' },
    { '<', '>' }
};

        private void shadowEditor_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            var sci = shadowEditor;
            char typedChar = (char)e.Char;

            if (autoPairs.TryGetValue(typedChar, out char closingChar))
            {
                int pos = sci.CurrentPosition;

                // Wenn schon das nächste Zeichen das schließende ist, nichts einfügen
                if ((pos < sci.TextLength && (char)sci.GetCharAt(pos) == closingChar))
                    return;

                sci.InsertText(pos, closingChar.ToString());
                sci.GotoPosition(pos);
            }
        }

        private void shadowEditor_KeyDown(object sender, KeyEventArgs e)
        {
            var sci = shadowEditor;
            int pos = sci.CurrentPosition;

            if (pos <= 0 || pos >= sci.TextLength)
                return;

            // Backspace: Prüfe Zeichen links und rechts
            if (e.KeyCode == Keys.Back)
            {
                char left = (char)sci.GetCharAt(pos - 1);
                char right = (char)sci.GetCharAt(pos);

                if (autoPairs.TryGetValue(left, out char expectedRight) && right == expectedRight)
                {
                    sci.DeleteRange(pos - 1, 2);
                    e.Handled = true;
                }
            }

            // Delete: Prüfe Zeichen unter Cursor und danach
            if (e.KeyCode == Keys.Delete)
            {
                char left = (char)sci.GetCharAt(pos);
                char right = (char)sci.GetCharAt(pos + 1);

                if (autoPairs.TryGetValue(left, out char expectedRight) && right == expectedRight)
                {
                    sci.DeleteRange(pos, 2);
                    e.Handled = true;
                }
            }
        }

        private void InitializeUI()
        {
            var menu = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var buildMenu = new ToolStripMenuItem("Build");

            var openItem = new ToolStripMenuItem("Open", null, (s, e) => OpenFile());
            var saveItem = new ToolStripMenuItem("Save", null, (s, e) => SaveFile());
            var saveAsItem = new ToolStripMenuItem("Save As", null, (s, e) => SaveFileAs());
            var newItem = new ToolStripMenuItem("New Script", null, (s, e) => SaveFile());
            var compileItem = new ToolStripMenuItem("Compile", null, (s, e) => ExecuteCode());

            fileMenu.DropDownItems.Add(openItem);
            fileMenu.DropDownItems.Add(saveItem);
            fileMenu.DropDownItems.Add(saveAsItem);
            fileMenu.DropDownItems.Add(newItem);
            buildMenu.DropDownItems.Add(compileItem);

            menu.Items.Add(fileMenu);
            menu.Items.Add(buildMenu);

            Controls.Add(menu);
            MainMenuStrip = menu;

            consoleOutput = new RichTextBox
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 11)
            };
            Controls.Add(consoleOutput);
        }

        private void InitializeInputUI()
        {
            inputBox = new System.Windows.Forms.TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Visible = false
            };

            /*submitInputButton = new Button
            {
                Text = "Eingabe senden",
                Dock = DockStyle.Bottom,
                Height = 30,
                Visible = false
            };

            submitInputButton.Click += SubmitInputButton_Click;

            Controls.Add(submitInputButton);*/
            Controls.Add(inputBox);
        }

        /*private void SubmitInputButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(waitingForInputName))
            {
                string input = inputBox.Text.Trim();

                SaveVariable(waitingForInputName, input, variablesPath);
                WriteConsole($"→ {waitingForInputName} = {input}");

                inputBox.Visible = false;
                submitInputButton.Visible = false;
                inputBox.Text = "";

                waitingForInputName = null;
                ContinueScriptExecution();
            }
        }*/

        private void SetupEditorHackerStyle()
        {
            shadowEditor.BackColor = Color.Black;
            shadowEditor.ForeColor = Color.Lime;
            shadowEditor.Font = new Font("Consolas", 11, FontStyle.Regular);

            shadowEditor.CaretForeColor = Color.Lime;
            shadowEditor.CaretLineVisible = false;
            shadowEditor.CaretWidth = 1;

            shadowEditor.ReadOnly = false;
            shadowEditor.Focus();
        }

        private void HighlightSyntax()
        {
            string text = shadowEditor.Text;
            shadowEditor.StartStyling(0);
            shadowEditor.SetStyling(text.Length, 0); // Reset styles to default first

            // Keywords in UmbraLang
            string[] keywords = new[]
            {
        "repeat", "white", "lwhite", "black", "storeInput", "shadow",
        "waitShadow", "code", "clearWhite", "import", "readFile", "writeFile",
        "delete", "exists", "mkdir", "now", "timestamp", "random", "getPath",
        "listDir", "if", "else", "while", "usingCode", "setupUmbra", "maxable",
        "addLabel", "addButton", "UmbraWindowLib", "addInput", "getTextOfInput",
        "changeTextOfLabel", "changeColorOfLabel", "quit", "isOpenndWindow",
        "setDebuging", "isDebuging", "isConnected", "getWindowSize", "setIcon",
        "getInputCount", "gteLabelCount", "addImage", "setBackgroundColor",
        "setBackgroundImage", "playAudio", "startGame", "UmbraGameLib", "getPlayerPos",
        "replacePlayerX", "replacePlayerY", "movePlayer", "quit", "Instantiate", "ifKeyPressed",
        "getKeyInput"
    };
            string[] bools = new[]
            {
                "true", "false"
            };
            // Keywords hervorheben
            foreach (string word in keywords)
            {
                int index = 0;
                while ((index = text.IndexOf(word, index, StringComparison.Ordinal)) != -1)
                {
                    bool isStart = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
                    bool isEnd = index + word.Length == text.Length || !char.IsLetterOrDigit(text[index + word.Length]);
                    if (isStart && isEnd)
                    {
                        shadowEditor.StartStyling(index);
                        shadowEditor.SetStyling(word.Length, 1); // Style 1: Keyword (blau)
                    }
                    index += word.Length;
                }
            }

            foreach (string word in bools)
            {
                int index = 0;
                while ((index = text.IndexOf(word, index, StringComparison.Ordinal)) != -1)
                {
                    bool isStart = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
                    bool isEnd = index + word.Length == text.Length || !char.IsLetterOrDigit(text[index + word.Length]);
                    if (isStart && isEnd)
                    {
                        shadowEditor.StartStyling(index);
                        shadowEditor.SetStyling(word.Length, 4);
                    }
                    index += word.Length;
                }
            }

            // Strings hervorheben
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '"')
                {
                    int start = i;
                    i++;
                    while (i < text.Length && text[i] != '"') i++;
                    if (i < text.Length)
                    {
                        int len = i - start + 1;
                        shadowEditor.StartStyling(start);
                        shadowEditor.SetStyling(len, 2); // Style 2: Strings (grün)
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }

            // Zahlen hervorheben
            for (int j = 0; j < text.Length; j++)
            {
                if (char.IsDigit(text[j]))
                {
                    int start = j;
                    while (j < text.Length && char.IsDigit(text[j])) j++;
                    int length = j - start;
                    shadowEditor.StartStyling(start);
                    shadowEditor.SetStyling(length, 3); // Style 3: Numbers (rot)
                    j--;
                }
            }
        }

        private void StyleEditorHackerTheme()
        {
            shadowEditor.Styles[Style.Default].BackColor = Color.Black;
            shadowEditor.Styles[Style.Default].ForeColor = Color.LimeGreen;
            shadowEditor.Styles[Style.Default].Font = "Consolas"; // Oder "Courier New", "Hack", "Fira Code"
            shadowEditor.Styles[Style.Default].Size = 11;

            shadowEditor.StyleClearAll(); // Wendet Default auf alle an

            // Hacker Syntaxfarben
            shadowEditor.Styles[Style.Cpp.String].ForeColor = Color.LimeGreen;
            shadowEditor.Styles[Style.Cpp.Number].ForeColor = Color.Red;
            shadowEditor.Styles[Style.Cpp.Word].ForeColor = Color.DeepSkyBlue;
            shadowEditor.Styles[Style.Cpp.Identifier].ForeColor = Color.LimeGreen;

            shadowEditor.Styles[Style.Cpp.Operator].ForeColor = Color.White;
            shadowEditor.Styles[Style.Cpp.Comment].ForeColor = Color.Gray;
            shadowEditor.Styles[Style.Cpp.CommentLine].ForeColor = Color.Gray;

            shadowEditor.Lexer = Lexer.Cpp; // Oder Lua/Python falls du was anderes nutzt
            shadowEditor.SetKeywords(0, "black white storeInput repeat break if else function return");

            // Optional: Liniennummern im Hacker-Stil
            shadowEditor.Margins[0].Width = 35;
            shadowEditor.Styles[Style.LineNumber].BackColor = Color.Black;
            shadowEditor.Styles[Style.LineNumber].ForeColor = Color.DarkGreen;
        }

        private void OpenFile()
        {
            var dialog = new OpenFileDialog { Filter = "UmbraLang Files (*.umbras)|*.umbras|All Files (*.*)|*.*" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                shadowEditor.Text = File.ReadAllText(dialog.FileName);
                currentScriptPath = dialog.FileName;
            }
        }

        private void SaveFile()
        {
            if (!string.IsNullOrWhiteSpace(currentScriptPath))
            {
                // Wenn ein gültiger Pfad existiert → direkt speichern
                File.WriteAllText(currentScriptPath, shadowEditor.Text);
            }
            else
            {
                // Falls kein Pfad existiert → Dialog anzeigen
                var dialog = new SaveFileDialog
                {
                    Filter = "UmbraLang Files (*.umbras)|*.umbras|All Files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, shadowEditor.Text);
                    currentScriptPath = dialog.FileName;
                }
            }
        }

        private void SaveFileAs()
        {
            var dialog = new SaveFileDialog { Filter = "UmbraLang Files (*.umbras)|*.umbras|All Files (*.*)|*.*" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, shadowEditor.Text);
                currentScriptPath = dialog.FileName;
            }
        }

        Dictionary<string, List<string>> functions = new Dictionary<string, List<string>>();
        string currentFunctionName;
        //bool inFunction;
        private List<string> currentFunctionLines = new List<string>();

        private bool isWindowLibaryConnected = false; // Flag für UmbraGameLib-Verbindung
        private bool isGameLibaryConnected = false; // Flag für UmbraGameLib-Verbindung
        private bool isOpendWindow = false; // Flag für UmbraGameLib-Verbindung
        private bool isOpendGame = false; // Flag für UmbraGameLib-Verbindung

        private void ExecuteCode()
        {
            currentFunctionName = null;
            currentFunctionLines = new List<string>();  // <== Wichtig!
            functions.Clear();
            waitingForInputName = null;
            consoleOutput.Clear();
            isOpendWindow = false;
            isWindowLibaryConnected = false;
            if (window != null)
            {
                window.Close();
                window = null;
            }
            allInput.Clear();
            allLabels.Clear();
            if (gameWindow != null)
            {
                playerX = playerXNormal;
                playerY = playerYNormal;
                gameWindow.SetPlayerX(playerX);
                gameWindow.SetPlayerY(playerY);
                gameWindow.Close();
                gameWindow = null;
            }

            consoleOutput.Clear();
            if (string.IsNullOrWhiteSpace(currentScriptPath))
            {
                MessageBox.Show("Bitte zuerst speichern, damit ein Pfad existiert.");
                return;
            }
            CompileScript(shadowEditor.Text, currentScriptPath);
        }

        private void CompileScript(string content, string path)
        {
            string[] lines = content.Split('\n');
            string baseDir = Path.GetDirectoryName(path);
            variablesPath = Path.Combine(baseDir, "Variables", "variables.var");
            Directory.CreateDirectory(Path.GetDirectoryName(variablesPath));

            if (!File.Exists(variablesPath)) File.CreateText(variablesPath).Close();

            scriptQueue.Clear();
            foreach (string line in lines)
                scriptQueue.Enqueue(line);

            stealthPath = path;
            ContinueScriptExecution(content, path);
        }

        private Form window;
        private List<System.Windows.Forms.TextBox> allInput = new List<System.Windows.Forms.TextBox>();
        private List<System.Windows.Forms.Label> allLabels = new List<Label>();
        bool isDebuging = false;
        bool closabel = true;
        private List<KeyAction> keyActions = new List<KeyAction>();
        //private System.Windows.Forms.Label[] allButtons;
        private void ContinueScriptExecution(string content, string path)
        {
            try
            {
                string[] lines = content.Split('\n');
                string baseDir = Path.GetDirectoryName(path);
                string variablesFolder = Path.Combine(baseDir, "Variables");
                Directory.CreateDirectory(variablesFolder);

                string variablesPath = Path.Combine(variablesFolder, "variables.var");
                string localVariablesPath = Path.Combine(variablesFolder, "localVariables.localvar");
                if (!File.Exists(variablesPath))
                {
                    File.CreateText(variablesPath).Close();
                }
                string functionsFolder = Path.Combine(variablesFolder, "Functions");
                Directory.CreateDirectory(functionsFolder);
                File.SetAttributes(functionsFolder, FileAttributes.Directory | FileAttributes.Hidden);

                functions = new();
                currentFunctionName = null;
                currentFunctionLines = null;

                for (int i = 0; i < lines.Length; i++)
                {
                    Random rnd = new Random();

                    string line = scriptQueue.Dequeue();

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    {
                        // Leerzeile oder Kommentar überspringen
                        continue;
                    }

                    // Checken falls die GameLibary verbunden wird //
                    if (line.StartsWith("usingCode UmbraWindowLib"))
                    {
                        isWindowLibaryConnected = true;
                    }
                    if (line.StartsWith("usingCode UmbraGameLib"))
                    {
                        isGameLibaryConnected = true;
                    }

                    if (waitingForInputName != null) return;

                    #region GameCodeLib
                    if (isGameLibaryConnected)
                    {
                        if (line.StartsWith("startGame("))
                        {
                            string imagePath = "";
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);

                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');

                                if (args.Length == 1)
                                {
                                    imagePath = args[0].Trim().Trim('"');

                                    if (!File.Exists(imagePath))
                                    {
                                        WriteConsole($"❌ Bildpfad nicht gefunden: {imagePath}");
                                        return;
                                    }

                                    if (gameWindow != null)
                                    {
                                        gameWindow.Close();
                                        gameWindow = null;
                                    }

                                    isOpendGame = true;
                                    isOpendWindow = true;
                                }
                                else
                                {
                                    WriteConsole("❌ Ungültige Anzahl an Argumenten für startGame(). Erwartet 1 Pfad.");
                                    return;
                                }
                            }
                            else
                            {
                                WriteConsole("❌ Fehler beim Parsen von startGame(). Klammern nicht korrekt.");
                                return;
                            }

                            try
                            {
                                gameWindow = new GameWindow(imagePath);
                                gameWindow.StartPosition = FormStartPosition.Manual;
                                gameWindow.Location = new Point(this.Right + 10, this.Top);
                                //timer.Tick += (s, e) => UpdatePressedKeysFromPhysicalState();
                                timer.Tick += (s, e) =>
                                {
                                    gameWindow.ApplyGravity();
                                    gameWindow.CheckAndTeleportPlayer();
                                    Invalidate();
                                };
                                gameWindow.Show();
                            }
                            catch (Exception ex)
                            {
                                WriteConsole("❌ Fehler beim Erstellen des GameWindows: " + ex.Message + "\n" + ex.StackTrace);
                            }
                        }
                        else if (line.StartsWith("getPlayerPos()"))
                        {
                            WriteConsole(gameWindow.GetPlayerX().ToString() + " " + gameWindow.GetPlayerY().ToString());
                        }
                        else if (line.Contains("replacePlayerX()"))
                        {
                            line = line.Replace("replacePlayerX()", gameWindow.GetPlayerX().ToString());
                            //WriteConsole(gameWindow.GetPlayerX().ToString() + " " + gameWindow.GetPlayerY().ToString());
                        }   
                        else if (line.Contains("replacePlayerY()"))
                        {
                            line = line.Replace("replacePlayerY()", gameWindow.GetPlayerY().ToString());
                            //WriteConsole(gameWindow.GetPlayerX().ToString() + " " + gameWindow.GetPlayerY().ToString());
                        }
                        else if (line.StartsWith("movePlayer("))
                        {
                            // move player in direction
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length == 2)
                                {
                                    string direction = args[0].Trim().ToLower();
                                    if (gameWindow == null)
                                    {
                                        WriteConsole("❌ Game window not initialized.");
                                        return;
                                    }
                                    int speed = int.Parse(args[1]);
                                    //int.TryParse(args[1], out speed);
                                    if (direction == "left")
                                    {
                                        playerX -= speed;
                                    }
                                    else if (direction == "right")
                                    {
                                        playerX += speed;
                                    }
                                    else if (direction == "up")
                                    {
                                        playerY -= speed;
                                    }
                                    else if (direction == "down")
                                    {
                                        playerY += speed;
                                    }
                                    else
                                    {
                                        WriteConsole("❌ Ungültige Richtung: " + direction);
                                        continue;
                                    }
                                    gameWindow.SetPlayerY(playerY);
                                    gameWindow.SetPlayerX(playerX);
                                    xs[0] = playerX;
                                    ys[0] = playerY;
                                    if (isDebuging)
                                        WriteConsole($"Player moved {direction}");
                                }
                            }
                        }
                        else if (line.StartsWith("quit()"))
                        {
                            gameWindow.Close();
                            gameWindow = null;
                        }
                        else if (line.StartsWith("Instantiate("))
                        {
                            // Instaniate a object like so Instantiate("File\Path\To\PNG", x, y)
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(")");
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length == 5)
                                {
                                    string imagePath = args[0].Trim().Trim('"');
                                    int x = int.Parse(args[1]);
                                    int y = int.Parse(args[2]);
                                    int sizex = int.Parse(args[3]);
                                    int sizey = int.Parse(args[4]);
                                    sxs.Add(sizex);
                                    sys.Add(sizey);
                                    if (gameWindow == null)
                                    {
                                        WriteConsole("❌ Game window not initialized.");
                                        return;
                                    }
                                    gameWindow.Inistialize(imagePath, x, y, sizex, sizey);
                                    if (isDebuging)
                                        WriteConsole($"Instantiated object at ({x}, {y}) with image {imagePath}");
                                }
                            }
                        }
                        else if (line.StartsWith("playAudio("))
                        {
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string audioPath = inBrackets.Trim('"');
                                WriteConsole(audioPath);
                                if (File.Exists(audioPath))
                                {
                                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(audioPath);
                                    player.Play();
                                    if (isDebuging)
                                        WriteConsole($"Playing audio: {audioPath}");
                                }
                                else
                                {
                                    WriteConsole($"❌ Audio file not found: {audioPath}");
                                }
                            }
                        }
                        else if (line.StartsWith("setBackgroundColor("))
                        {
                            if (gameWindow == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string colorName = inBrackets.Trim('"');
                                Color color;
                                try
                                {
                                    color = Color.FromName(colorName);
                                    gameWindow.SetColor(color);
                                    gameWindow.BackColor = color;
                                    if (isDebuging)
                                        WriteConsole($"Background color set: {colorName}");
                                }
                                catch
                                {
                                    WriteConsole($"❌ Invalid color name: {colorName}");
                                }
                            }
                        }
                        else if (line.StartsWith("setBackgroundImage("))
                        {
                            if (gameWindow == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string imagePath = inBrackets.Trim('"');
                                if (File.Exists(imagePath))
                                {
                                    gameWindow.BackgroundImage = Image.FromFile(imagePath);
                                    gameWindow.BackgroundImageLayout = ImageLayout.Stretch;
                                    if (isDebuging)
                                        WriteConsole($"Background image set: {imagePath}");
                                }
                                else
                                {
                                    WriteConsole($"❌ Background image not found: {imagePath}");
                                }
                            }
                        }
                        else if (line.StartsWith("setIcon()"))
                        {
                            if (gameWindow == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            string iconPath = Path.Combine(Path.GetDirectoryName(path), "icon.ico");
                            if (File.Exists(iconPath))
                            {
                                gameWindow.Icon = new Icon(iconPath);
                                if (isDebuging)
                                    WriteConsole("Icon set to icon.ico");
                            }
                            else
                            {
                                WriteConsole("❌ icon.ico not found in script directory.");
                            }
                        }
                        /*else if (line.StartsWith("ifKeyPressed("))
                        {
                            // Wir sammeln alle Zeilen bis zur schließenden Klammer
                            string fullLine = line;
                            int blockDepth = line.Count(c => c == '{') - line.Count(c => c == '}');

                            while (blockDepth > 0 && scriptQueue.Count > 0)
                            {
                                string nextLine = scriptQueue.Dequeue();
                                fullLine += "\n" + nextLine;
                                blockDepth += nextLine.Count(c => c == '{') - nextLine.Count(c => c == '}');
                            }

                            // Jetzt ist fullLine die vollständige Anweisung (auch über mehrere Zeilen)

                            int openParen = fullLine.IndexOf('(');
                            int firstBrace = fullLine.IndexOf('{');
                            int lastBrace = fullLine.LastIndexOf('}');

                            string argsPart = line.Substring(openParen + 1, firstBrace - openParen - 1).Trim().TrimEnd(',');
                            string[] args = argsPart.Split(',').Select(a => a.Trim()).ToArray(); ;

                            if (args.Length >= 2)
                            {
                                string keyName = args[0].Trim().Trim('"');
                                string codeToExecute = fullLine.Substring(firstBrace + 1, lastBrace - firstBrace - 1).Trim();

                                if (gameWindow == null)
                                {
                                    WriteConsole("Game window not initialized.");
                                    return;
                                }

                                WriteConsole($"Key gesucht: {keyName}, gedrückt: {gameWindow.LastKeyPressed}");

                                if (Enum.TryParse(keyName, true, out Keys targetKey))
                                {
                                    if (gameWindow.LastKeyPressed == targetKey)
                                    {
                                        try
                                        {
                                            WriteConsole($"{codeToExecute}");
                                            ContinueScriptExecution(codeToExecute, path);
                                            //if (isDebuging)
                                                WriteConsole($"Executed code for key: {keyName}");
                                        }
                                        catch (Exception ex)
                                        {
                                            WriteConsole($"Fehler beim Ausführen von Code für {keyName}: {ex.Message}");
                                        }

                                        gameWindow.LastKeyPressed = Keys.None; // Reset
                                    }
                                }
                                else
                                {
                                    WriteConsole($"Ungültiger Tastename: {keyName}");
                                }
                            }
                        }*/
                        else if (line.StartsWith("ifKeyPressed("))
                        {
                            string fullLine = line;
                            int blockDepth = line.Count(c => c == '{') - line.Count(c => c == '}');

                            while (blockDepth > 0 && scriptQueue.Count > 0)
                            {
                                string nextLine = scriptQueue.Dequeue();
                                fullLine += "\n" + nextLine;
                                blockDepth += nextLine.Count(c => c == '{') - nextLine.Count(c => c == '}');
                            }

                            int openParen = fullLine.IndexOf('(');
                            int firstBrace = fullLine.IndexOf('{');
                            int lastBrace = fullLine.LastIndexOf('}');

                            string argsPart = fullLine.Substring(openParen + 1, firstBrace - openParen - 1).Trim().TrimEnd(',');
                            string[] args = argsPart.Split(',').Select(a => a.Trim()).ToArray();

                            if (args.Length >= 1)
                            {
                                string keyName = args[0].Trim('"');
                                string codeBlock = fullLine.Substring(firstBrace + 1, lastBrace - firstBrace - 1).Trim();

                                if (Enum.TryParse(keyName, true, out Keys targetKey))
                                {
                                    if (CurrentlyPressedKeys.Contains(targetKey))
                                    {
                                        CompileScript(codeBlock + "\n", path);
                                    }

                                    if (isDebuging)
                                        WriteConsole($"🟢 ifKeyPressed ausgeführt: {targetKey}");
                                }
                            }
                        }
                        else if (line.StartsWith("waitKeyPressed("))
                        {
                            if (gameWindow == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }

                            // Wir sammeln alle Zeilen bis zur schließenden Klammer
                            string fullLine = line;
                            int blockDepth = line.Count(c => c == '{') - line.Count(c => c == '}');

                            while (blockDepth > 0 && scriptQueue.Count > 0)
                            {
                                string nextLine = scriptQueue.Dequeue();
                                fullLine += "\n" + nextLine;
                                blockDepth += nextLine.Count(c => c == '{') - nextLine.Count(c => c == '}');
                            }

                            // Jetzt ist fullLine die vollständige Anweisung (auch über mehrere Zeilen)

                            int openParen = fullLine.IndexOf('(');
                            int firstBrace = fullLine.IndexOf('{');
                            int lastBrace = fullLine.LastIndexOf('}');

                            string argsPart = fullLine.Substring(openParen + 1, firstBrace - openParen - 1).Trim().TrimEnd(',');
                            string[] args = argsPart.Split(',').Select(a => a.Trim()).ToArray();

                            if (args.Length >= 1)
                            {
                                string keyName = args[0].Trim('"');

                                string codeBlock = fullLine.Substring(firstBrace + 1, lastBrace - firstBrace - 1).Trim();

                                if (Enum.TryParse(keyName, true, out Keys targetKey))
                                {
                                    if (gameWindow.LastKeyPressed == targetKey)
                                    {
                                        CompileScript(codeBlock + "\n", path);
                                        //if (isDebuging)
                                        WriteConsole($"🟢 ifKeyPressed ausgeführt: {targetKey}");
                                    }
                                    keyActions.Add(new KeyAction
                                    {
                                        CodeBlock = codeBlock,
                                        TriggerKey = targetKey,
                                        IsLooping = false
                                    });
                                }

                            }
                            else
                            {
                                WriteConsole("❌ Ungültige Argumente in addButton()");
                            }
                        }
                        else if (line.StartsWith("whileKeyPressed("))
                        {
                            if (gameWindow == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }

                            // Wir sammeln alle Zeilen bis zur schließenden Klammer
                            string fullLine = line;
                            int blockDepth = line.Count(c => c == '{') - line.Count(c => c == '}');

                            while (blockDepth > 0 && scriptQueue.Count > 0)
                            {
                                string nextLine = scriptQueue.Dequeue();
                                fullLine += "\n" + nextLine;
                                blockDepth += nextLine.Count(c => c == '{') - nextLine.Count(c => c == '}');
                            }

                            // Jetzt ist fullLine die vollständige Anweisung (auch über mehrere Zeilen)

                            int openParen = fullLine.IndexOf('(');
                            int firstBrace = fullLine.IndexOf('{');
                            int lastBrace = fullLine.LastIndexOf('}');

                            string argsPart = fullLine.Substring(openParen + 1, firstBrace - openParen - 1).Trim().TrimEnd(',');
                            string[] args = argsPart.Split(',').Select(a => a.Trim()).ToArray();

                            if (args.Length >= 1)
                            {
                                string keyName = args[0].Trim('"');

                                string codeBlock = fullLine.Substring(firstBrace + 1, lastBrace - firstBrace - 1).Trim();

                                if (Enum.TryParse(keyName, true, out Keys targetKey))
                                {
                                    if (gameWindow.LastKeyPressed == targetKey)
                                    {
                                        CompileScript(codeBlock + "\n", path);
                                        //if (isDebuging)
                                        WriteConsole($"🟢 ifKeyPressed ausgeführt: {targetKey}");
                                    }
                                    keyActions.Add(new KeyAction
                                    {
                                        CodeBlock = codeBlock,
                                        TriggerKey = targetKey,
                                        IsLooping = true
                                    });
                                }

                            }
                            else
                            {
                                WriteConsole("❌ Ungültige Argumente in addButton()");
                            }
                        }
                        else if (line.StartsWith("checkCollisionOfSomething("))
                        {
                            if (gameWindow == null)
                            {
                                WriteConsole("No game Window initalized");
                                return;
                            }

                            string fullLine = line;
                            int blockDepth = line.Count(c => c == '{') - line.Count(c => c == '}');

                            while (blockDepth > 0 && scriptQueue.Count > 0)
                            {
                                string nextLine = scriptQueue.Dequeue();
                                fullLine += "\n" + nextLine;
                                blockDepth += nextLine.Count(c => c == '{') - nextLine.Count(c => c == '}');
                            }

                            // Jetzt ist fullLine die vollständige Anweisung (auch über mehrere Zeilen)

                            int openParen = fullLine.IndexOf('(');
                            int firstBrace = fullLine.IndexOf('{');
                            int lastBrace = fullLine.LastIndexOf('}');

                            string argsPart = fullLine.Substring(openParen + 1, firstBrace - openParen - 1).Trim().TrimEnd(',');
                            string[] args = argsPart.Split(',').Select(a => a.Trim()).ToArray();

                            if (args.Length >= 1)
                            {
                                string codeBlock = fullLine.Substring(firstBrace + 1, lastBrace - firstBrace - 1).Trim();

                                if (gameWindow.IfCollision())
                                {
                                    CompileScript(codeBlock + "\n", path);
                                }

                            }
                            else
                            {
                                WriteConsole("❌ Ungültige Argumente in addButton()");
                            }
                        }
                        else if (line.StartsWith("checkCollision("))
                        {
                            if (gameWindow == null)
                            {
                                WriteConsole("No game Window initalized");
                                return;
                            }

                            string fullLine = line;
                            int blockDepth = line.Count(c => c == '{') - line.Count(c => c == '}');

                            while (blockDepth > 0 && scriptQueue.Count > 0)
                            {
                                string nextLine = scriptQueue.Dequeue();
                                fullLine += "\n" + nextLine;
                                blockDepth += nextLine.Count(c => c == '{') - nextLine.Count(c => c == '}');
                            }

                            // Jetzt ist fullLine die vollständige Anweisung (auch über mehrere Zeilen)

                            int openParen = fullLine.IndexOf('(');
                            int firstBrace = fullLine.IndexOf('{');
                            int lastBrace = fullLine.LastIndexOf('}');

                            string argsPart = fullLine.Substring(openParen + 1, firstBrace - openParen - 1).Trim().TrimEnd(',');
                            string[] args = argsPart.Split(',').Select(a => a.Trim()).ToArray();

                            if (args.Length >= 2)
                            {
                                int object1Index = int.Parse(args[0]);
                                int object2Index = int.Parse(args[1]);
                                string codeBlock = fullLine.Substring(firstBrace + 1, lastBrace - firstBrace - 1).Trim();

                                if (gameWindow.IfCollision2(object1Index, object2Index))
                                {
                                    CompileScript(codeBlock + "\n", path);
                                }

                            }
                            else
                            {
                                WriteConsole("❌ Ungültige Argumente in checkCollision()");
                            }
                        }
                        else if (line.Contains("getKeyInput"))
                        {
                            // Get Key that was pressed on the keyboard
                            if (gameWindow == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            if (gameWindow.LastKeyPressed != Keys.None)
                            {
                                string key = gameWindow.LastKeyPressed.ToString();
                                line = line.Replace("getKeyInput", key);
                                WriteConsole($"Last key pressed: {key}");
                                gameWindow.LastKeyPressed = Keys.None; // Reset after reading
                            }
                            else
                            {
                                WriteConsole("No key pressed yet.");
                            }
                        }
                        else if (line.StartsWith("maxable("))
                        {
                            if (gameWindow == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }

                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length >= 1)
                                {
                                    bool.TryParse(args[0], out bool Maximaizable);

                                    gameWindow.MaximizeBox = Maximaizable;
                                }
                            }
                        }
                        else if (line.StartsWith("addGravity("))
                        {
                            int open = line.IndexOf('(');
                            int close = line.IndexOf(')', open);
                            if (open != -1 && close != -1)
                            {
                                string arg = line.Substring(open + 1, close - open - 1).Trim();
                                if (int.TryParse(arg, out int index))
                                {
                                    gameWindow.AddGravityToObject(index);
                                    if (isDebuging)
                                        WriteConsole($"🌍 Gravitation aktiviert für Objekt {index}");
                                }
                                else
                                {
                                    WriteConsole("❌ Ungültiger Index bei addGravity()");
                                }
                            }
                        }
                    }
                    #endregion

                    #region WindowCodeLib
                    if (isWindowLibaryConnected)
                    {
                        if (line.StartsWith("addLabel("))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }

                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length >= 4)
                                {
                                    string text = args[0].Trim('"');
                                    int x = int.Parse(args[1]);
                                    int y = int.Parse(args[2]);
                                    int size = int.Parse(args[3]);

                                    Label l = new Label();
                                    l.Location = new Point(x, y);
                                    l.Text = text;
                                    l.AutoSize = true;
                                    l.ForeColor = Color.Black;
                                    l.Font = new Font("Calibri", size);

                                    window.Controls.Add(l);

                                    allLabels.Add(l);

                                    if (isDebuging)
                                        WriteConsole("Added Label");
                                }
                            }
                        }
                        else if (line.StartsWith("addInput("))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }

                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length >= 4)
                                {
                                    int x = int.Parse(args[0]);
                                    int y = int.Parse(args[1]);
                                    int width = int.Parse(args[3]);
                                    int size = int.Parse(args[2]);

                                    System.Windows.Forms.TextBox inp = new System.Windows.Forms.TextBox();
                                    inp.Location = new Point(x, y);
                                    inp.Font = new Font("Calibri", size);
                                    inp.Size = new Size(width, 0);

                                    window.Controls.Add(inp);

                                    // add inp to allInput List //
                                    allInput.Add(inp);

                                    if (isDebuging)
                                        WriteConsole("Added InputField");
                                }
                            }
                        }
                        else if (line.Contains("getTextOfInput("))
                        {
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length == 1)
                                {
                                    if (allInput.Count == 0)
                                    {
                                        WriteConsole("❌ Keine Inputs vorhanden.");
                                        return;
                                    }
                                    if (!int.TryParse(args[0], out int index))
                                    {
                                        WriteConsole("❌ Ungültiger Input-Index: " + args[0]);
                                        return;
                                    }
                                    if (index < 0 || index >= allInput.Count)
                                    {
                                        WriteConsole("❌ Ungültiger Input-Index: " + index);
                                        return;
                                    }
                                    string text = GetInputByNumber(index).Text;
                                    line = line.Replace("getTextOfInput(" + inBrackets + ")", text);
                                }
                            }
                        }
                        else if (line.StartsWith("changeTextOfLabel("))
                        {
                            // make the change of text compatible with variables like this: chnageTextOfLabel(0, "Hallo " + name)
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length == 2)
                                {
                                    if (!int.TryParse(args[0], out int index))
                                    {
                                        WriteConsole("❌ Ungültiger Label-Index: " + args[0]);
                                        return;
                                    }
                                    if (index < 0 || index >= allLabels.Count)
                                    {
                                        WriteConsole("❌ Ungültiger Label-Index: " + index);
                                        return;
                                    }
                                    // Set Text Like this: "Hallo " + variableName
                                    string text = args[1].Trim(); // Entferne Anführungszeichen
                                    string[] textTiles = new string[] { };
                                    if (text.Contains("+"))
                                    {
                                        textTiles = text.Split('+');
                                    }
                                    if (textTiles.Length == 0)
                                    {
                                        if (text.StartsWith("\"") && text.EndsWith("\""))
                                        {
                                            text = text.Trim('"'); // Entferne Anführungszeichen
                                        }
                                        else
                                        {
                                            text = GetValueOfVariable(text, variablesPath); // Hole den Wert der Variable
                                        }
                                    }
                                    else
                                    {
                                        text = "";
                                        foreach (string texttt in textTiles)
                                        {
                                            string textt = texttt;
                                            textt = RemoveSpacesOutsideQuotes(textt);
                                            if (textt.StartsWith("\"") && textt.EndsWith("\""))
                                            {
                                                text += textt.Trim('"'); // Entferne Anführungszeichen
                                            }
                                            else
                                            {
                                                text += GetValueOfVariable(textt, variablesPath); // Hole den Wert der Variable
                                            }
                                        }
                                    }
                                    // Setze den Text des Labels
                                    allLabels[index].Text = text;
                                    if (isDebuging)
                                        WriteConsole($"Label {index} geändert: {text}");
                                }
                            }
                        }
                        else if (line.StartsWith("changeColorOfLabel("))
                        {
                            // Change Color of Label like this: changeColorOfLabel(0, Red)
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(")");
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length == 2)
                                {
                                    if (!int.TryParse(args[0], out int index))
                                    {
                                        WriteConsole("❌ Ungültiger Label-Index: " + args[0]);
                                        return;
                                    }
                                    if (index < 0 || index >= allLabels.Count)
                                    {
                                        WriteConsole("❌ Ungültiger Label-Index: " + index);
                                        return;
                                    }
                                    string colorName = args[1].Trim();
                                    Color color;
                                    try
                                    {
                                        color = Color.FromName(colorName);
                                    }
                                    catch
                                    {
                                        WriteConsole("❌ Ungültige Farbe: " + colorName);
                                        return;
                                    }
                                    allLabels[index].ForeColor = color;
                                    if (isDebuging)
                                        WriteConsole($"Label {index} Farbe geändert: {color}");
                                }
                            }
                        }
                        else if (line.TrimStart().StartsWith("addButton("))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }

                            // Wir sammeln alle Zeilen bis zur schließenden Klammer
                            string fullLine = line;
                            int blockDepth = line.Count(c => c == '{') - line.Count(c => c == '}');

                            while (blockDepth > 0 && scriptQueue.Count > 0)
                            {
                                string nextLine = scriptQueue.Dequeue();
                                fullLine += "\n" + nextLine;
                                blockDepth += nextLine.Count(c => c == '{') - nextLine.Count(c => c == '}');
                            }

                            // Jetzt ist fullLine die vollständige Anweisung (auch über mehrere Zeilen)

                            int openParen = fullLine.IndexOf('(');
                            int firstBrace = fullLine.IndexOf('{');
                            int lastBrace = fullLine.LastIndexOf('}');

                            string argsPart = fullLine.Substring(openParen + 1, firstBrace - openParen - 1).Trim().TrimEnd(',');
                            string[] args = argsPart.Split(',').Select(a => a.Trim()).ToArray();

                            if (args.Length >= 4)
                            {
                                string text = args[0].Trim('"');
                                int x = int.Parse(args[1]);
                                int y = int.Parse(args[2]);
                                int size = int.Parse(args[3]);

                                string codeBlock = fullLine.Substring(firstBrace + 1, lastBrace - firstBrace - 1).Trim();

                                System.Windows.Forms.Button b = new System.Windows.Forms.Button();
                                b.Location = new Point(x, y);
                                b.Text = text;
                                b.AutoSize = true;
                                b.ForeColor = Color.Black;
                                b.Font = new Font("Calibri", size);

                                b.Click += (sender, e) =>
                                {
                                    //WriteConsole("Button clicked. Running code:");
                                    //WriteConsole(codeBlock);
                                    CompileScript(codeBlock + "\n", path);
                                };

                                window.Controls.Add(b);

                                if (isDebuging)
                                    WriteConsole($"Button hinzugefügt: {text}");
                            }
                            else
                            {
                                WriteConsole("❌ Ungültige Argumente in addButton()");
                            }
                        }
                        else if (line.StartsWith("maxable("))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }

                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length >= 1)
                                {
                                    bool.TryParse(args[0], out bool Maximaizable);

                                    window.MaximizeBox = Maximaizable;
                                }
                            }
                        }
                        else if (line.StartsWith("quit()"))
                        {
                            if (window != null)
                            {
                                window.Close();
                                window = null;
                                isOpendWindow = false;
                                if (isDebuging)
                                    WriteConsole("Game window closed.");
                            }
                            else
                            {
                                WriteConsole("Game window not initialized.");
                            }
                        }
                        else if (line.StartsWith("isOpendWindow()"))
                        {
                            WriteConsole(isOpendWindow ? "Game window is open." : "Game window is closed.");
                        }
                        else if (line.StartsWith("isConnected()"))
                        {
                            WriteConsole(isWindowLibaryConnected ? "UmbraGameLib connected." : "UmbraGameLib not connected.");
                        }
                        else if (line.StartsWith("isDebuging()"))
                        {
                            WriteConsole(isDebuging ? "Debugging is enabled." : "Debugging is disabled.");
                        }
                        else if (line.StartsWith("setDebuging("))
                        {
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                bool.TryParse(inBrackets, out isDebuging);
                                if (isDebuging)
                                    WriteConsole($"Debugging set to: {isDebuging}");
                            }
                        }
                        else if (line.StartsWith("getWindowSize()"))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            WriteConsole($"Window Size: {window.Width}x{window.Height}");
                        }
                        else if (line.StartsWith("setIcon()"))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            string iconPath = Path.Combine(Path.GetDirectoryName(path), "icon.ico");
                            if (File.Exists(iconPath))
                            {
                                window.Icon = new Icon(iconPath);
                                if (isDebuging)
                                    WriteConsole("Icon set to icon.ico");
                            }
                            else
                            {
                                WriteConsole("❌ icon.ico not found in script directory.");
                            }
                        }
                        else if (line.StartsWith("getInputCount()"))
                        {
                            WriteConsole($"Input Count: {allInput.Count}");
                        }
                        else if (line.StartsWith("getLabelCount()"))
                        {
                            WriteConsole($"Label Count: {allLabels.Count}");
                        }
                        else if (line.StartsWith("addImage("))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length >= 3)
                                {
                                    string imagePath = args[0].Trim('"');
                                    int x = int.Parse(args[1]);
                                    int y = int.Parse(args[2]);
                                    if (File.Exists(imagePath))
                                    {
                                        PictureBox pictureBox = new PictureBox
                                        {
                                            Image = Image.FromFile(imagePath),
                                            Location = new Point(x, y),
                                            SizeMode = PictureBoxSizeMode.AutoSize
                                        };
                                        window.Controls.Add(pictureBox);
                                        if (isDebuging)
                                            WriteConsole($"Image added: {imagePath}");
                                    }
                                    else
                                    {
                                        WriteConsole($"❌ Image not found: {imagePath}");
                                    }
                                }
                            }
                        }
                        else if (line.StartsWith("setBackgroundColor("))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string colorName = inBrackets.Trim('"');
                                Color color;
                                try
                                {
                                    color = Color.FromName(colorName);
                                    window.BackColor = color;
                                    if (isDebuging)
                                        WriteConsole($"Background color set: {colorName}");
                                }
                                catch
                                {
                                    WriteConsole($"❌ Invalid color name: {colorName}");
                                }
                            }
                        }
                        else if (line.StartsWith("setBackgroundImage("))
                        {
                            if (window == null)
                            {
                                WriteConsole("Game window not initialized.");
                                return;
                            }
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string imagePath = inBrackets.Trim('"');
                                if (File.Exists(imagePath))
                                {
                                    window.BackgroundImage = Image.FromFile(imagePath);
                                    window.BackgroundImageLayout = ImageLayout.Stretch;
                                    if (isDebuging)
                                        WriteConsole($"Background image set: {imagePath}");
                                }
                                else
                                {
                                    WriteConsole($"❌ Background image not found: {imagePath}");
                                }
                            }
                        }
                        else if (line.StartsWith("playAudio("))
                        {
                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string audioPath = inBrackets.Trim('"');
                                WriteConsole(audioPath);
                                if (File.Exists(audioPath))
                                {
                                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(audioPath);
                                    player.Play();
                                    if (isDebuging)
                                        WriteConsole($"Playing audio: {audioPath}");
                                }
                                else
                                {
                                    WriteConsole($"❌ Audio file not found: {audioPath}");
                                }
                            }
                        }
                        else if (line.StartsWith("setupUmbra(") && !isOpendWindow)
                        {
                            //window.ControlBox = false;
                            //window.FormClosing += MainForm_FormClosing;
                            window = new Form();

                            if (isDebuging)
                                WriteConsole("Open Window");

                            int openParen = line.IndexOf('(');
                            int closeParen = line.IndexOf(')', openParen + 1);
                            if (openParen != -1 && closeParen != -1)
                            {
                                string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                                string[] args = inBrackets.Split(',');
                                if (args.Length >= 3)
                                {
                                    string title = args[0].Trim('"');
                                    int width = int.Parse(args[1]);
                                    int height = int.Parse(args[2]);
                                    stealthPath = Path.Combine(Path.GetDirectoryName(stealthPath), "UmbraWindowLib");
                                    variablesPath = Path.Combine(stealthPath, "Variables", "variables.var");
                                    DirectoryInfo di = Directory.CreateDirectory(Path.GetDirectoryName(variablesPath));
                                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                                    window.Text = title;
                                    window.Width = width;
                                    window.Height = height;
                                }
                            }

                            isOpendWindow = true;
                            window.Show();
                        }
                    }
                    #endregion

                    #region NormalStandartCodeLib

                    if (line.TrimStart().StartsWith("storeInput"))
                    {
                        int openParen = line.IndexOf('(');
                        int closeParen = line.IndexOf(')', openParen + 1);
                        if (openParen != -1 && closeParen != -1)
                        {
                            string inBrackets = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                            string name = inBrackets.Trim('"');
                            using (var dialog = new InputDialog($"Bitte gib '{name}' ein:"))
                            {
                                if (dialog.ShowDialog() == DialogResult.OK)
                                {
                                    SaveVariable(name, dialog.InputValue, variablesPath);
                                    WriteConsole($"> {dialog.InputValue}");
                                }
                                else
                                {
                                    // Dialog abgebrochen, ggf. Variable nicht speichern oder Standardwert
                                    WriteConsole("> Eingabe abgebrochen.");
                                }
                            }
                        }
                        continue;  // Wichtig, sonst wird evtl. nochmal was mit der gleichen Zeile gemacht
                    }
                    if (line.StartsWith("black"))
                    {
                        string inBrackets = line.Substring(line.IndexOf('(') + 1);
                        inBrackets = inBrackets.Substring(0, inBrackets.LastIndexOf(')')).Trim();
                        string output = "";

                        if (inBrackets.Contains("+"))
                        {
                            string[] parts = inBrackets.Split('+');
                            foreach (string part in parts)
                            {
                                string trimmed = part.Trim();
                                output += trimmed.StartsWith("\"") ? trimmed.Trim('"') : GetValueOfVariable(trimmed, variablesPath);
                            }
                        }
                        else
                        {
                            output = inBrackets.StartsWith("\"") ? inBrackets.Trim('"') : GetValueOfVariable(inBrackets, variablesPath);
                        }
                        WriteConsole(output);
                    }
                    else if (line.TrimStart().StartsWith("shadow"))
                    {
                        int openBracket = line.IndexOf('(');
                        int closeBracket = line.IndexOf(')', openBracket + 1);
                        if (openBracket != -1 && closeBracket != -1)
                        {
                            string inBrackets = line.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();
                            string text = inBrackets.Trim('"');
                            if (float.TryParse(text.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float duration))
                            {
                                int ms = (int)(duration * 1000f);
                                Application.DoEvents();
                                Thread.Sleep(ms);
                            }
                        }
                    }
                    else if (line.TrimStart().StartsWith("waitShadow()"))
                    {
                        string Input = Console.ReadLine();
                    }
                    else if (line.TrimStart().StartsWith("code "))
                    {
                        int openParen = line.IndexOf('(');
                        int closeParen = line.IndexOf(')', openParen + 1);
                        int openBrace = line.IndexOf('{', closeParen + 1);

                        if (openParen != -1 && closeParen != -1 && openBrace != -1)
                        {
                            string funcDecl = line.Substring(0, openParen).Trim();
                            string funcName = funcDecl.Split(' ')[1].Trim();

                            // Vollständigen Funktionsblock lesen (inkl. geschachtelter {})
                            List<string> funcLines = ReadBlock(scriptQueue, line);

                            // Funktion speichern
                            functions[funcName] = funcLines;

                            if (isDebuging)
                                WriteConsole($"💾 Funktion '{funcName}' gespeichert");
                        }
                        else
                        {
                            WriteConsole("❌ Fehlerhafte Funktionsdefinition.");
                        }
                    }
                    else if (line.TrimStart().StartsWith("clearWhite()"))
                    {
                        using (StreamWriter writer = new StreamWriter(localVariablesPath, append: false))
                        {

                        }
                    }
                    else if (line.TrimStart().StartsWith("if"))
                    {
                        int openParen = line.IndexOf('(');
                        int closeParen = line.IndexOf(')', openParen + 1);
                        if (openParen != -1 && closeParen != -1)
                        {
                            List<string> blockLines = ReadBlock(scriptQueue, line);
                            string condition = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                            bool conditionMet = EvaluateCondition(condition, variablesPath);

                            if (conditionMet)
                            {
                                i++; // nächste Zeile
                                if (i < lines.Length && lines[i].Trim() == "{")
                                {
                                    i++;
                                    while (i < lines.Length && lines[i].Trim() != "}")
                                    {
                                        CompileScript(lines[i] + "\n", path);
                                        i++;
                                    }
                                }
                                else
                                {
                                    CompileScript(lines[i] + "\n", path);
                                }
                            }
                            else
                            {
                                // Check for "else if" or "else"
                                while (i + 1 < lines.Length)
                                {
                                    string nextLine = lines[i + 1].TrimStart();
                                    if (nextLine.StartsWith("else if"))
                                    {
                                        i++;
                                        line = lines[i];
                                        goto else_if_recheck; // Neuen if-Block prüfen
                                    }
                                    else if (nextLine.StartsWith("else"))
                                    {
                                        i++;
                                        if (i + 1 < lines.Length && lines[i + 1].Trim() == "{")
                                        {
                                            i += 2;
                                            while (i < lines.Length && lines[i].Trim() != "}")
                                            {
                                                CompileScript(lines[i] + "\n", path);
                                                i++;
                                            }
                                        }
                                        else
                                        {
                                            i++;
                                            CompileScript(lines[i] + "\n", path);
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    else_if_recheck:; // Label für goto
                    }
                    else if (line.TrimStart().StartsWith("repeat"))
                    {
                        int openParen = line.IndexOf('(');
                        int closeParen = line.IndexOf(')', openParen + 1);
                        int repeatCount = 0;

                        List<string> blockLines = ReadBlock(scriptQueue, line);

                        if (openParen != -1 && closeParen != -1)
                        {
                            string inside = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();

                            // Wert aus Variablen oder direkt Zahl
                            string value = ReplaceVariables(inside); // optional: falls du Variablen wie $anzahl nutzt
                            int.TryParse(value, out repeatCount);
                        }

                        // Jetzt alle Zeilen im Block sammeln
                        List<string> repeatBlock = new List<string>();
                        int a = Array.IndexOf(lines, line) + 1;

                        int blockDepth = 0;
                        while (a < lines.Length)
                        {
                            string blockLine = lines[a].Trim();

                            if (blockLine == "{")
                            {
                                blockDepth++;
                            }
                            else if (blockLine == "}")
                            {
                                if (blockDepth == 0)
                                    break;
                                else
                                    blockDepth--;
                            }
                            else
                            {
                                repeatBlock.Add(lines[a]);
                            }

                            a++;
                        }

                        // Wiederhole den Block
                        for (int r = 0; r < repeatCount; r++)
                        {
                            // Temporäre Variable speichern für diesen Schleifendurchlauf
                            SaveVariable("repeat_index", r.ToString(), variablesPath);

                            // Skript ausführen
                            CompileScript(string.Join("\n", repeatBlock.ToArray()), "repeatBlock_" + r);
                        }

                        // Springe hinter den Block
                        continue;
                    }
                    else if (line.TrimStart().StartsWith("while"))
                    {
                        int open = line.IndexOf('(');
                        int close = line.IndexOf(')', open + 1);
                        if (open != -1 && close != -1)
                        {
                            string condition = line.Substring(open + 1, close - open - 1).Trim();

                            i++;
                            if (i < lines.Length && lines[i].Trim() == "{")
                            {
                                List<string> blockLines = ReadBlock(scriptQueue, line);
                                //List<string> blockLines = new();
                                int temp = i + 1;
                                while (temp < lines.Length && lines[temp].Trim() != "}")
                                {
                                    blockLines.Add(lines[temp]);
                                    temp++;
                                }

                                while (EvaluateCondition(condition, variablesPath))
                                {
                                    foreach (string l in blockLines)
                                    {
                                        CompileScript(l + "\n", path);
                                    }
                                }

                                i = temp; // springe hinter die while-Klammer
                            }
                        }
                    }
                    else if (line.TrimStart().StartsWith("import("))
                    {
                        int start = line.IndexOf('(');
                        int end = line.LastIndexOf(')');
                        if (start != -1 && end != -1)
                        {
                            string importPath = line.Substring(start + 1, end - start - 1).Trim('"');
                            string fullPath = Path.Combine(Path.GetDirectoryName(path), importPath);
                            if (File.Exists(fullPath))
                            {
                                string imported = File.ReadAllText(fullPath);
                                CompileScript(imported, fullPath);
                            }
                        }
                    }
                    else if (line.Contains("= readFile("))
                    {
                        string[] parts = line.Split(new[] { '=' }, 2);
                        string varName = parts[0].Trim().Split(' ').Last();
                        string filePath = parts[1].Trim();
                        int start = filePath.IndexOf('(');
                        int end = filePath.LastIndexOf(')');
                        string fileName = filePath.Substring(start + 1, end - start - 1).Trim('"');

                        string contentt = File.Exists(fileName) ? File.ReadAllText(fileName) : "";
                        SaveVariable(varName, contentt, variablesPath);
                    }
                    else if (line.TrimStart().StartsWith("writeFile("))
                    {
                        string[] args = ExtractArgs(line);
                        if (args.Length == 2)
                        {
                            File.WriteAllText(args[0], args[1]);
                        }
                    }
                    else if (line.Contains("exists("))
                    {
                        string file = ExtractArgs(line)[0];
                        string result = (File.Exists(file) || File.Exists(Path.Combine(variablesPath, file))) ? "true" : "false";
                        SaveVariable("lastExists", result, variablesPath); // Optional
                    }
                    else if (line.TrimStart().StartsWith("delete("))
                    {
                        string[] args = ExtractArgs(line);
                        if (args.Length == 1 && File.Exists(args[0]))
                        {
                            File.Delete(args[0]);
                        }
                    }
                    else if (line.Contains("= now()"))
                    {
                        string var = line.Split('=')[0].Trim().Split(' ').Last();
                        SaveVariable(var, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), variablesPath);
                    }
                    else if (line.Contains("= timestamp()"))
                    {
                        string var = line.Split('=')[0].Trim().Split(' ').Last();
                        long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        SaveVariable(var, unix.ToString(), variablesPath);
                    }
                    else if (line.Contains("= random("))
                    {
                        string var = line.Split('=')[0].Trim().Split(' ').Last();
                        string[] args = ExtractArgs(line);
                        if (args.Length == 2)
                        {
                            int min = int.Parse(args[0]);
                            int max = int.Parse(args[1]) + 1;
                            SaveVariable(var, rnd.Next(min, max).ToString(), variablesPath);
                        }
                    }
                    else if (line.Contains("= getPath()"))
                    {
                        string var = line.Split('=')[0].Trim().Split(' ').Last();
                        string dir = Path.GetDirectoryName(path);
                        SaveVariable(var, dir, variablesPath);
                    }
                    else if (line.Contains("= listDir("))
                    {
                        string var = line.Split('=')[0].Trim().Split(' ').Last();
                        string[] args = ExtractArgs(line);
                        if (args.Length == 1)
                        {
                            string[] files = Directory.Exists(args[0]) ? Directory.GetFiles(args[0]) : Array.Empty<string>();
                            SaveVariable(var, string.Join(",", files), variablesPath);
                        }
                    }
                    else if (line.TrimStart().StartsWith("mkdir("))
                    {
                        string[] args = ExtractArgs(line);
                        if (args.Length == 1)
                        {
                            Directory.CreateDirectory(args[0]);
                        }
                    }
                    else if (line.TrimStart().StartsWith("lwhite"))
                    {
                        // Save variable in the localVariables.localvar //
                        int firstSpace = line.IndexOf(' ');
                        int equalsSign = line.IndexOf('=');
                        if (firstSpace != -1 && equalsSign != -1 && equalsSign > firstSpace)
                        {
                            string name = line.Substring(firstSpace + 1, equalsSign - firstSpace - 1).Trim();
                            string value = line.Substring(equalsSign + 1).Trim();
                            if (value.StartsWith("\"") && value.EndsWith("\""))
                            {
                                value = value.Substring(1, value.Length - 2);
                            }

                            if (GetValueOfVariable(name, localVariablesPath) != "")
                            {
                                continue;
                            }
                            SaveVariable(name, value, localVariablesPath);
                        }
                    }
                    else if (line.StartsWith("white"))
                    {
                        // Variablendeklaration
                        int firstSpace = line.IndexOf(' ');
                        int equalsSign = line.IndexOf('=');
                        if (firstSpace != -1 && equalsSign != -1 && equalsSign > firstSpace)
                        {
                            string name = line.Substring(firstSpace + 1, equalsSign - firstSpace - 1).Trim();
                            string value = line.Substring(equalsSign + 1).Trim();
                            if (value.StartsWith("\"") && value.EndsWith("\""))
                            {
                                value = value.Substring(1, value.Length - 2);
                            }

                            SaveVariable(name, value, variablesPath);
                        }
                    }
                    else if (line.StartsWith("run "))
                    {
                        string funcToRun = line.Substring(4).Trim();

                        if (functions.ContainsKey(funcToRun))
                        {
                            var funcLines = functions[funcToRun];
                            foreach (string funcLine in funcLines)
                                scriptQueue.Enqueue(funcLine);
                        }
                        else
                        {
                            WriteConsole($"❌ Funktion '{funcToRun}' nicht gefunden");
                        }
                    }
                    /*else if (line.EndsWith("()"))
                    {
                        string funcName = line.Substring(0, line.Length - 2).Trim();
                        string funcFile = Path.Combine(functionsFolder, funcName + ".func");
                        if (File.Exists(funcFile))
                        {
                            string[] funcLines = File.ReadAllLines(funcFile);
                            foreach (string funcLine in funcLines)
                            {
                                CompileScript(funcLine + "\n", Path.Combine(baseDir, "main.scsl"));
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Function '{funcName}' not found.");
                        }
                    }*/

                    #endregion
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "Die Warteschlange ist leer.")
                    WriteConsole("[UmbraLang] Fehler: " + ex.Message + " in line: " + ex.StackTrace);
            }
        }
        public static void SetPlayerY(int newY)
        {
            playerY = newY;
        }
        public static List<int> GetXPositions()
        {
            return xs;
        }
        public static List<int> GetYPositions()
        {
            return ys;
        }
        public static void SetDictionary(List<int> x,List<int> y)
        {
            xs = x;
            ys = y;
        }
        private List<string> ReadBlock(Queue<string> scriptQueue, string firstLine)
        {
            List<string> blockLines = new List<string>();
            int braceDepth = firstLine.Count(c => c == '{') - firstLine.Count(c => c == '}');

            blockLines.Add(firstLine);

            while (braceDepth > 0 && scriptQueue.Count > 0)
            {
                string nextLine = scriptQueue.Dequeue();
                blockLines.Add(nextLine);
                braceDepth += nextLine.Count(c => c == '{') - nextLine.Count(c => c == '}');
            }

            return blockLines;
        }

        private HashSet<Keys> CurrentlyPressedKeys = new HashSet<Keys>();

        /*private void UpdatePressedKeysFromPhysicalState()
        {
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if ((GetAsyncKeyState(key) & 0x8000) != 0)
                    gameWindow.LastKeyPressed = key;
                else
                    gameWindow.LastKeyPressed = Keys.None;
            }
        }*/

        /*[DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);*/

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        private void CheckKeyActions()
        {
            List<KeyAction> prefKeyAction = new List<KeyAction>();
            if (gameWindow == null)
                return;
            try
            {
                // Physisch gedrückte Taste ermitteln
                // Letzte gedrückte Taste erkennen (funktioniert in WinForms)
                bool foundKey = false;
                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    if ((GetAsyncKeyState(key) & 0x8000) != 0)
                    {
                        gameWindow.LastKeyPressed = key;
                        foundKey = true;
                        break;
                    }
                }

                if (!foundKey)
                {
                    gameWindow.LastKeyPressed = Keys.None;
                }
                // Aktionen verarbeiten
                foreach (var action in keyActions)
                {
                    bool isDown = gameWindow.LastKeyPressed == action.TriggerKey;

                    if (action.IsLooping)
                    {
                        if (isDown)
                        {
                            CompileScript(action.CodeBlock + "\n", currentScriptPath);
                        }
                    }
                    else
                    {
                        if (isDown && !action.WasPreviouslyDown)
                        {
                            action.WasPreviouslyDown = true;
                            CompileScript(action.CodeBlock + "\n", currentScriptPath);
                            prefKeyAction.Add(action);
                        }
                        else if (!isDown)
                        {
                            action.WasPreviouslyDown = false;
                        }
                    }
                }
                foreach (KeyAction ka in prefKeyAction)
                {
                    keyActions.Remove(ka);
                }
            }
            catch (Exception ex)
            {
                WriteConsole("[UmbraLang] Fehler: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        string RemoveSpacesOutsideQuotes(string input)
        {
            var result = new StringBuilder();
            bool insideQuotes = false;

            foreach (char c in input)
            {
                if (c == '"')
                {
                    insideQuotes = !insideQuotes;
                    result.Append(c);
                }
                else if (!insideQuotes && char.IsWhiteSpace(c))
                {
                    // Leerzeichen außerhalb von Anführungszeichen → überspringen
                    continue;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        private Label GetLabelByNumber(int i)
        {
            Label l = allLabels[i];
            return l;
        }

        private System.Windows.Forms.TextBox GetInputByNumber(int i)
        {
            System.Windows.Forms.TextBox l = allInput[i];
            return l;
        }

        private string ReplaceVariables(string input)
        {
            return Regex.Replace(input, @"\$(\w+)", match =>
            {
                string varName = match.Groups[1].Value;
                return LoadVariable(varName, variablesPath) ?? "";
            });
        }

        private string LoadVariable(string name, string path)
        {
            if (!File.Exists(path)) return "";

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                if (line.StartsWith(name + "="))
                {
                    return line.Substring(name.Length + 1);
                }
            }
            return "";
        }

        private bool EvaluateCondition(string condition, string variablesPath)
        {
            condition = condition.Trim();

            if (condition.Contains("&&") || condition.Contains("||"))
            {
                string[] orParts = condition.Split(new[] { "||" }, StringSplitOptions.None);
                foreach (string orPart in orParts)
                {
                    string[] andParts = orPart.Split(new[] { "&&" }, StringSplitOptions.None);
                    bool andResult = true;
                    foreach (string part in andParts)
                    {
                        if (!EvaluateSimpleCondition(part.Trim(), variablesPath))
                        {
                            andResult = false;
                            break;
                        }
                    }
                    if (andResult)
                        return true;
                }
                return false;
            }

            return EvaluateSimpleCondition(condition, variablesPath);
        }

        private string[] ExtractArgs(string line)
        {
            int start = line.IndexOf('(');
            int end = line.LastIndexOf(')');
            if (start == -1 || end == -1) return Array.Empty<string>();
            string argsRaw = line.Substring(start + 1, end - start - 1);
            return argsRaw.Split(',').Select(a => a.Trim().Trim('"')).ToArray();
        }

        private bool EvaluateSimpleCondition(string condition, string variablesPath)
        {
            if (condition.Contains("=="))
            {
                string[] parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string left = parts[0].Trim();
                    string right = parts[1].Trim();

                    string leftVal = GetValueOfVariable(left, variablesPath);
                    string rightVal = GetValueOfVariable(right, variablesPath);

                    return leftVal == rightVal;
                }
            }
            return false;
        }

        private void WriteConsole(string text)
        {
            consoleOutput.AppendText(text + Environment.NewLine);
        }

        private void SaveVariable(string name, string value, string path)
        {
            try
            {
                var lines = new List<string>();
                if (File.Exists(path))
                    lines.AddRange(File.ReadAllLines(path));

                bool found = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    var parts = lines[i].Split('=');
                    if (parts[0].Trim() == name)
                    {
                        lines[i] = name + "=" + value;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    lines.Add(name + "=" + value);

                // Atomarer Schreibvorgang, überschreibt die Datei komplett
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                // Optional: Fehler protokollieren, damit der Nutzer informiert wird
                WriteConsole($"Fehler beim Speichern der Variable '{name}': {ex.Message}");
            }
        }

        private string GetValueOfVariable(string name, string path)
        {
            if (!File.Exists(path)) return "";
            foreach (string line in File.ReadAllLines(path))
            {
                var parts = line.Split('=');
                if (parts.Length == 2 && parts[0].Trim() == name)
                    return parts[1].Trim();
            }
            return "";
        }

        private Point GetCaretLocationApprox(ScintillaNET.Scintilla scintilla)
        {
            int caretPos = scintilla.CurrentPosition;

            // Ermittle die Zeile und Spalte des Carets
            int line = scintilla.LineFromPosition(caretPos);
            int col = caretPos - scintilla.Lines[line].Position;

            // Berechne die Position in Pixel (grob)
            // Zeichenbreite kann unterschiedlich sein, hier ein Durchschnittswert nehmen:
            int charWidth = scintilla.TextWidth(0, "M");  // 'M' als breites Zeichen als Referenz
            int charHeight = scintilla.Lines[line].Height;

            int x = col * charWidth;
            int y = line * charHeight;

            return new Point(x, y);
        }
    }
    class Programm
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UmbraLangIDE());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Start:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Drücke Enter zum Beenden...");
                Console.ReadLine();
            }
        }
    }
    public class GameWindow : Form
    {
        public Keys LastKeyPressed { get; set; } = Keys.None;
        Color color = Color.Gray;
        private int playerX = 100;
        private int playerY = 100;
        private Image playerSprite;
        int x;
        int y;
        int sizex;
        int sizey;
        Image image;
        List<int> xl = new List<int>();
        List<int> yl = new List<int>();
        List<int> sizexl = new List<int>();
        List<int> sizeyl = new List<int>();
        List<Image> imagel = new List<Image>();
        public static List<int> gravityObjects = new List<int>();

        private void WriteConsole(string text)
        {
            UmbraLangIDE.consoleOutput.AppendText(text + Environment.NewLine);
        }

        bool CheckCollision(int x1, int y1, int w1, int h1,
                    int x2, int y2, int w2, int h2)
        {
            return x1 < x2 + w2 &&
                   x1 + w1 > x2 &&
                   y1 < y2 + h2 &&
                   y1 + h1 > y2;
        }

        public bool IfCollision()
        {
            for (int i = 0; i < imagel.Count; i++)
            {
                if (CheckCollision(playerX, playerY, 64, 64, xl[i], yl[i], sizexl[i], sizeyl[i]))
                {
                    WriteConsole("🎯 Kollision mit Objekt " + i);
                    return true;
                }
            }
            return false;
        }

        public bool IsPlayerCollidingWithAny(int anotherCount)
        {
            for (int i = 1; i < imagel.Count; i++) // i = 1, weil 0 der Player ist
            {
                if (CheckCollision(
                    xl[0], yl[0], sizexl[0], sizeyl[0],
                    xl[anotherCount], yl[anotherCount], sizexl[anotherCount], sizeyl[anotherCount]
                ))
                {
                    WriteConsole($"🎯 Spieler kollidiert mit Objekt {anotherCount}");
                    return true;
                }
            }
            return false;
        }
        public bool CollideSMTwithPlayerCollidingWithAny(int anotherCount)
        {
            for (int i = 1; i < imagel.Count; i++) // i = 1, weil 0 der Player ist
            {
                if (CheckCollision(
                    xl[anotherCount], yl[anotherCount], sizexl[anotherCount], sizeyl[anotherCount],
                    xl[0], yl[0], sizexl[0], sizeyl[0]
                ))
                {
                    WriteConsole($"🎯 Spieler kollidiert mit Objekt {anotherCount}");
                    return true;
                }
            }
            return false;
        }

        private bool CheckCollision2(int x1, int y1, int w1, int h1,
                            int x2, int y2, int w2, int h2)
        {
            return x1 < x2 + w2 &&
                   x1 + w1 > x2 &&
                   y1 < y2 + h2 &&
                   y1 + h1 > y2;
        }

        public bool IfCollision2(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= imagel.Count || indexB < 0 || indexB >= imagel.Count)
            {
                WriteConsole("❌ Ungültiger Index für Kollisionsprüfung.");
                return false;
            }

            /*if (indexA == 0)
            {
                return IsPlayerCollidingWithAny(indexB);
            }
            else if (indexB == 0)
            {
                return CollideSMTwithPlayerCollidingWithAny(indexB);
            }
            else
            {*/
            return CheckCollision(
                UmbraLangIDE.xs[indexA], UmbraLangIDE.ys[indexA], UmbraLangIDE.sxs[indexA], UmbraLangIDE.sys[indexA],
                UmbraLangIDE.xs[indexB], UmbraLangIDE.ys[indexB], UmbraLangIDE.sxs[indexB], UmbraLangIDE.sys[indexB]
            );
            //}
        }


        public GameWindow(string imagePath)
        {
            this.Text = "Game Preview";
            this.Size = new Size(600, 400);
            this.DoubleBuffered = true;
            this.KeyPreview = true; // wichtig, damit KeyDown funktioniert
            this.Paint += GameWindow_Paint;

            try
            {
                playerSprite = Image.FromFile(imagePath);
            }
            catch
            {
                MessageBox.Show($"Fehler: '{imagePath}' nicht gefunden.");
                playerSprite = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(playerSprite))
                    g.FillRectangle(Brushes.Red, 0, 0, 32, 32);
            }

            this.Paint += GameWindow_Paint;
        }

        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            LastKeyPressed = e.KeyCode;
        }
        private void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            LastKeyPressed = Keys.None;
        }

        public void SetColor(Color colo)
        {
            color = colo;
        }

        public int GetPlayerX()
        {
            return playerX;
        }

        public int GetPlayerY()
        {
            return playerY;
        }

        public void SetPlayerX(int x)
        {
            playerX = x;
            Invalidate();
        }
        public void SetPlayerY(int x)
        {
            playerY = x;
            Invalidate();
        }

        public void Inistialize(string PNGPath, int c, int v, int sizec, int sizev)
        {
            x = c;
            y = v;
            sizex = sizec;
            sizey = sizev;
            // Initalize a new Objekt with custom Image and position
            try
            {
                image = Image.FromFile(PNGPath);
            }
            catch
            {
                MessageBox.Show("Fehler: Image File Not Found.");
                image = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(playerSprite))
                    g.FillRectangle(Brushes.Yellow, x, y, sizex, sizey);
            }
            xl.Add(c);
            yl.Add(v);
            List<int> xx = UmbraLangIDE.GetXPositions();
            List<int> yy = UmbraLangIDE.GetYPositions();

            xx.Add(c);
            yy.Add(v);
            sizexl.Add(sizec);
            sizeyl.Add(sizev);
            imagel.Add(image);

            UmbraLangIDE.SetDictionary(xx, yy);

            this.Paint += GameWindow_Paint;
            Invalidate();
        }
        private void GameWindow_PaintPNG(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(color);
            e.Graphics.DrawImage(image, x, y, 64, 64);
        }
        void Draw(PaintEventArgs e, Image image, int x, int y)
        {
            e.Graphics.DrawImage(image, x, y, 64, 64);
        }
        private void GameWindow_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(color);

            // Für Pixel Art: keine Glättung!
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half; // optional

            if (image != null)
            {
                for (int i = 0; i < imagel.Count; i++)
                {
                    e.Graphics.DrawImage(imagel[i], xl[i], yl[i], sizexl[i], sizeyl[i]);
                }
            }
            e.Graphics.DrawImage(playerSprite, playerX, playerY, 64, 64);
        }
        // In GameWindow.cs
        private HashSet<int> objectsWithGravity = new HashSet<int>();
        private int gravitySpeed = 2;

        public void AddGravityToObject(int index)
        {
            objectsWithGravity.Add(index);
        }

        public void ApplyGravity()
        {
            foreach (int index in objectsWithGravity)
            {
                bool isStandingOnSomething = false;

                for (int i = 0; i < imagel.Count; i++)
                {
                    if (i == index) continue;

                    int futureY = UmbraLangIDE.ys[index] + gravitySpeed;

                    if (CheckCollision2(
                        UmbraLangIDE.xs[index], futureY, UmbraLangIDE.sxs[index], UmbraLangIDE.sys[index],
                        UmbraLangIDE.xs[i], UmbraLangIDE.ys[i], UmbraLangIDE.sxs[i], UmbraLangIDE.sys[i]))
                    {
                        isStandingOnSomething = true;

                        // Sofort genau über dem Objekt absetzen:
                        UmbraLangIDE.ys[index] = UmbraLangIDE.ys[i] - UmbraLangIDE.sys[index];
                        SetObjectY(index, UmbraLangIDE.ys[index]);
                        break;
                    }
                }

                if (!isStandingOnSomething)
                {
                    // Frei → weiter fallen
                    UmbraLangIDE.ys[index] += gravitySpeed;
                    SetObjectY(index, UmbraLangIDE.ys[index]);
                }
                else
                {
                    // ✅ Steht auf etwas → nicht bewegen
                    SetObjectY(index, UmbraLangIDE.ys[index]); // Optional: zum "Einrasten" bei Bodenkontakt
                                                               // Optional: Logging oder Debug
                    //Console.WriteLine($"Objekt {index} steht auf Boden, Gravity gestoppt.");
                }
            }
        }

        public void SetObjectY(int index, int newY)
        {
            if (index >= 0 && index < UmbraLangIDE.ys.Count)
            {
                UmbraLangIDE.ys[index] = newY;
                if (index == 0)
                {
                    playerY = newY;
                    UmbraLangIDE.SetPlayerY(playerY);
                }

                List<int> xx = UmbraLangIDE.GetXPositions();
                List<int> yy = UmbraLangIDE.GetYPositions();

                yy[index] = newY;

                UmbraLangIDE.SetDictionary(xx, yy);

                Invalidate(); // neu zeichnen
            }
            else
            {
                Console.WriteLine($"❌ Ungültiger Index in SetObjectY: {index}");
            }
        }
        private bool IsPlayerNearObject(int objectIndex, int threshold = 20)
        {
            if (objectIndex < 0 || objectIndex >= xl.Count)
                return false;

            int playerCenterX = playerX + 32; // 64/2
            int playerCenterY = playerY + 32;

            int objCenterX = xl[objectIndex] + sizexl[objectIndex] / 2;
            int objCenterY = yl[objectIndex] + sizeyl[objectIndex] / 2;

            int distX = Math.Abs(playerCenterX - objCenterX);
            int distY = Math.Abs(playerCenterY - objCenterY);

            return distX <= threshold && distY <= threshold;
        }
        public void CheckAndTeleportPlayer()
        {
            for (int i = 1; i < xl.Count; i++) // 0 = Player, deswegen ab 1
            {
                if (IsPlayerNearObject(i))
                {
                    // Teleportiere Player zur Position des Objekts (z.B. oben links)
                    SetPlayerX(xl[i]);
                    SetPlayerY(yl[i]);
                    WriteConsole($"Player teleportiert zu Objekt {i}");
                    break; // Nur zu einem Objekt teleportieren
                }
            }
        }
    }

    public class KeyAction
    {
        public Keys TriggerKey;
        public string CodeBlock;
        public bool IsLooping;
        public bool WasPreviouslyDown;
    }
}
