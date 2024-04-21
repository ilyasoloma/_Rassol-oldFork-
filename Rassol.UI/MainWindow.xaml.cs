using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Rassol.Core;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using Rassol.Core.Class;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;
using System.Text.RegularExpressions;

namespace Rassol
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        // public event System.Windows.Forms.TabControlEventHandler Selected;
        private TreeViewItem draggedItem;

        public Rassol.Core.Core RassolCore = new Rassol.Core.Core();
        //private BindingList<Headquarters> _headquarters;
        //private BindingList<Squad> _squad;
        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedItem = mainTabControl.SelectedItem as TabItem;

            if (selectedItem != null)
            {
                if (selectedItem.Name.ToString() == "previewTab")
                {
                    Console.WriteLine("заебись");
                }
            }
        }
            public MainWindow()
        {
            InitializeComponent();
            
            // previewTab.IsEnabled = false;
        }
        public bool checkPath()
        {
            if (pathFileSquad.Text != "" && pathFileHome.Text != "")
            {
                try
                {
                    RassolCore.ParseCsvFiles();
                    //_headquarters = new BindingList<Headquarters>(RassolCore.Headquarters);
                    //_squad = new BindingList<Squad>(RassolCore.Squads);
                    headquartersData.ItemsSource = RassolCore.Headquarters;
                    //squadEditor.ItemsSource = RassolCore.Squads;
                    homeData.ItemsSource = RassolCore.Home;
                    //_squad.ListChanged += _squad_ListChanged;
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("Долбоеб, файлы неправильные. В шары не ебись, проверь путь и структуру файлов");
                    pathFileSquad.Text = "";
                    pathFileHome.Text = "";
                }
                previewTab.IsEnabled = true;
                
                return true;
            }
            return false;
        }
        private void _squad_ListChanged(object sender, ListChangedEventArgs e)
        {
            /*switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    break;
                case ListChangedType.ItemDeleted:
                    break;
                case ListChangedType.
            }*/

            throw new NotImplementedException();
        }
        private void buttonOpenSquad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            //MessageBox.Show(((TabItem)mainTabControl.SelectedItem).Name.ToString());
            //openFileDialog.Filter = ".csv files (*.csv)*.csv | All files (*.*) | *.*";
            if (openFileDialog.ShowDialog() == true)
            {
                pathFileSquad.Text = openFileDialog.FileName;
                RassolCore.pathCsvSquad = openFileDialog.FileName;
                checkPath();
            }

        }
        private void buttonOpenHome_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            //openFileDialog.Filter = ".csv files (*.csv)*.csv | All files (*.*) | *.*";
            if (openFileDialog.ShowDialog() == true)
            {
                pathFileHome.Text = openFileDialog.FileName;
                RassolCore.pathCsvRoom = openFileDialog.FileName;
                checkPath();
            }
        }
        private void previewTab_MouseClick(Object sender, System.Windows.Forms.TabControlEventArgs e)
        {
            System.Windows.MessageBox.Show("Заебись");
        }


        public void ErrorMsg(string msg)
        {
            System.Windows.Forms.MessageBox.Show(msg);
        }

        private void homeData_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if(homeData.SelectedIndex != -1)
            roomEditor.ItemsSource = RassolCore.Home[homeData.SelectedIndex].Room;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (homeData.SelectedIndex != -1)
            {
                RassolCore.Home[homeData.SelectedIndex].checkData();
                RassolCore.Home[homeData.SelectedIndex].getSumLevelBeds();
                RassolCore.Home[homeData.SelectedIndex].getSumHomeBeds();
                homeData.Items.Refresh();
            }
        }

        private void headquartersData_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (headquartersData.SelectedIndex != -1)
                squadEditor.ItemsSource = RassolCore.Headquarters[headquartersData.SelectedIndex].Squads;
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            RassolCore.Headquarters[headquartersData.SelectedIndex].SetHeadquarterQuota();
            RassolCore.Headquarters[headquartersData.SelectedIndex].checkHead();
            headquartersData.Items.Refresh();
        }

        private void squadEditor_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            //RassolCore.Headquarters[headquartersData.SelectedIndex].Squads.Add(new Squad {Name = squadEditor.Sele} );
        }

        private void Calc_Click(object sender, RoutedEventArgs e)
        {
            RassolCore.RunCalc();
            RassolCore.ConsoleLog();
            RassolCore.WriteCSV();
            summaryDataGrid.ItemsSource = RassolCore.Home;
            GenerateTree();
        }

        private void summaryDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (summaryDataGrid.SelectedIndex != -1)
                roomDataGrid.ItemsSource = RassolCore.Home[summaryDataGrid.SelectedIndex].Room;
        }

        private void roomDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (summaryDataGrid.SelectedIndex != -1 && roomDataGrid.SelectedIndex != -1)
                squadDataGrid.ItemsSource = RassolCore.Home[summaryDataGrid.SelectedIndex].Room[roomDataGrid.SelectedIndex].CloseBeds;
        }

        private void GenerateTree()
        {
            TreeHome.Items.Clear();
            foreach(var home in RassolCore.Home)
            {
                var ItemHome = new TreeViewItem();
                ItemHome.Header = home.Name;
                foreach(var level in home.Levels)
                {
                    var ItemLevel = new TreeViewItem();
                    ItemLevel.Header = level.Key + " Этаж";
                    foreach(var room in  level.Value)
                    {
                        var ItemRoom = new TreeViewItem();
                        ItemRoom.Header = "Комната " + room.RoomNumber;
                        foreach(var squad in  room.CloseBeds)
                        {
                            for (int i = 0; i < squad.Value; ++i)
                            {
                                var ItemSquad = new TreeViewItem();
                                ItemSquad.Header = squad.Key;
                                ItemRoom.Items.Add(ItemSquad);
                            }
                        }
                        ItemLevel.Items.Add(ItemRoom);
                    }
                    ItemHome.Items.Add(ItemLevel);
                }
                TreeHome.Items.Add(ItemHome);
            }
            //ExpandAllItems(TreeHome);
    }


    private void TreeHome_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Запоминаем выбранный элемент для передачи при перетаскивании
        var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            
            if (treeViewItem != null)
        {
                treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
               
            e.Handled = true;
        }
    }

        private void TreeHome_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // При перетаскивании элемента обновляем внешний вид
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
                var level = GetTreeViewItemLevel(treeViewItem);
            if (treeViewItem != null && treeViewItem.Items.Count == 0 && level == 3)
            {
                    try
                    {
                        DragDrop.DoDragDrop(treeViewItem, treeViewItem, System.Windows.DragDropEffects.Move);
                        e.Handled = true;
                    }
                    catch
                    {
                        Console.WriteLine("хуйнч какая-то");
                    }
            }
        }
    }


    private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T ancestor)
            {
                return ancestor;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        while (current != null);
        return null;
    }

        private void TreeHome_Drop(object sender, System.Windows.DragEventArgs e)
        {
            // Перемещаем элемент в новый уровень
            var sourceItem = e.Data.GetData(typeof(TreeViewItem)) as TreeViewItem;
            var targetItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

            if (sourceItem != null && targetItem != null)
            {
                var sourceParent = sourceItem.Parent as ItemsControl;
                var targetParent = targetItem.Parent as ItemsControl;

                if (sourceParent != null && targetParent != null)
                {
                    sourceParent.Items.Remove(sourceItem);
                    targetParent.Items.Add(sourceItem);
                    if(targetParent.Items.Count > GetMaxCountRoom(targetParent))
                    {
                        Console.WriteLine($"{targetParent.Items.Count } also {GetMaxCountRoom(targetParent)}");
                        targetParent.Background = Brushes.Red;
                    }
                    else
                    {
                        targetParent.Background = Brushes.White;
                    }


                    if (sourceItem.Items.Count > GetMaxCountRoom(sourceParent))
                    {
                        sourceParent.Background = Brushes.Red;
                    }
                    else
                    {
                        sourceParent.Background = Brushes.White;
                    }
                }
            }
        }
        private int GetMaxCountRoom(ItemsControl currentParent)
        {
           /* List<string> headers = new List<string>();
            while(currentParent != null)
            {
                TreeViewItem itemControl = currentParent as TreeViewItem;
                headers.Add(itemControl.Header.ToString());
                if (itemControl.Parent is TreeViewItem parentItemControl)
                currentParent = (ItemsControl)itemControl.Parent;
            }*/
            List<string> headers = new List<string>();
            while (currentParent != null)
            {
                TreeViewItem itemControl = currentParent as TreeViewItem;
                headers.Add(itemControl.Header.ToString());
                if (itemControl.Parent is TreeViewItem parentItemControl)
                    currentParent = (ItemsControl)itemControl.Parent;
                else
                    currentParent = null; // Добавляем эту строку для выхода из цикла, когда достигнут корень дерева
            }
            

            headers.Reverse();
            var indexHome = 0;
            var indexLevel = Convert.ToInt32(new Regex(@"\d").Match(headers[1]).Value);
            var indexRoom = Convert.ToInt32(new Regex(@"\d").Match(headers[2]).Value);
            foreach (var h in RassolCore.Home)
            {
                if (h.Name == headers[0])
                {
                    var targetRoom = h.Levels[indexLevel];
                    foreach(var rm in targetRoom)
                    {
                        if(rm.RoomNumber == indexRoom)
                        {
                            return rm._countBeds;
                        }
                    }
                }

            }
            return -1;
           
        }
        private void ExpandAllItems(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                if (item is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsExpanded = true;
                    ExpandAllItems(treeViewItem);
                }
            }
        }
        private int GetTreeViewItemLevel(TreeViewItem item)
        {
            int level = 0;
            try
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);

                while (parent != null && !(parent is System.Windows.Controls.TreeView))
                {
                    if (parent is TreeViewItem)
                    {
                        level++;
                    }

                    parent = VisualTreeHelper.GetParent(parent);
                }

                return level;
            }
            catch
            {
                return 0;
            }
        }
    }
}
