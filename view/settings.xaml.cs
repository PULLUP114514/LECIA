using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using ClassIsland.Core.Controls.CommonDialog;
using MaterialDesignThemes.Wpf;
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
using static LECIA.Plugin;
using System.ComponentModel;
using System.Text.RegularExpressions;
using unvell.ReoGrid.Actions;
using MahApps.Metro.Controls;
using System.Windows.Forms;
using System.Windows.Threading;


namespace LECIA.view
{
    [SettingsPageInfo(
        "LECIA.Settings",
        "LECIA设置",
        PackIconKind.CogOutline,   // 未选中时设置页面图标
        PackIconKind.Cog,  // 选中时设置页面图标
        SettingsPageCategory.External  // 设置页面类别
    )]

    /// <summary>
    /// settings.xaml 的交互逻辑
    /// </summary>
    public partial class settings : SettingsPageBase
    {
        //ini RW
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);
        /// <summary>
        /// 写入INI
        /// 配置节点名称，键名，值，路径
        /// </summary>
        /// <param name="sSection">配置节点名称</param>
        /// <param name="sKey">键名</param>
        /// <param name="sValue">设置的键值</param>
        /// <param name="sPath">路径</param>
        private void vINIWRITE(string sSection, string sKey, string sValue, string sPath)
        {
            WritePrivateProfileString(sSection, sKey, sValue, sPath);
        }

        /// <summary>
        /// 读取INI
        /// 配置节点名称，键名，路径
        /// 每次从ini中读取1024字节
        /// </summary>
        /// <param name="sSection">配置节点名称</param>
        /// <param name="sKey">键名</param>
        /// <param name="sPath">路径</param>
        /// <returns></returns>
        private string sINIREAD(string sSection, string sKey, string sPath)
        {
            System.Text.StringBuilder temp = new System.Text.StringBuilder(1024);
            try
            {
                GetPrivateProfileString(sSection, sKey, "null", temp, 255, sPath);
            }
            catch (Exception ex)
            {
                return "null";
            }
            return temp.ToString();
        }

        /// <summary>
        /// 检查字符串是否为COM*
        /// </summary>
        /// <param name="sInput">欲检测string</param>
        /// <returns></returns>
        private bool bCHECKCOMSTRING(string sInput)
        {
            if (string.IsNullOrEmpty(sInput))
                return false;

            return Regex.IsMatch(sInput, @"^COM\d+$", RegexOptions.IgnoreCase);
        }

        private int iCHECKRETURNINT(string sInputInt)
        {
            if (Regex.IsMatch(sInputInt, @"^[0-9]\d*$"))
            {
                return int.Parse(sInputInt);
            }
            else
            {
                return -1;
            }
        }

        private bool bFULLCHECK()
        {
            try
            {
                int iDataTarget = iCHECKRETURNINT(
                    ((System.Windows.Controls.TextBox)SETTING_DATATARGET.Switcher).Text
                    );
                if (iDataTarget == 0)
                {
                    //校验COM
                    if (!bCHECKCOMSTRING(
                        ((System.Windows.Controls.TextBox)SETTING_COMPORT.Switcher).Text
                    ))
                    {
                        ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo("COM口格式错误!");
                        return false;
                    }
                    //校验波特率
                    int iBaundrate = iCHECKRETURNINT(
                        ((System.Windows.Controls.TextBox)SETTING_BAUNDRATE.Switcher).Text
                        );
                    if (iBaundrate == -1)
                    {
                        ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo("波特率错误，请输入正整数!");
                        return false;
                    }
                }
                else if (iDataTarget == 1)
                {
                    //校验UDP
                    //校验IP
                    string sIP = ((System.Windows.Controls.TextBox)SETTING_UDPIP.Switcher).Text;
                    if (string.IsNullOrEmpty(sIP))
                    {
                        ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo("IP错误!");

                        return false;
                    }
                    //校验端口
                    int iPort = iCHECKRETURNINT(
                        ((System.Windows.Controls.TextBox)SETTING_UDPPORT.Switcher).Text
                        );
                    if(iPort == -1 || iPort > 65535 || iPort <= 0)
                    {
                        ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo("UDP端口错误");
                        return false;
                    }
                }
                else
                {
                    ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo("不合法的数据目标");
                    return false;
                }


                //校验Delay
                int iTempDelay = iCHECKRETURNINT(
                    ((System.Windows.Controls.TextBox)SETTING_DELAYTIME.Switcher).Text
                    );
                if (iTempDelay == -1)
                {
                    ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo("延时错误，请输入正整数!");
                    return false;
                }


                //数据格式
                if (string.IsNullOrEmpty(
                    ((System.Windows.Controls.TextBox)SETTING_DATAFORMAT.Switcher).Text
                    ))
                {
                    ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo("数据格式为空!");
                    return false;
                }


                return true;
            }
            catch(Exception ex)
            {
                ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo($"发生了错误\n{ex.Message}");
                return false;   
            }
        }

