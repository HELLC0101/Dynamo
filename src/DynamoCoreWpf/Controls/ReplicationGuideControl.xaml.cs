using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for ReplicationGuideControl.xaml
    /// </summary>
    public partial class ReplicationGuideControl : UserControl
    {
        public ReplicationGuideControl()
        {
            InitializeComponent();
        }

        private void AddReplicationGuideButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PortViewModel;
            if (vm == null) return;

            vm.AddReplicationGuide();
        }

        private void RemoveReplicationGuideButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PortViewModel;
            if (vm == null) return;

            vm.RemoveReplicationGuide();
        }

        private void UpDownBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vm = DataContext as PortViewModel;
            if (vm == null) return;

            vm.PortModel.Owner.OnNodeModified();
        }
    }
}
