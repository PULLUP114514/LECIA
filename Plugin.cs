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
using System.Text;
using System.Windows.Forms;
using LECIA.view;


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


        
        //public static bool bKeepWorking { get; set; } = true;
        public static Thread thrMainLoop ;
        private static SerialPort spSerialPort = null;
        public ILessonsService lsLessonsService { get; set; }
        public static ClassPlan cpCurrentClassPlan;
        public static Subject sNextClassSubject;
        public static TimeSpan tsTimeToNextPoint;
        //public bool bThreadStarted { get; set; } = false;
        //public LECIA.view.settings Settings { get; set; } = new();

        public IProfileService psProfile { get; set; }
        public override void Initialize(HostBuilderContext context, IServiceCollection services)
        {
            GlobalVars.sConfigPath = PluginConfigFolder;
            if(string.IsNullOrEmpty(GlobalVars.sConfigPath))
            {
                throw new Exception("LECIA: Cannot Get Config Path.");
            }
            GlobalVars.sConfigPath += "\\config.ini";
            Console.WriteLine($"LECIA: Config Path: {GlobalVars.sConfigPath}");
            string sTemp = "";
            
            //注册退出
            var vAPP = AppBase.Current;
            vAPP.AppStopping += (sender, args) => {
                vONSHUTDOWN();
            };

            //初始化读取配置文件
            
            {
                //autostart      0=disable 1=enable
                sTemp = sINIREAD("mainconfig", "autostart", GlobalVars.sConfigPath);
                if (sTemp != "null")
                {
                    if (sTemp == "0")
                    {
                        GlobalVars.sSettings.bAutoStart = false;
                    }
                    else if (sTemp == "1")
                    {
                        GlobalVars.sSettings.bAutoStart = true;
                    }
                    else
                    {
                        vINIWRITE("mainconfig", "autostart", "0", GlobalVars.sConfigPath);
                    }
                }
                else
                {
                    vINIWRITE("mainconfig", "autostart", "0", GlobalVars.sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> autostart: {GlobalVars.sSettings.bAutoStart}");

                //datatarget
                sTemp = sINIREAD("mainconfig", "datatarget", GlobalVars.sConfigPath);
                if (sTemp != "null")
                {
                    GlobalVars.sSettings.iDataTarget = int.Parse(sTemp);
                }
                else
                {
                    vINIWRITE("mainconfig", "datatarget", "0", GlobalVars.sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> datatarget: {GlobalVars.sSettings.iDataTarget}");

                //comport
                sTemp = sINIREAD("mainconfig", "comport", GlobalVars.sConfigPath);
                if (sTemp != "null")
                {
                    GlobalVars.sSettings.sComPort = sTemp;
                }
                else
                {
                    vINIWRITE("mainconfig", "comport", "COM1", GlobalVars.sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> comport: {GlobalVars.sSettings.sComPort}");

                //BaundRate
                sTemp = sINIREAD("mainconfig", "baundrate", GlobalVars.sConfigPath);
                if (sTemp != "null")
                {
                    GlobalVars.sSettings.iBaundRate = int.Parse(sTemp);
                }
                else
                {
                    vINIWRITE("mainconfig", "baundrate", "0", GlobalVars.sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> BaundRate: {GlobalVars.sSettings.iBaundRate}");

                //maindataformat
                sTemp = sINIREAD("mainconfig", "maindataformat", GlobalVars.sConfigPath);
                if (sTemp != "null")
                {
                    GlobalVars.sSettings.sMainDataFormat = sTemp;
                }
                else
                {
                    vINIWRITE("mainconfig", "maindataformat", "no config", GlobalVars.sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> maindataformat: {GlobalVars.sSettings.sMainDataFormat}");

                //delay
                sTemp = sINIREAD("mainconfig", "delay", GlobalVars.sConfigPath);
                if (sTemp != "null")
                {
                    GlobalVars.sSettings.iDelay = int.Parse(sTemp);
                }
                else
                {
                    vINIWRITE("mainconfig", "delay", "0", GlobalVars.sConfigPath);
                }
                Console.WriteLine($"LECIA: Load Config -> delay: {GlobalVars.sSettings.iDelay}");
            }

            if (GlobalVars.sSettings.bAutoStart == true)
            {
                GlobalVars.bThreadStarted = true;
                thrMainLoop = new(vMAINLOOP);
                vAPP.AppStarted += (sender, args) => {
                    thrMainLoop.Start();
                };
            }
            services.AddSettingsPage<settings>();
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
                    GlobalVars.sSettings.sComPort,
                    GlobalVars.sSettings.iBaundRate,
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

        public string sGETSUBJECTNAMEBYGUID(string guid)
        {
            if (psProfile.Profile.Subjects == null)
                return null;

            if (psProfile.Profile.Subjects.TryGetValue(guid, out Subject subject))
                return subject?.Name;

            return null; 
        }



        public void vMAINLOOP()
        {
            lsLessonsService = IAppHost.GetService<ILessonsService>();
            psProfile = IAppHost.GetService<IProfileService>();
            DateTime dtTargetDate = DateTime.Today;
            string sGuid;
            string sMessage = "";
            while (GlobalVars.bKeepWorking)
            {
                dtTargetDate = DateTime.Today;

                //↓测试数据
                //sMessage = "NextPointTime: {NextPointTime}    ClassLeftTime: {ClassLeftTime}    " +
                //    "BreakingLeftTime: {BreakingLeftTime}    CurrentSubjectName:{CurrentSubjectName}    CurrentClassPlan:{CurrentClassPlan}";
                sMessage = GlobalVars.sSettings.sMainDataFormat;
                try
                {
                    //获取今日课表
                    AppBase.Current.Dispatcher.Invoke(() =>
                    {
                        sNextClassSubject = lsLessonsService.NextClassSubject;
                        cpCurrentClassPlan = lsLessonsService.GetClassPlanByDate(dtTargetDate, out sGuid);
                    });
                    if (cpCurrentClassPlan != null)
                    {
                        //pProfile.Subjects.TryGetValue("97d0bf3f-137f-4f8a-87d6-ff387063bbd3", out var subject1);
                        //sMessage = GetSubjectNameByGuid("97d0bf3f-137f-4f8a-87d6-ff387063bbd3");
                        //sMessage = sNextClassSubject.Name;

                        /*
                         * 以下为关键字： (大小写敏感的）
                         * {NextPointTime}              距离下个时间点的剩余时间（可能为Zero）
                         * {ClassLeftTime}              距离上课的时间（可能为Zero）
                         * {BreakingLeftTime}           距离下课的时间（可能为Zero）
                         * {CurrentSubjectName}         当前的课程名  （当无课程时为“无课程”）
                         * {CurrentClassPlan}           当前的课表
                         */


                        //获取下个时间点
                        if (lsLessonsService.OnBreakingTimeLeftTime != TimeSpan.Zero)
                        {
                            tsTimeToNextPoint = lsLessonsService.OnBreakingTimeLeftTime;
                        }
                        else if (lsLessonsService.OnClassLeftTime != TimeSpan.Zero)
                        {
                            tsTimeToNextPoint = lsLessonsService.OnClassLeftTime;
                        }
                        else
                        {
                            tsTimeToNextPoint = TimeSpan.Zero;
                        }
                        sMessage = sMessage.Replace("{NextPointTime}", tsTimeToNextPoint.ToString());


                        sMessage = sMessage.Replace("{ClassLeftTime}", lsLessonsService.OnClassLeftTime.ToString());


                        sMessage = sMessage.Replace("{BreakingLeftTime}", lsLessonsService.OnBreakingTimeLeftTime.ToString());

                        string sCurrentSubjectNameTemp = "";
                        if(sNextClassSubject.AttachedObjects.Count == 0)
                        {
                            sCurrentSubjectNameTemp = "无课程";
                        }
                        else
                        {
                            sCurrentSubjectNameTemp = sNextClassSubject.Name;
                        }
                        sMessage = sMessage.Replace("{CurrentSubjectName}", sCurrentSubjectNameTemp);

                        
                        string sClassPlan = "";
                        for (int i = 0; i < cpCurrentClassPlan.Classes.Count; i++)
                        {
                            sClassPlan += sGETSUBJECTNAMEBYGUID(cpCurrentClassPlan.Classes[i].SubjectId);
                            if(i < cpCurrentClassPlan.Classes.Count - 1)
                            {
                                sClassPlan += ",";
                            }
                        }
                        if (!string.IsNullOrEmpty(sClassPlan))
                        {
                            sMessage = sMessage.Replace("{CurrentClassPlan}", sClassPlan);
                        }



                        //重初始化
                        if (spSerialPort == null || !spSerialPort.IsOpen)
                        {
                            vINITSERIALPORT();
                            continue;
                        }


                        //发送
                        if (spSerialPort != null && spSerialPort.IsOpen && !string.IsNullOrEmpty(sMessage))
                        {
                            byte[] bData = Encoding.UTF8.GetBytes($"{sMessage}\n");
                            spSerialPort.Write(bData, 0, bData.Length);
                        }
                    }
                    else
                    {
                        //重初始化
                        if (spSerialPort == null || !spSerialPort.IsOpen)
                        {
                            vINITSERIALPORT();
                            continue;
                        }
                        byte[] bData = Encoding.UTF8.GetBytes("noclass\n");
                        spSerialPort.Write(bData, 0, bData.Length);
                    }

                    
                    Thread.Sleep(GlobalVars.sSettings.iDelay);
                }
                catch (Exception ex)
                {
                    //需要尝试重新初始化
                    Console.WriteLine($"LECIA: ERROR: {ex.ToString()}");
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
            GlobalVars.bKeepWorking = false;
            Thread.Sleep(500);
            return;
        }   
    }
}
