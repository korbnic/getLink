//*********************************************************************
//getLink
//Copyright (c) 2021 Nikolay Korobiy
//Product URL: https://github.com/korbnic/getLink
//License: https://github.com/korbnic/getLink/blob/main/LICENSE
//*********************************************************************
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace getLink
{
    public struct FileData
    {
        public string VaultName;
        public string FileID;
        public string FileName;
        public string FolderID;
        public string FolderPath;
    }

    public partial class addinWindow : Window
    {
        // https://stackoverflow.com/questions/1009983/help-button
        private const uint WS_EX_CONTEXTHELP = 0x00000400;
        private const uint WS_MINIMIZEBOX = 0x00020000;
        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_FRAMECHANGED = 0x0020;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CONTEXTHELP = 0xF180;

        private FileData[] F2P;

        enum Action { OPEN, VIEW, EXPLORE, GET, LOCK, PROPERTIES, HISTORY };
        enum Format { RAW, PATH, FORMATED };
        string[] formatedAction = new string[7]; //SourceURL: {0}
        bool Canceled = false;
        sbyte lastAction = 2;
        private const string html = @"Version:0.9
StartHTML:<<<<<<<1
EndHTML:<<<<<<<2
StartFragment:<<<<<<<3
EndFragment:<<<<<<<4
<html>
<body>
<!--StartFragment-->
{0}
<!--EndFragment-->
</body>
</html>";
        private const string anchor = "<a href='{0}'>{1}</a>";

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, uint newStyle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            uint styles = GetWindowLong(hwnd, GWL_STYLE);
            styles &= 0xFFFFFFFF ^ (WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
            SetWindowLong(hwnd, GWL_STYLE, styles);
            styles = GetWindowLong(hwnd, GWL_EXSTYLE);
            styles |= WS_EX_CONTEXTHELP;
            SetWindowLong(hwnd, GWL_EXSTYLE, styles);
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HelpHook);
        }

        private IntPtr HelpHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND &&
                    ((int)wParam & 0xFFF0) == SC_CONTEXTHELP)
            {
                AboutWindow AboutW = new AboutWindow();
                AboutW.Owner = this;
                AboutW.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                AboutW.ShowDialog();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public addinWindow(ref FileData[] FP)
        {
            InitializeComponent();
            F2P = FP;
            readSettings();
            updatePreview();
        }
        private sbyte readActionINDEX()
        {
            if (rbOPEN.IsChecked == true)
                return 0;
            else if (rbVIEW.IsChecked == true)
                return 1;
            else if (rbEXPLORE.IsChecked == true)
                return 2;
            else if (rbGET.IsChecked == true)
                return 3;
            else if (rbLOCK.IsChecked == true)
                return 4;
            else if (rbPROPERTIES.IsChecked == true)
                return 5;
            else if (rbHISTORY.IsChecked == true)
                return 6;
            else return -1;
        }
        private sbyte readFormatINDEX()
        {
            if (rbRAW.IsChecked == true)
                return 0;
            else if (rbPATH.IsChecked == true)
                return 1;
            else if (rbFORMATED.IsChecked == true)
                return 2;
            else return -1;
        }
        private void readSettings()
        {
            // moving the window to the saved position
            this.Top = Properties.Settings.Default.WindowLocationTop;
            this.Left = Properties.Settings.Default.WindowLocationLeft;

            //считываем сохраненные значения суффикса для форматированной ссылки
            formatedAction[0] = Properties.Settings.Default.OPEN0;
            formatedAction[1] = Properties.Settings.Default.VIEW1;
            formatedAction[2] = Properties.Settings.Default.EXPLORE2;
            formatedAction[3] = Properties.Settings.Default.GET3;
            formatedAction[4] = Properties.Settings.Default.LOCK4;
            formatedAction[5] = Properties.Settings.Default.PROPERTIES5;
            formatedAction[6] = Properties.Settings.Default.HISTORY6;

            //reading the saved link format and marking affected radiobutton
            switch (Properties.Settings.Default.Format)
            {
                case (int)Format.RAW:
                    rbRAW.IsChecked = true;
                    break;
                case (int)Format.PATH:
                    rbPATH.IsChecked = true;
                    break;
                case (int)Format.FORMATED:
                    rbFORMATED.IsChecked = true;
                    break;
                default:
                    rbRAW.IsChecked = true;
                    break;
            }

            //reading the saved action and marking affected radiobutton
            switch (Properties.Settings.Default.Action)
            {
                case (int)Action.OPEN:
                    rbOPEN.IsChecked = true;
                    break;
                case (int)Action.VIEW:
                    rbVIEW.IsChecked = true;
                    break;
                case (int)Action.EXPLORE:
                    rbEXPLORE.IsChecked = true;
                    break;
                case (int)Action.GET:
                    rbGET.IsChecked = true;
                    break;
                case (int)Action.LOCK:
                    rbLOCK.IsChecked = true;
                    break;
                case (int)Action.PROPERTIES:
                    rbPROPERTIES.IsChecked = true;
                    break;
                case (int)Action.HISTORY:
                    rbHISTORY.IsChecked = true;
                    break;
                default:
                    rbEXPLORE.IsChecked = true;
                    break;
            }

            //remember the current actionId, in order to use it to save the entered prefix when switching the Action type
            lastAction = readActionINDEX();

            //fill in the prefix field depending on the selected Action
            prefixText.Text = formatedAction[lastAction];

            //if the format is RAW, PATH, then we make the prefix input field inactive
            if (rbRAW.IsChecked == true || rbPATH.IsChecked == true)
                prefixText.IsEnabled = false;
            else this.prefixText.IsEnabled = true;

            //setting the saved state IsFileNameIncluded
            if (Properties.Settings.Default.IsFileNameIncluded == true)
                IsFileNameIncluded.IsChecked = true;
            else IsFileNameIncluded.IsChecked = false;

            if (rbPATH.IsChecked == true)
            {
                prefixText.IsEnabled = false;
                gbACTION.IsEnabled = false;
            }
            else
            {
                prefixText.IsEnabled = true;
                gbACTION.IsEnabled = true;
            }
            if (rbRAW.IsChecked == true)
            {
                IsFileNameIncluded.IsEnabled = false;
                prefixText.IsEnabled = false;
            }
            updatePreview();
        }

        private IDataObject GetOutput(in bool isPreview)
        {
            IDataObject tempDO = new DataObject();
            string OutputString = null;
            Action ActionID = (Action)Enum.ToObject(typeof(Action), readActionINDEX());

            switch (readFormatINDEX())
            {
                case 0:
                    if (F2P.Length == 1) OutputString = String.Format("conisio://{0}/{1}?projectid={2}&documentid={3}&objecttype=1", F2P[0].VaultName, ActionID, F2P[0].FolderID, F2P[0].FileID);
                    else
                        for (int i = 0; i < F2P.Length; i++)
                        {
                            if (F2P.Length - i > 1) OutputString = OutputString + "[" + F2P[i].FileName + "](" + string.Format("conisio://{0}/{1}?projectid={2}&documentid={3}&objecttype=1", F2P[i].VaultName, ActionID, F2P[i].FolderID, F2P[i].FileID) + ")" + Environment.NewLine;
                            else OutputString = OutputString + "[" + F2P[i].FileName + "](" + string.Format("conisio://{0}/{1}?projectid={2}&documentid={3}&objecttype=1", F2P[i].VaultName, ActionID, F2P[i].FolderID, F2P[i].FileID) + ")";
                        }
                    tempDO.SetData(DataFormats.Text, OutputString);
                    break;
                case 1:
                    if (IsFileNameIncluded.IsChecked == true)
                        for (int i = 0; i < F2P.Length; i++)
                        {
                            if (F2P.Length - i > 1) OutputString = OutputString + F2P[i].FolderPath + "\\" + F2P[i].FileName + Environment.NewLine;
                            else OutputString = OutputString + F2P[i].FolderPath + "\\" + F2P[i].FileName;
                        }
                    else OutputString = F2P[0].FolderPath + "\\";
                    tempDO.SetData(DataFormats.Text, OutputString);
                    break;
                case 2:
                    string PREFIX = prefixText.Text;
                    string OutputStringPlain = null;
                    string OutputStringPreview = null;
                    if (!IsFileNameIncluded.IsChecked == true & String.IsNullOrWhiteSpace(PREFIX))
                    {
                        MessageBox.Show("The text representation of the link cannot be empty. The program automatically included the file name in it.");
                        IsFileNameIncluded.IsChecked = true;
                    }
                    for (int i = 0; i < F2P.Length; i++)
                    {
                        string URLname = null;
                        string coniLink = string.Format("conisio://{0}/{1}?projectid={2}&documentid={3}&objecttype=1", F2P[i].VaultName, ActionID, F2P[i].FolderID, F2P[i].FileID);

                        if (IsFileNameIncluded.IsChecked == true)
                        {
                            if (!string.IsNullOrWhiteSpace(PREFIX)) URLname = PREFIX + " " + F2P[i].FileName;
                            else URLname = F2P[i].FileName;
                        }
                        else if (F2P.Length == 1) URLname = PREFIX;
                        else URLname = PREFIX + $"({i + 1:00})";

                        string webLink = string.Format(anchor, coniLink, URLname);

                        if (F2P.Length - i > 1)
                        {
                            OutputStringPlain = OutputStringPlain + "[" + URLname + "](" + coniLink + ")" + Environment.NewLine;
                            OutputString = OutputString + webLink + "<br />";
                            OutputStringPreview = OutputStringPreview + URLname + Environment.NewLine;
                        }
                        else
                        {
                            OutputStringPlain = OutputStringPlain + "[" + URLname + "](" + coniLink + ")";
                            OutputString = OutputString + webLink;
                            OutputStringPreview = OutputStringPreview + URLname;
                        }
                    }
                    if (isPreview) tempDO.SetData(DataFormats.Text, OutputStringPreview);
                    else
                    {
                        tempDO.SetData(DataFormats.Html, string.Format(html, OutputString));
                        tempDO.SetData(DataFormats.Text, OutputStringPlain);
                    }
                    break;
            }
            return tempDO;
        }

        private void updatePreview()
        {
            switch (readFormatINDEX())
            {
                case 0:
                case 1:
                    tbPREVIEW.TextDecorations = null;
                    tbPREVIEW.Foreground = Brushes.Black;
                    break;
                case 2:
                    tbPREVIEW.TextDecorations = TextDecorations.Underline;
                    tbPREVIEW.Foreground = Brushes.Blue;
                    break;
            }
            tbPREVIEW.ToolTip = GetOutput(true).GetData(typeof(string));
            tbPREVIEW.Text = GetOutput(true).GetData(typeof(string)).ToString();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Canceled)
            {
                Properties.Settings.Default.Format = readFormatINDEX();
                Properties.Settings.Default.Action = readActionINDEX();
                Properties.Settings.Default.OPEN0 = formatedAction[0];
                Properties.Settings.Default.VIEW1 = formatedAction[1];
                Properties.Settings.Default.EXPLORE2 = formatedAction[2];
                Properties.Settings.Default.GET3 = formatedAction[3];
                Properties.Settings.Default.LOCK4 = formatedAction[4];
                Properties.Settings.Default.PROPERTIES5 = formatedAction[5];
                Properties.Settings.Default.HISTORY6 = formatedAction[6];
                Properties.Settings.Default.WindowLocationTop = Top;
                Properties.Settings.Default.WindowLocationLeft = Left;
                Properties.Settings.Default.IsFileNameIncluded = (bool)IsFileNameIncluded.IsChecked;
                Properties.Settings.Default.Save();
            }
        }
        private void rbAction_Click(object sender, RoutedEventArgs e)
        {
            formatedAction[lastAction] = prefixText.Text;
            prefixText.Text = formatedAction[readActionINDEX()];
            lastAction = (sbyte)readActionINDEX();
            updatePreview();
        }
        private void bCOPY_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(GetOutput(false));
            Close();
        }
        private void bCANCEL_Click(object sender, RoutedEventArgs e)
        {
            Canceled = true;
            Close();
        }
        private void rbRAW_Click(object sender, RoutedEventArgs e)
        {
            prefixText.IsEnabled = false;
            gbACTION.IsEnabled = true;
            IsFileNameIncluded.IsEnabled = false;
            updatePreview();
        }
        private void rbPATH_Click(object sender, RoutedEventArgs e)
        {
            prefixText.IsEnabled = false;
            gbACTION.IsEnabled = false;
            IsFileNameIncluded.IsEnabled = true;
            updatePreview();
        }
        private void rbFORMATED_Click(object sender, RoutedEventArgs e)
        {
            prefixText.IsEnabled = true;
            gbACTION.IsEnabled = true;
            IsFileNameIncluded.IsEnabled = true;
            updatePreview();
        }
        private void prefixText_TextChanged(object sender, TextChangedEventArgs e)
        {
            formatedAction[readActionINDEX()] = prefixText.Text;
            updatePreview();
        }
        private void IsFileNameIncluded_Click(object sender, RoutedEventArgs e)
        {
            updatePreview();
        }
        private void bRESTORE_Click(object sender, RoutedEventArgs e)
        {
            var dialogResult = MessageBox.Show("Do you really want to restore default prefix text value for current action?", "Restore to Default Value?", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                switch (readActionINDEX())
                {
                    case 0:
                        this.prefixText.Text = "Open";
                        break;
                    case 1:
                        this.prefixText.Text = "View";
                        break;
                    case 2:
                        this.prefixText.Text = "Explore";
                        break;
                    case 3:
                        this.prefixText.Text = "Get";
                        break;
                    case 4:
                        this.prefixText.Text = "Lock";
                        break;
                    case 5:
                        this.prefixText.Text = "Properties of";
                        break;
                    case 6:
                        this.prefixText.Text = "History of";
                        break;
                }
                updatePreview();
            }
        }
        private void bLOCATE_Click(object sender, RoutedEventArgs e)
        {
            string coniLink = @"conisio://" + F2P[0].VaultName + "/EXPLORE?projectid=" + F2P[0].FolderID + "&documentid=" + F2P[0].FileID + "&objecttype=1";
            Process.Start(coniLink);
            Close();
        }
    }
}