        public bool bKeepWorking { get; set; }
        private DispatcherTimer dtThreadMessageTimer;

        public settings()
        {
            InitializeComponent();
            if (GlobalVars.bThreadStarted)
            {
                MENU_STARTSW.Header = "立即停止";
            }

            DataContext = this; // 别漏了

            //set
            SETTING_AUTOSTARTSW.IsOn = GlobalVars.sSettings.bAutoStart;
            ((System.Windows.Controls.TextBox)SETTING_COMPORT.Switcher).Text = GlobalVars.sSettings.sComPort;
            ((System.Windows.Controls.TextBox)SETTING_BAUNDRATE.Switcher).Text = GlobalVars.sSettings.iBaundRate.ToString();
            ((System.Windows.Controls.TextBox)SETTING_DATAFORMAT.Switcher).Text = GlobalVars.sSettings.sMainDataFormat;
            ((System.Windows.Controls.TextBox)SETTING_DELAYTIME.Switcher).Text = GlobalVars.sSettings.iDelay.ToString();
            ((System.Windows.Controls.TextBox)SETTING_DATATARGET.Switcher).Text = GlobalVars.sSettings.iDataTarget.ToString();
            ((System.Windows.Controls.TextBox)SETTING_UDPIP.Switcher).Text = GlobalVars.sSettings.sUDPNetIP;
            ((System.Windows.Controls.TextBox)SETTING_UDPPORT.Switcher).Text = GlobalVars.sSettings.iUDPNetPort.ToString();


        }

        private void vTIMERTICK(object sender, System.EventArgs e)
        {
            if(TEXTBOX_THREADMESSAGE.Text != GlobalVars.sThreadMessage)
            {
                TEXTBOX_THREADMESSAGE.Text = GlobalVars.sThreadMessage;
            }
        }

        private void vSETDATA()
        {
            //set
            GlobalVars.sSettings.bAutoStart = SETTING_AUTOSTARTSW.IsOn;
            GlobalVars.sSettings.iDataTarget = int.Parse(((System.Windows.Controls.TextBox)SETTING_DATATARGET.Switcher).Text);
            if (GlobalVars.sSettings.iDataTarget == 0)
            {
                GlobalVars.sSettings.sComPort = ((System.Windows.Controls.TextBox)SETTING_COMPORT.Switcher).Text;
                GlobalVars.sSettings.iBaundRate = int.Parse(((System.Windows.Controls.TextBox)SETTING_BAUNDRATE.Switcher).Text);
            }
            else if (GlobalVars.sSettings.iDataTarget == 1)
            {
                GlobalVars.sSettings.sUDPNetIP = ((System.Windows.Controls.TextBox)SETTING_UDPIP.Switcher).Text;
                GlobalVars.sSettings.iUDPNetPort = int.Parse(((System.Windows.Controls.TextBox)SETTING_UDPPORT.Switcher).Text);
            }
            else
            {
                return;
            }

            GlobalVars.sSettings.sMainDataFormat = ((System.Windows.Controls.TextBox)SETTING_DATAFORMAT.Switcher).Text;
            GlobalVars.sSettings.iDelay = int.Parse(((System.Windows.Controls.TextBox)SETTING_DELAYTIME.Switcher).Text);
        }

