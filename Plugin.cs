using System.IO;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClassIsland.Core.Controls.CommonDialog;
using System.Diagnostics;
using ClassIsland.Core;
using System.IO.Ports;
using System.ComponentModel;
using ClassIsland.Shared.Models.Profile;
using dotnetCampus.Ipc.CompilerServices.Attributes;
using ClassIsland.Shared.IPC;
using ClassIsland.Shared.IPC.Abstractions.Services;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Controls.LessonsControls;
using ClassIsland.Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LECIA
{
    /*
         /$$       /$$$$$$$$  /$$$$$$  /$$$$$$  /$$$$$$ 
        | $$      | $$_____/ /$$__  $$|_  $$_/ /$$__  $$
        | $$      | $$      | $$  \__/  | $$  | $$  \ $$
        | $$      | $$$$$   | $$        | $$  | $$$$$$$$
        | $$      | $$__/   | $$        | $$  | $$__  $$
        | $$      | $$      | $$    $$  | $$  | $$  | $$
        | $$$$$$$$| $$$$$$$$|  $$$$$$/ /$$$$$$| $$  | $$
        |________/|________/ \______/ |______/|__/  |__/
     */
    //L     E      C     I      A
    //LECIA Enable Class Island Anywhere

    [PluginEntrance]
    public class Plugin : PluginBase
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

        //结构体
        struct stSettings
        {
            public bool bAutoStart;
            public int iDataTarget;    // 0 -> COM    1 -> TCP
            public string sComPort;
            public int iBaundRate;
            public string sNetIP;
            public int iNetPort;
            public string sMainDataFormat;

            public stSettings(bool autoStart, int dataTarget, string comPort, int BaundRate, string netIP, int netPort, string mainDataFormat)
            {
                bAutoStart = autoStart;
                iDataTarget = dataTarget;
                sComPort = comPort;
                sNetIP = netIP;
                iBaundRate = BaundRate;
                iNetPort = netPort;
                sMainDataFormat = mainDataFormat;
            }
        }

        private static stSettings sSettings = new stSettings(false, 0, "COM1" ,115200, "", 0, "");
        private string sConfigPath = "";
        private static bool bKeepWorking = true;
        private static Thread thrMainLoop = null;
        private static SerialPort spSerialPort = null;
        public ILessonsService LessonsService { get; set; }
        public static ClassPlan cpCurrentClassPlan;
        public static Subject sNextClassSubject;
        public static TimeSpan tsTimeToNextPoint;
        public IProfileService psProfile { get; set; }
        public override void Initialize(HostBuilderContext context, IServiceCollection services)
        {
            sConfigPath = PluginConfigFolder;
            if(string.IsNullOrEmpty(sConfigPath))
            {
                throw new Exception("LECIA: Cannot Get Config Path.");
            }
            sConfigPath += "\\config.ini";
            Console.WriteLine($"LECIA: Config Path: {sConfigPath}");
            string sTemp = "";
            
            //注册退出
            var vAPP = AppBase.Current;
            vAPP.AppStopping += (sender, args) => {
                vONSHUTDOWN();
            };

            //初始化读取配置文件
            
            {
                //autostart      0=disable 1=enable
                sTemp = sINIREAD("mainconfig", "autostart", sConfigPath);
                if (sTemp != "null")
                {
                    if (sTemp == "0")
                    {
                        sSettings.bAutoStart = false;
                    }
                    else if (sTemp == "1")
                    {
                        sSettings.bAutoStart = true;
                    }
                    else
                    {
                        vINIWRITE("mainconfig", "autostart", "0", sConfigPath);
                    }
                }
                else
                {
                    vINIWRITE("mainconfig", "autostart", "0", sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> autostart: {sSettings.bAutoStart}");

                //datatarget
                sTemp = sINIREAD("mainconfig", "datatarget", sConfigPath);
                if (sTemp != "null")
                {
                    sSettings.iDataTarget = int.Parse(sTemp);
                }
                else
                {
                    vINIWRITE("mainconfig", "datatarget", "0", sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> datatarget: {sSettings.iDataTarget}");

                //comport
                sTemp = sINIREAD("mainconfig", "comport", sConfigPath);
                if (sTemp != "null")
                {
                    sSettings.sComPort = sTemp;
                }
                else
                {
                    vINIWRITE("mainconfig", "comport", "COM1", sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> comport: {sSettings.sComPort}");

                //BaundRate
                sTemp = sINIREAD("mainconfig", "baundrate", sConfigPath);
                if (sTemp != "null")
                {
                    sSettings.iBaundRate = int.Parse(sTemp);
                }
                else
                {
                    vINIWRITE("mainconfig", "baundrate", "0", sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> BaundRate: {sSettings.iBaundRate}");

                //maindataformat
                sTemp = sINIREAD("mainconfig", "maindataformat", sConfigPath);
                if (sTemp != "null")
                {
                    sSettings.sMainDataFormat = sTemp;
                }
                else
                {
                    vINIWRITE("mainconfig", "maindataformat", "no config", sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> maindataformat: {sSettings.sMainDataFormat}");
            }

            if (sSettings.bAutoStart == true)
            {
                thrMainLoop = new(vMAINLOOP);
                vAPP.AppStarted += (sender, args) => {
                    thrMainLoop.Start();
                };
            }
            return;
        }



        /// <summary>
        /// 初始化串口
        /// </summary>
        private static void vINITSERIALPORT()
        {
            try
            {
                if (spSerialPort != null)
                {
                    spSerialPort.Dispose();
                }

                spSerialPort = new SerialPort(
                    sSettings.sComPort,
                    sSettings.iBaundRate,
                    Parity.None,
                    8,
                    StopBits.One);

                spSerialPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine($"LECIA: Init Comport error: {e.Message}");
                Thread.Sleep(200);
            }
        }

        public string GetSubjectNameByGuid(string guid)
        {
            if (psProfile.Profile.Subjects == null)
                return null;

            if (psProfile.Profile.Subjects.TryGetValue(guid, out Subject subject))
                return subject?.Name;

            return null; // 或者 return $"未找到GUID: {guid}";
        }


        private void vMAINLOOP()
        {
            LessonsService = IAppHost.GetService<ILessonsService>();
            psProfile = IAppHost.GetService<IProfileService>();
            DateTime targetDate = DateTime.Today;
            string guid;
            string sMessage = "";
            //var subject1;
            while (bKeepWorking)
            {
                targetDate = DateTime.Today;
                try
                {
                    //获取数据
                    AppBase.Current.Dispatcher.Invoke(() =>
                    {
                        sNextClassSubject = LessonsService.NextClassSubject;
                        cpCurrentClassPlan = LessonsService.GetClassPlanByDate(targetDate, out guid);
                    });

                    if (cpCurrentClassPlan != null)
                    {
                        //pProfile.Subjects.TryGetValue("97d0bf3f-137f-4f8a-87d6-ff387063bbd3", out var subject1);
                        //sMessage = GetSubjectNameByGuid("97d0bf3f-137f-4f8a-87d6-ff387063bbd3");
                        //sMessage = sNextClassSubject.Name;

                    }

                    

                    if (spSerialPort == null || !spSerialPort.IsOpen)
                    {
                        vINITSERIALPORT();
                        continue;
                    }
                    if (spSerialPort != null && spSerialPort.IsOpen )
                    {
                        //发送
                        if (!string.IsNullOrEmpty(sMessage))
                        {
                            spSerialPort.Write($"{sMessage}\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //需要尝试重新初始化
                    Console.WriteLine($"LECIA: ERROR: {ex.Message}");
                }
            }
        }

        public static void vONSHUTDOWN()
        {
            if(spSerialPort != null)
            {
                if (spSerialPort.IsOpen)
                {
                    spSerialPort.Close();
                    Console.WriteLine($"LECIA: Closed the Serial");
                }
            }

            Console.WriteLine($"LECIA: Exiting");
            bKeepWorking = false;
            Thread.Sleep(500);
            return;
        }   
    }
}
