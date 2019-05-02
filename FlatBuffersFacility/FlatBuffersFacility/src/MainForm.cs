using System;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace FlatBuffersFacility
{
    public class MainForm : Form
    {
        private TextBox namespaceTextBox;
        private CheckBox generatePoolVersionCheckBox;

        public MainForm()
        {
            Title = "FlatBuffersFacilityGenerator";
            ClientSize = new Size(800, 400);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            AppData.Init();
            Content = ConstructLayout();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            AppData.TargetNamespace = namespaceTextBox.Text;
            AppData.SaveConfig();
        }

        private TableLayout ConstructLayout()
        {
            TableLayout layout = new TableLayout();
            layout.Spacing = new Size(10, 10);

            var filePickerLayout = ConstructFbsFilePickerLayout();
            var buttonLayout = ConstructButtonLayout();
            layout.Rows.Add(new TableRow {Cells = {filePickerLayout, buttonLayout}});

            return layout;
        }

        #region 左侧文件复选框

        private string[] fbsFileNames;

        private TableLayout ConstructFbsFilePickerLayout()
        {
            TableLayout layout = new TableLayout();

            fbsFileNames = GetFbsFileNamesInDirectory(AppData.FbsDirectory);

            if (fbsFileNames != null && fbsFileNames.Length > 0)
            {
                Scrollable scrollable = new Scrollable();
                scrollable.Height = 300;

                TableLayout scrollContentLayout = new TableLayout();
                scrollContentLayout.Spacing = new Size(30, 10);
                for (int i = 0; i < fbsFileNames.Length; i++)
                {
                    string fileName = fbsFileNames[i];
                    Label fileNameLabel = new Label {Text = fileName, Width = 200};
                    scrollContentLayout.Rows.Add(fileNameLabel);
                }

                scrollContentLayout.Rows.Add(new TableRow {ScaleHeight = true});

                scrollable.Content = scrollContentLayout;
                layout.Rows.Add(scrollable);

                //添加按钮
                Button refreshButton = new Button {Text = "刷新"};
                refreshButton.Click += (sender, args) => { ReConstructWindowLayout(); };

                TableLayout fileButtonLayout = new TableLayout {Spacing = new Size(10, 10)};
                fileButtonLayout.Rows.Add(new TableRow {Cells = {refreshButton}});
                layout.Rows.Add(fileButtonLayout);
            }
            else
            {
                layout.Rows.Add(new Label {Text = "路径内没有.fbs文件"});
            }

            layout.Rows.Add(new TableRow {ScaleHeight = true});
            return layout;
        }

        private string[] GetFbsFileNamesInDirectory(string fbsDirectory)
        {
            if (!Directory.Exists(fbsDirectory))
            {
                return null;
            }

            string[] filePaths = Directory.GetFiles(fbsDirectory, "*.fbs");
            for (int i = 0; i < filePaths.Length; i++)
            {
                filePaths[i] = Path.GetFileName(filePaths[i]);
            }

            return filePaths;
        }

        #endregion

        #region 右侧按钮

        private TableLayout ConstructButtonLayout()
        {
            TableLayout layout = new TableLayout();

            layout.Spacing = new Size(10, 10);
            layout.Padding = new Padding(10, 10, 10, 10);

            Button selectFbsDirectoryButton = new Button {Text = "选择文件夹"};
            selectFbsDirectoryButton.Click += OnSelectFbsDirectoryBtnClick;
            Button openFbsDirectoryBtn = new Button {Text = "打开所在文件夹"};
            openFbsDirectoryBtn.Click += OnOpenFbsDirectoryBtnClick;
            TableLayout selectFbsDirectoryLayout = new TableLayout();
            selectFbsDirectoryLayout.Rows.Add(new TableRow
            {
                Cells =
                {
                    new TableCell {Control = new Label {Text = $".fbs文件路径: {AppData.FbsDirectory}"}, ScaleWidth = true},
                    new TableCell {Control = new Panel {Content = selectFbsDirectoryButton, Size = new Size(100, 30)}},
                    new TableCell {Control = new Panel {Content = openFbsDirectoryBtn, Size = new Size(100, 30)}},
                }
            });
            layout.Rows.Add(selectFbsDirectoryLayout);

            Button selectCompilerBtn = new Button {Text = "选择文件"};
            selectCompilerBtn.Click += OnSelectCompilerBtnClick;
            Button openCompilerDirectoryBtn = new Button {Text = "打开所在文件夹"};
            openCompilerDirectoryBtn.Click += OnOpenCompilerDirectoryBtnClick;
            TableLayout selectCompilerLayout = new TableLayout();
            selectCompilerLayout.Rows.Add(new TableRow
            {
                Cells =
                {
                    new TableCell
                    {
                        Control = new Label {Text = $"compiler文件路径: {AppData.CompilerPath}"}, ScaleWidth = true
                    },
                    new TableCell {Control = new Panel {Content = selectCompilerBtn, Size = new Size(100, 30)}},
                    new TableCell {Control = new Panel {Content = openCompilerDirectoryBtn, Size = new Size(100, 30)}},
                }
            });
            layout.Rows.Add(selectCompilerLayout);

            Button selectCSharpDirectoryBtn = new Button {Text = "选择文件夹"};
            selectCSharpDirectoryBtn.Click += OnSelectCSharpDirectoryBtnClick;
            Button openCSharpDirectoryBtn = new Button {Text = "打开所在文件夹"};
            openCSharpDirectoryBtn.Click += OnOpenCSharpDirectoryBtnClick;
            TableLayout selectCSharpDirectoryLayout = new TableLayout();
            selectCSharpDirectoryLayout.Rows.Add(new TableRow
            {
                Cells =
                {
                    new TableCell
                    {
                        Control = new Label {Text = $"csharp文件输出路径: {AppData.CsOutputDirectory}"}, ScaleWidth = true
                    },
                    new TableCell {Control = new Panel {Content = selectCSharpDirectoryBtn, Size = new Size(100, 30)}},
                    new TableCell {Control = new Panel {Content = openCSharpDirectoryBtn, Size = new Size(100, 30)}},
                }
            });
            layout.Rows.Add(selectCSharpDirectoryLayout);

            namespaceTextBox = new TextBox {Text = AppData.TargetNamespace};
            TableLayout namespaceInputLayout = new TableLayout();
            namespaceInputLayout.Rows.Add(new TableRow
            {
                Cells = {new TableCell {Control = new Label {Text = "输出命名空间:"}}, new TableCell {Control = namespaceTextBox}},
            });
            layout.Rows.Add(namespaceInputLayout);

            generatePoolVersionCheckBox = new CheckBox {Text = "生成对象池版本", Checked = AppData.IsGeneratePoolVersion};
            generatePoolVersionCheckBox.CheckedChanged += (sender, args) =>
            {
                AppData.IsGeneratePoolVersion = generatePoolVersionCheckBox.Checked.Value;
                AppData.SaveConfig();
            };
            TableLayout miscLayout = new TableLayout();
            miscLayout.Rows.Add(new TableRow {Cells = {new TableCell {Control = generatePoolVersionCheckBox}}});
            layout.Rows.Add(miscLayout);

            //generate button
            Button generateButton = new Button {Text = "生成", Width = 100, Height = 50};
            generateButton.Click += GenerateCodes;
            TableLayout generateButtonLayout = new TableLayout();
            generateButtonLayout.Rows.Add(new TableRow
            {
                Cells = {new TableCell {ScaleWidth = true}, generateButton, new TableCell {ScaleWidth = true}}
            });
            layout.Rows.Add(generateButtonLayout);

            layout.Rows.Add(new TableRow {ScaleHeight = true});
            return layout;
        }

        private void GenerateCodes(object sender, EventArgs e)
        {
            if (fbsFileNames.Length == 0)
            {
                return;
            }

            if (AppData.TargetNamespace != namespaceTextBox.Text)
            {
                AppData.TargetNamespace = namespaceTextBox.Text;
                AppData.SaveConfig();
            }

            CodeGenerator.Generate(AppData.TargetNamespace, fbsFileNames, AppData.IsGeneratePoolVersion);
        }

        private void OnOpenCSharpDirectoryBtnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AppData.CsOutputDirectory) || !Directory.Exists(AppData.CsOutputDirectory))
            {
                return;
            }

            Application.Instance.Open(AppData.CsOutputDirectory);
        }

        private void OnSelectCSharpDirectoryBtnClick(object sender, EventArgs e)
        {
            SelectFolderDialog selectFolderDialog = new SelectFolderDialog
            {
                Title = "选择csharp输出文件夹", Directory = AppData.CsOutputDirectory
            };
            if (selectFolderDialog.ShowDialog(this) == DialogResult.Ok)
            {
                AppData.CsOutputDirectory = selectFolderDialog.Directory;
                ReConstructWindowLayout();
            }
        }

        private void OnOpenCompilerDirectoryBtnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AppData.CompilerPath))
            {
                return;
            }

            string directoryName = Path.GetDirectoryName(AppData.CompilerPath);
            if (!Directory.Exists(directoryName))
            {
                return;
            }

            Application.Instance.Open(directoryName);
        }

        private void OnSelectCompilerBtnClick(object sender, EventArgs e)
        {
            string targetFilePath = AppData.CompilerPath;
            targetFilePath = string.IsNullOrEmpty(targetFilePath)
                ? AppDomain.CurrentDomain.BaseDirectory
                : Path.GetDirectoryName(AppData.CompilerPath);
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择flatbuffers compiler文件",
                MultiSelect = false,
                Directory = new Uri(targetFilePath ?? throw new NullReferenceException())
            };
            openFileDialog.Filters.Add(new FileFilter("flatc", ".exe"));
            if (openFileDialog.ShowDialog(this) == DialogResult.Ok)
            {
                string filePath = openFileDialog.FileName;
                if (File.Exists(filePath))
                {
                    AppData.CompilerPath = filePath;
                    ReConstructWindowLayout();
                }
            }
        }

        private void OnOpenFbsDirectoryBtnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AppData.FbsDirectory) || !Directory.Exists(AppData.FbsDirectory))
            {
                return;
            }

            Application.Instance.Open(AppData.FbsDirectory);
        }

        private void OnSelectFbsDirectoryBtnClick(object sender, EventArgs e)
        {
            SelectFolderDialog selectFolderDialog = new SelectFolderDialog
            {
                Title = "选择.fbs文件夹", Directory = AppData.FbsDirectory
            };
            if (selectFolderDialog.ShowDialog(this) == DialogResult.Ok)
            {
                AppData.FbsDirectory = selectFolderDialog.Directory;
                ReConstructWindowLayout();
            }
        }

        #endregion

        private void ReConstructWindowLayout()
        {
            Content = ConstructLayout();
        }
    }
}