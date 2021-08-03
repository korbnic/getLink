//*********************************************************************
//getLink
//Copyright (c) 2021 Nikolay Korobiy
//Product URL: https://github.com/korbnic/getLink
//License: https://github.com/korbnic/getLink/blob/main/LICENSE
//*********************************************************************

using System.Windows;

namespace getLink
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }
    }


}
