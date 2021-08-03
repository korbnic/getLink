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

        string VaultID;
        string FileID;
        string ProjectID;
        string fName;
        string fPath;
        enum Action { OPEN, VIEW, EXPLORE, GET, LOCK, PROPERTIES, HISTORY };
        enum Format { RAW, PATH, FORMATED };
        string[] formatedAction = new string[7];
        bool Canceled = false;
        sbyte lastAction = 2;
        private const string html = @"Version:0.9
StartHTML:<<<<<<<1
EndHTML:<<<<<<<2
StartFragment:<<<<<<<3
EndFragment:<<<<<<<4
SourceURL: {0}
<html>
<body>
<!--StartFragment-->
<a href='{0}'>{1}</a>
<!--EndFragment-->
</body>
</html>";

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

        public addinWindow(in string vID, in int pID, in int fID, in string fN, in string fP)
        {
            InitializeComponent();
            VaultID = vID;
            ProjectID = pID.ToString();
            FileID = fID.ToString();
            fName = fN;
            fPath = fP;
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
        private void updatePreview()
        {
            string previewString = "";
            switch (readFormatINDEX())
            {
                case 0:
                    Action ActionID = (Action)Enum.ToObject(typeof(Action), readActionINDEX());
                    previewString = @"conisio://" + VaultID + "/" + ActionID + "?projectid=" + ProjectID + "&documentid=" + FileID + "&objecttype=1";
                    tbPREVIEW.TextDecorations = null;
                    tbPREVIEW.Foreground = Brushes.Black;
                    break;
                case 1:
                    if (IsFileNameIncluded.IsChecked == true)
                    {
                        previewString = fPath + "\\" + fName;
                    }
                    else previewString = fPath + "\\";
                    tbPREVIEW.TextDecorations = null;
                    tbPREVIEW.Foreground = Brushes.Black;
                    break;
                case 2:
                    if (!IsFileNameIncluded.IsChecked == true & String.IsNullOrWhiteSpace(prefixText.Text))
                    {
                        MessageBox.Show("The text representation of the link cannot be empty. The program automatically included the file name in it.");
                        IsFileNameIncluded.IsChecked = true;
                        previewString = fName;
                        break;
                    }
                    if (IsFileNameIncluded.IsChecked == true)
                    {
                        if (!String.IsNullOrWhiteSpace(prefixText.Text))
                            previewString = prefixText.Text + " " + fName;
                        else previewString = fName;
                    }
                    else previewString = prefixText.Text;
                    tbPREVIEW.TextDecorations = TextDecorations.Underline;
                    tbPREVIEW.Foreground = Brushes.Blue;
                    break;
            }
            tbPREVIEW.ToolTip = previewString;
            tbPREVIEW.Text = previewString;
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
            Action ActionID = (Action)Enum.ToObject(typeof(Action), readActionINDEX());
            string coniLink = String.Format("conisio://{0}/{1}?projectid={2}&documentid={3}&objecttype=1", VaultID, ActionID, ProjectID, FileID);
            switch (readFormatINDEX())
            {
                case 0:
                    Clipboard.SetText(coniLink);
                    break;
                case 1:
                    if (IsFileNameIncluded.IsChecked == true)
                    {
                        Clipboard.SetText(fPath + "\\" + fName);
                    }
                    else Clipboard.SetText(fPath + "\\");
                    break;
                case 2:
                    string URLname;
                    if (IsFileNameIncluded.IsChecked ==true)
                    {
                        if (!String.IsNullOrWhiteSpace(prefixText.Text))
                            URLname = prefixText.Text + " " + fName;
                        else URLname = fName;
                    }
                    else URLname = prefixText.Text;
                    string webLink = String.Format(html, coniLink, URLname);
                    Clipboard.SetText(webLink, TextDataFormat.Html);
                    break;
            }
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
            string coniLink = @"conisio://" + VaultID + "/EXPLORE?projectid=" + ProjectID + "&documentid=" + FileID + "&objecttype=1";
            Process.Start(coniLink);
            Close();
        }
    }
}
