using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;
using System.Drawing.Text;

namespace WindowsFormsApp9
{
    public partial class Form1 : Form
    {
        private List<Process> processes = null; //главный список, в котором будут храниться все процессы

        public Form1()
        {
            InitializeComponent();
        }

        private void GetProcesses() //метод для обновления и зачищения списка
        {
            processes.Clear(); //для очищения списка

            processes = Process.GetProcesses().ToList<Process>(); //Возвращаемое значение, ибо процесс - эррей, приводим к листу
        }

        private void RefreshProcessesList() //метод для взаимодействия с лист вью и заполнения его контентом
        {
            listView1.Items.Clear();

            double memSize = 0; //переменная для хранения памяти, которую занимают процессы

            foreach (Process p in processes) //тут будет вычисляться память каждого процесса
            {
                memSize = 0;

                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;

                memSize = (double)pc.NextValue() / (1000 * 1000); // измерение будет в мегабайтах

                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString() };

                listView1.Items.Add(new ListViewItem(row));

                pc.Close();
                pc.Dispose();
            }

            Text = "Запущено процессов: " + processes.Count.ToString();
        }

            private void RefreshProcessesList(List<Process> processes, string keyword)
            // + перегрузка
            //метод для взаимодействия с лист вью и заполнения его контентом
            {
            try
            {
                listView1.Items.Clear();

                double memSize = 0; //переменная для хранения памяти, которую занимают процессы

                foreach (Process p in processes) //тут будет вычисляться память каждого процесса
                {
                    if (p != null) //проверка, отсутствует ли процесс
                    {
                        memSize = 0;

                        PerformanceCounter pc = new PerformanceCounter();
                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = p.ProcessName;

                        memSize = (double)pc.NextValue() / (1000 * 1000); // измерение будет в мегабайтах

                        string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString() };

                        listView1.Items.Add(new ListViewItem(row));

                        pc.Close();
                        pc.Dispose();
                    }
                }

                Text = $"Запущено процессов: '{keyword}'" + processes.Count.ToString();
            }
            catch(Exception) { }
            }

            private void KillProcess(Process process) // метод для удаления процессов
            {
                process.Kill();
                process.WaitForExit();
            }

            private void KillProcessAndChildren(int pid)
            {
                if (pid == 0)
                {
                    return;
                }

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "Select * From Win32_Process Where ParentProcessId= " + pid);
                ManagementObjectCollection objectCollection = searcher.Get();
                
                foreach(ManagementObject obj in objectCollection) 
                {
                    KillProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
                }

                try
                {
                    Process p = Process.GetProcessById(pid);

                    p.Kill();
                    p.WaitForExit();
                }
                catch(ArgumentException)
                {

                }
            }

            private int GetParentProcessId(Process p) //нам нужно получать айдишник от родительского процесса
            {
                int parentID = 0;

                try
                {
                    ManagementObject managementObject = new ManagementObject("win32_process.handle= '" + p.Id + "'");

                    managementObject.Get();
                parentID = Convert.ToInt32(managementObject["ParentProcessId"]);
                }
                catch (Exception) { }
                return parentID;
            }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();

            GetProcesses();

            RefreshProcessesList();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();

            RefreshProcessesList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcess(processToKill);

                    GetProcesses();

                    RefreshProcessesList();
                }
            }
            catch(Exception) { }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(processToKill));
                    GetProcesses();
                    RefreshProcessesList();

                }
            }
            catch (Exception) { }
        }

        private void завершитьДеревоПроцессовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(processToKill));
                    GetProcesses();
                    RefreshProcessesList();

                }
            }
            catch (Exception) { }
        }

        private void запуститьЗадачуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Interaction.InputBox("Введите имя программы","Запуск новой задачи");

            try
            {
                Process.Start(path);
            }
            catch(Exception) { }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripLabel1_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();

            List<Process> filteredprocess = processes.Where((x) =>
            x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList<Process>();

            RefreshProcessesList(filteredprocess, toolStripTextBox1.Text);
        }
    } 
}
