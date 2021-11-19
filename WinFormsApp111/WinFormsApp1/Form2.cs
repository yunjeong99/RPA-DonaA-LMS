using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace WinFormsApp1
{
    public partial class Form2 : Form
    {

        public Form2()
        {
            InitializeComponent();
        }

        ChromeDriver driver1;
        public Form2(Form1.listArr[] list, int count, ChromeDriver driver)//폼1에서 키워드를 넘겨받는 생성자
        {
            InitializeComponent();
            driver1 = driver;
            for (int i = 0; i < count; i++)
            {
                ListViewItem listItem = new ListViewItem(Convert.ToString(i + 1));
                listItem.SubItems.Add(list[i].Subject);
                listItem.SubItems.Add(list[i].Kind);
                listItem.SubItems.Add(list[i].Title);
                listItem.SubItems.Add(list[i].Date);
                listView1.Items.Add(listItem);
            }
            MessageBox.Show("프로그램 실행이 완료되었습니다.");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            driver1.Close();
            Application.Exit();
        }
    }
}