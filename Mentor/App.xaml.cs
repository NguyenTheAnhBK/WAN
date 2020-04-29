using Mentor.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mentor.MVVM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //InitializeComponent();

            BuildDependencies();
            MainViewModel mainViewModel = new MainViewModel();
        }

        private void BuildDependencies()
        {
            if (ServiceLocator.Instance.Built)
                return;

            ServiceLocator.Instance.RegisterViewModels();
            ServiceLocator.Instance.Build();
        }
    }
}
