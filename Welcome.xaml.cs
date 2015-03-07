﻿using System.Windows;
using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Management;
using Grabacr07.KanColleViewer.Composition;
using System.Security.Cryptography;
using System.Text;
using Grabacr07.KanColleViewer;

namespace HoppoPlugin
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private async void CallMethodButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (retryCount < 1)
                {
                    if (!Directory.Exists(HoppoPluginSettings.KCVSettingsPath))
                        Directory.CreateDirectory(HoppoPluginSettings.KCVSettingsPath);
                    Pgb_Progress.Value = 80;
                    Btn_Retry.Visibility = Visibility.Hidden;
                    MainContent.Visibility = Visibility.Hidden;
                    LoadingGrid.Visibility = Visibility.Visible;
                    if (!Directory.Exists("HoppoPlugin"))
                    {
                        Directory.CreateDirectory("HoppoPlugin");
                        DirectoryInfo di = new DirectoryInfo("HoppoPlugin");
                        di.CreateSubdirectory("KanColleCache");
                    }
                    Stream s = App.GetResourceStream(new Uri("pack://application:,,,/HoppoPlugin;component/nekoError.png")).Stream;
                    Byte[] b = new Byte[s.Length];
                    s.Read(b, 0, b.Length);
                    File.WriteAllBytes(UniversalConstants.CurrentDirectory + @"\HoppoPlugin\nekoError.png", b);
                    Pgb_Progress.Value += await recordUsage();
                    if (Directory.Exists("Sounds")) { Directory.Delete("Sounds", true); }
                    Directory.CreateDirectory("Sounds");
                    DirectoryInfo d = new DirectoryInfo("Sounds");
                    d.CreateSubdirectory(NotifyType.Build.ToString());
                    d.CreateSubdirectory(NotifyType.Expedition.ToString());
                    d.CreateSubdirectory(NotifyType.Rejuvenated.ToString());
                    d.CreateSubdirectory(NotifyType.Repair.ToString());
                }
                else
                {
                    MessageBox.Show("多次错误，点击完成设置继续。（无影响）");
                    Btn_Finish.Visibility = Visibility.Visible;
                }
            }

            catch (Exception ex)
            {
                if (ex is WebException)
                {
                    Tbl_Introdution.Text = "错误！请重试";
                    MessageBox.Show("网络出现问题！请重试");
                    Btn_Retry.Visibility = Visibility.Visible;
                }
                else if (ex is UnauthorizedAccessException)
                {
                    Tbl_Introdution.Text = "错误！请重试";
                    MessageBox.Show("权限不足，无法在KCV目录创建文件夹，请使用管理员身份运行KCV再试！");
                    Btn_Retry.Visibility = Visibility.Visible;
                }
                else
                {
                    Tbl_Introdution.Text = "错误！请重试";
                    Btn_Retry.Visibility = Visibility.Visible;
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private async Task<double> recordUsage()
        {
            return await Task.Run(() =>
            {
                if (File.Exists(HoppoPluginSettings.UsageRecordPath))
                    return 20;
                var req = WebRequest.Create("http://provissy.com/visit.php");
                req.Timeout = 3000;
                req.Method = "GET";
                req.GetResponse();
                return 20;
            });
        }

        private void PgbValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(Pgb_Progress.Value == 100)
            {
                Btn_Finish.Visibility = Visibility.Visible;
            }
        }

        int retryCount = 0;

        private void Btn_Retry_Click(object sender, RoutedEventArgs e)
        {
            CallMethodButton_Click(null, null);
            Tbl_Introdution.Text = "正在重试...";
            retryCount++;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                (Process.GetProcessesByName("KanColleViewer")[0]).Kill();
            }
            catch { }
        }
    }
}
