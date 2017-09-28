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
using System.Threading;             
using Microsoft.Win32;              



namespace multiThreadingWPF
{
    
    public partial class MainWindow : Window
    {
        private Worker worker1;
        private string choosedFileName;
        private string choosedBlackListName;
        private DataClassString2 nameOfFile;

        public MainWindow()
        {
            InitializeComponent();

            butStart.Click += butStart_Click;
            butStop.Click += butStop_Click;
            butOpen.Click += butOpen_Click;
            butOpenBlack.Click += butOpenBlackList_Click;
        }


        private void butStop_Click(object Sender, EventArgs e)
        {
            if (worker1 != null)
                worker1.Cancel();
                butStop.IsEnabled = false;
        }﻿


        private void butStart_Click(object Sender, EventArgs e)
        {
            butStop.IsEnabled = true;
            nameOfFile = new DataClassString2();

            worker1 = new Worker();
            worker1.ProcessChanged += worker_ProcessChanged;
            worker1.WorkCompleted += worker_WorkCompleted;
            worker1.ListBoxNotUniqEvent += worker_ListBoxNotUniqEvent;
            worker1.ListBoxNotValidEvent += worker_ListBoxNotValidEvent;
            worker1.ListBoxInBlackListEvent += worker_ListBoxInBlackListEvent;
            worker1.StageChanged += worker_StageChanged;

            if (choosedFileName == null || choosedBlackListName==null)
                MessageBox.Show("Select the input file and the blacklist");
            else
            {
                butStart.IsEnabled = false;

                ParameterizedThreadStart forSecondThread2 = worker1.ReadTheFile;
                forSecondThread2 += worker1.ReadTheBlackList;
                forSecondThread2 += worker1.ValidationTaxId;
                forSecondThread2 += worker1.SearchNotUniqTaxId;
                forSecondThread2 += worker1.SearchTaxIdInBlackList;
                forSecondThread2 += worker1.WriteResultsIntoFile;

                nameOfFile.N1 = choosedFileName;
                nameOfFile.N2 = choosedBlackListName;

                Thread thread1 = new Thread(forSecondThread2);
                thread1.Start(nameOfFile);
            }
        }


        private void butOpen_Click(object Sender, EventArgs e)
        {
            OpenFileDialog fileDialog1 = new OpenFileDialog();
            fileDialog1.Multiselect = false;                                    //  we can choose only one file at the moment
            fileDialog1.Filter = "excel files|*.xlsx;*.xls|all files|*.*";      //  filter for choosing the file
            fileDialog1.DefaultExt = ".xlsx";
            Nullable <bool> dialogOk = fileDialog1.ShowDialog();                 //      variable dialogOk can be NULL

            if (dialogOk==true)
            {
                choosedFileName = fileDialog1.FileName;                        //      the name of choosed file
                textBoxOpen.Text = choosedFileName;
            }
        }﻿


        private void butOpenBlackList_Click(object Sender, EventArgs e)
        {
            OpenFileDialog fileDialog2 = new OpenFileDialog();
            fileDialog2.Multiselect = false;                                    //  we can choose only one file at the moment
            fileDialog2.Filter = "excel files|*.xlsx;*.xls|all files|*.*";      //  filter for choosing the file
            fileDialog2.DefaultExt = ".xlsx";
            Nullable<bool> dialogOk = fileDialog2.ShowDialog();            //      variable dialogOk can be NULL

            if (dialogOk == true)
            {
                choosedBlackListName = fileDialog2.FileName;              //      the name of choosed file
                textBoxOpenBlack.Text = choosedBlackListName;
            }
        }﻿


        private void worker_WorkCompleted(bool cancelled)
        {
            Action action = () =>
            {
                string message = cancelled ? "процесс отменен" : "процесс завершен";
                textBoxStage.Text = cancelled ? "Cancelled" : "Finished";
                MessageBox.Show(message);
                butStart.IsEnabled = true;
                butStop.IsEnabled = true;
            };
            Dispatcher.Invoke(action);
        }


        private void worker_ProcessChanged(int progress)
        {
            Action action = () =>
            {
                progressBar1.Value = progress;
            };
            Dispatcher.Invoke(action);
        }


        private void worker_StageChanged(string stage)
        {
            Action action = () =>
            {
                textBoxStage.Text = stage;
            };
            Dispatcher.Invoke(action);
        }


        private void worker_ListBoxNotUniqEvent(List<DataClassString4> listForShow)
        {
            Action action = () =>
            {
                ListBoxNotUniq.ItemsSource = listForShow;
            };
            Dispatcher.Invoke(action);
        }


        private void worker_ListBoxNotValidEvent(List<DataClassString3> listForShow)
        {
            Action action = () =>
            {
                ListBoxNotValid.ItemsSource = listForShow;
            };
            Dispatcher.Invoke(action);
        }


        private void worker_ListBoxInBlackListEvent(List<DataClassString4> listForShow)
        {
            Action action = () =>
            {
                ListBoxInBlackList.ItemsSource = listForShow;
            };
            Dispatcher.Invoke(action);
        }

    }
}


