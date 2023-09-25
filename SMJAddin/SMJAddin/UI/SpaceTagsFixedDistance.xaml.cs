using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SMJAddin.UI
{
    /// <summary>
    /// Interaction logic for SpaceTagsFixedDistance.xaml
    /// </summary>
    public partial class SpaceTagsFixedDistance : UserControl
    {
        public UIDocument UIDoc;
        public Document doc;

        public SpaceTagsFixedDistance(UIDocument UIdocument)
        {
            InitializeComponent();
            UIDoc = UIdocument;
            doc = UIDoc.Document;

            Window AWindow = new Window()
            {
                Name = "AWindow",
                Content = Content,
                SizeToContent = SizeToContent.Manual,
                Height = 450,
                Width = 450,
            };

            IntPtr revitWindowHandle = Process.GetCurrentProcess().MainWindowHandle;

            WindowInteropHelper helper = new WindowInteropHelper(AWindow);
            helper.Owner = revitWindowHandle;
            AWindow.Show();

        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Change Tag Orientation");
                
                trans.Commit();
            }
        }
    }
}