        private void MENU_SAVE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bFULLCHECK())
                {

                    {
                        vSETDATA();

                        //write
                        {
                            if (GlobalVars.sSettings.bAutoStart == true)
                            {
                                vINIWRITE("mainconfig", "AutoStart", "1", GlobalVars.sConfigPath);
                            }
                            else
                            {
                                vINIWRITE("mainconfig", "AutoStart", "0", GlobalVars.sConfigPath);
                            }
                            vINIWRITE("mainconfig", "datatarget", GlobalVars.sSettings.iDataTarget.ToString(), GlobalVars.sConfigPath);
                            vINIWRITE("mainconfig", "comport", GlobalVars.sSettings.sComPort, GlobalVars.sConfigPath);
                            vINIWRITE("mainconfig", "baundrate", GlobalVars.sSettings.iBaundRate.ToString(), GlobalVars.sConfigPath);
                            vINIWRITE("mainconfig", "maindataformat", GlobalVars.sSettings.sMainDataFormat, GlobalVars.sConfigPath);
                            vINIWRITE("mainconfig", "delay", GlobalVars.sSettings.iDelay.ToString(), GlobalVars.sConfigPath);
                            vINIWRITE("mainconfig", "udpnetport", GlobalVars.sSettings.iUDPNetPort.ToString(), GlobalVars.sConfigPath);
                            vINIWRITE("mainconfig", "udpnetIP", GlobalVars.sSettings.sUDPNetIP, GlobalVars.sConfigPath);
                            ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo("设置已保存!");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo($"发生了错误\n{ex.Message}");
                return;
            }
        }

        private void MENU_STARTSW_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalVars.bThreadStarted)
            {
                GlobalVars.bKeepWorking = false;
                if (dtThreadMessageTimer != null)
                {
                    dtThreadMessageTimer.Stop();
                }
                MENU_STARTSW.Header = "立即启动";
            }
            else
            {
                try
                {
                    if (bFULLCHECK())
                    {
                        vSETDATA();
                        GlobalVars.bKeepWorking = true;
                        Task.Run(() =>
                        {
                            var plugin = new LECIA.Plugin();
                            plugin.vMAINLOOP();
                        });
                        dtThreadMessageTimer = new DispatcherTimer();
                        dtThreadMessageTimer.Interval = System.TimeSpan.FromMilliseconds(20);
                        dtThreadMessageTimer.Tick += vTIMERTICK;
                        dtThreadMessageTimer.Start();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ClassIsland.Core.Controls.CommonDialog.CommonDialog.ShowInfo($"启动时发生了错误\n{ex.Message}");
                    return;
                }
                
                MENU_STARTSW.Header = "立即停止";
            }
            GlobalVars.bThreadStarted = !GlobalVars.bThreadStarted;
        }

        private void vTEXTBOX_FORMAT_CHANGED(object sender, TextChangedEventArgs e)
        {
            string input = ((System.Windows.Controls.TextBox)SETTING_DATAFORMAT.Switcher).Text;
            string result = sCHANGESHOWPREVIEW(input); // 调用你定义的方法
            TEXTBOX_FORMATSPREVIEW.Text = result;
        }

        private string sCHANGESHOWPREVIEW(string sInput)
        {
            sInput = sInput.Replace("{NextPointTime}", "12:34:56.8173949");
            sInput = sInput.Replace("{ClassLeftTime}", "02:11:44.8173949");
            sInput = sInput.Replace("{BreakingLeftTime}", "11:11:22.8122249");
            sInput = sInput.Replace("{CurrentSubjectName}", "通用技术");
            sInput = sInput.Replace("{CurrentClassPlan}", "自习,自习" +
                ",数学,通用技术,周测,班会,美术,政治");
            return sInput;
        }
    }
}
