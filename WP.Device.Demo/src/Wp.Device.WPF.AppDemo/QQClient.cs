using RemotingVcode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UtilityLibrary;
using System.Windows.Automation;

namespace QQClientHelper
{
    public class QQClient
    {
        private static object syncLock = new object();
        private Process loginProcess;
        private Process mainProcess;
        private SynchronizationContext syncContext;
        private System.Threading.Timer timer = null;
        private string statusMsg = string.Empty;
        private int onlineMinute = 0;
        private Random random = new Random();
        private QQLogin.WebLogin qqQzone = null;

        public string Username { get; private set; }
        public string Password { get; private set; }
        public string YzmCode { get; set; }
        public byte[] YzmBytes { get; set; }
        public string VcodeWorkder { get; set; }

        public IntPtr MainHandle { get; private set; }
        public string StatusMsg
        {
            get { return this.statusMsg; }
            private set
            {
                if (this.statusMsg != value)
                {
                    this.statusMsg = value;
                    OnStatusChanged();
                }
            }
        }

        public bool HasExited { get { return this.loginProcess == null && this.mainProcess == null; } }

        public bool DelProfileOnExit { get; set; }

        public bool DisableOnLineCount { get; set; }

        public bool LooseLogonCheck { get; set; } //登录成功 宽松判断


        public int OnLineMinute
        {
            get { return this.onlineMinute; }
            private set
            {
                if (this.onlineMinute != value)
                {
                    this.onlineMinute = value;
                    OnStatusChanged();
                }
            }
        }
        public int MaxOnLineMinute { get; set; }

        public bool MinimunOnLogon { get; set; }


        public QQClient(string username, string password)
        {
            this.MinimunOnLogon = true;
            this.DelProfileOnExit = false;
            this.Username = username;
            this.Password = password;
            this.syncContext = SynchronizationContext.Current;

            if (this.Username != null)
                this.Username = this.Username.Replace("\r", "").Replace("\n", "");
            if (this.Password != null)
                this.Password = this.Password.Replace("\r", "").Replace("\n", "");

            //内部包含一个qq空间的协议
            this.qqQzone = new QQLogin.WebLogin(this.Username, this.Password);
            this.qqQzone.VcodeRequired += (s, e) =>
            {
                OnVcodeRequired(e);
            };
            this.qqQzone.VcodeErrReport += (s, e) =>
            {
                OnVcodeErrReport(e);
            };
        }

        public event EventHandler<VcodeEventArgs> VcodeRequired;

        public event EventHandler<VcodeEventArgs> VcodeErrReport;

        public event EventHandler StatusChanged;

        public event EventHandler ClientLogon;

        public event EventHandler ClientExited;

        public event EventHandler LoginStarted;


        private void OnVcodeRequired(VcodeEventArgs e)
        {
            var handle = VcodeRequired;
            if (handle != null)
            {
                handle(this, e);
            }

            //if (!e.Handled)//if not handled,use local manual way
            //{
            //    CommonVcode vcode = CommonVcode.CreateVcode(VcodePlatformType.Manual);
            //    if (vcode != null)
            //    {
            //        vcode.GetVcode(e);
            //    }
            //}
        }

        private void OnVcodeErrReport(VcodeEventArgs e)
        {
            var handle = VcodeErrReport;
            if (handle != null)
            {
                handle(this, e);
            }
        }

        private void OnLoginStarted()
        {
            var handler = LoginStarted;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void OnStatusChanged()
        {
            var handler = StatusChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public ErrType OpenFriendMgrWindow(out IntPtr handle)
        {
            //if (this.process == null)
            //    this.process = Process.GetProcessesByName("QQ")[0];
            //this.MainHandle = (IntPtr)132928;

            handle = IntPtr.Zero;

            if (this.MainHandle == IntPtr.Zero)
            {
                StatusMsg = "找不到主界面窗口，可能未登录或已退出";
                return ErrType.OperationFailed;
            }

            Utility.ShowWindow(this.MainHandle, 9);
            Utility.ForceShowWindow(this.MainHandle, true);

            var rect = new Rectangle();
            Utility.GetWindowRect(this.MainHandle, ref rect);
            var point = new System.Drawing.Point();
            point.X = rect.X + 25;
            point.Y = rect.Height - 20;

            Utility.MouseClick(point.X, point.Y);
            System.Threading.Thread.Sleep(600);

            //打开主菜单
            var main = Utility.GetAutomationElementFromHandle(this.MainHandle);
            if (main == null)
            {
                StatusMsg = "打开好友管理器失败，无法打开主菜单";
                return ErrType.OperationFailed;
            }

            var tool = Utility.GetAutoElementByPath(main, new string[] { "TXMenuWindow", "工具" });
            if (tool == null)
            {
                StatusMsg = "打开好友管理器失败，找不到工具菜单项";
                return ErrType.OperationFailed;
            }
            var bound = tool.Current.BoundingRectangle;
            Utility.MouseClick((int)(bound.X + 20), (int)(bound.Y + bound.Height / 2));
            System.Threading.Thread.Sleep(600);

            var fri = Utility.GetAutoElementByPath(main, new string[] { "TXMenuWindow", "TXMenuWindow", "好友管理器" });
            if (fri == null)
            {
                StatusMsg = "打开好友管理器失败，找不到好友管理器项";
                return ErrType.OperationFailed;
            }
            bound = fri.Current.BoundingRectangle;
            Utility.MouseClick((int)(bound.X + 20), (int)(bound.Y + bound.Height / 2));
            Utility.ForceShowWindow(this.MainHandle, false);

            Utility.Win32WindowInfo friHwnd = null;
            int count = 4;



            do
            {
                System.Threading.Thread.Sleep(1000);
                var list = Utility.FindWindows("好友管理器", this.mainProcess.Id, "TXGuiFoundation");
                if (list.Count > 0)
                {
                    friHwnd = list[0];
                    break;
                }
            } while (count-- > 0 && friHwnd == null);

            if (friHwnd == null)
            {
                StatusMsg = "打开好友管理器失败，等待超时";
                return ErrType.OperationFailed;
            }
            handle = friHwnd.Handle;
            StatusMsg = "打开好友管理器成功";
            return ErrType.Success;
        }

        public void Exit(bool notify = true)
        {
            if (HasExited)
            {
                return;
            }
            //FileLog.WriteLog(string.Format("{0},Before exit...", this.Username));
            //if (this.MainHandle != IntPtr.Zero)
            //{
            //    Utility.ForceShowWindow(this.MainHandle, true);
            //    System.Threading.Thread.Sleep(1000);

            //    var mainfrm = Utility.GetAutomationElementFromHandle(this.MainHandle);
            //    if(mainfrm != null)
            //    {
            //        var bound = mainfrm.Current.BoundingRectangle;
            //        var p = Utility.RandomMoveMouse(this.MainHandle, new Rectangle((int)(bound.Right - bound.Left), (int)bound.Top + 10, 5, 5));
            //        Utility.MouseClick(p.X, p.Y);
            //        int count = 6;
            //        while (!this.mainProcess.HasExited && count-- > 0)
            //        {
            //            System.Threading.Thread.Sleep(1000);
            //        }
            //    }
            //}

            ClientHelper.ExitProcess(this.mainProcess);
            if (notify)
                this.StatusMsg = "客户端退出！";
            this.mainProcess = null;
            this.loginProcess = null;
            if (this.timer != null)
            {
                //shutdown
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            this.timer = null;
            if (this.DelProfileOnExit)
            {
                //FileLog.WriteLog(string.Format("{0},Begin del profile folder...", this.Username));
                ClientHelper.DelQQProfileFolder(this.Username);
            }

            //FileLog.WriteLog(string.Format("{0},Begin invoke ClientExited", this.Username));
            var handler = ClientExited;
            if (handler != null)
                handler(this, new EventArgs());
        }

        /// <summary>
        /// 可以调用此方法在正式登录客户端前先登录qq空间 检查是否被封了已经
        /// </summary>
        /// <returns>被封返回true</returns>
        public bool PreCheckProtect(out string msg)
        {

            var ret = this.qqQzone.Login(out msg, redirect: false);
            if (ret == 3)
            {
                StatusMsg = "空间检测结果：" + msg;
                return true;
            }
            return false;
        }

        public ErrType Login(string qqpath, int timeout = 60)
        {
            lock (syncLock)
            {
                ErrType err;

                ClientHelper.KillDirtyWindows();
                ClientHelper.KillDirtyProcesses();

                List<Utility.Win32WindowInfo> windowList;
                VcodeEventArgs lastVcodeArgs = null;
                VcodeEventArgs vcodeArgs = null;
                List<Tuple<string, string, string>> lastLogonList = null;

                MaxOnLineMinute = 0;
                onlineMinute = 0;

                try
                {
                    //FileLog.WriteLog(string.Format("{0},LoginStart...", this.Username));
                    if (!this.HasExited)
                        Exit();
                    StatusMsg = "登录中";
                    //FileLog.WriteLog(string.Format("{0},OnLoginStarted", this.Username));
                    OnLoginStarted();
                    string exePath = qqpath;
                    if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                    {
                        this.StatusMsg = "QQ路径错误！";
                        return ErrType.ExePathErr;
                    }

                    if (string.IsNullOrEmpty(this.Username) || string.IsNullOrEmpty(this.Password))
                    {
                        err = ErrType.AccountErr;
                        StatusMsg = "账号密码格式错误！";
                        return err;
                    }

                    ////capture current login list
                    //lastLogonList = ClientHelper.GetLogonYYList();

                    //FileLog.WriteLog(string.Format("{0},Process start...", this.Username));
                    var capturedPros = Process.GetProcessesByName("QQ").ToList();
                    Process.Start(exePath);

                    #region StartApp
                    IntPtr loginHandle = IntPtr.Zero;
                    int time = 0;
                    int count = 0;
                    while ((loginProcess == null || mainProcess == null) && loginHandle == IntPtr.Zero && time < timeout)
                    {
                        System.Threading.Thread.Sleep(3000);
                        time += 3;

                        var updatedPros = Process.GetProcessesByName("QQ").ToList().FindAll(p => capturedPros.Find(c => c.Id == p.Id) == null);
                        if (updatedPros.Count != 2)
                        {
                            continue;
                        }

                        windowList = Utility.EnumWindows();
                        for (int i = 0; i < updatedPros.Count; i++)
                        {
                            loginHandle = ClientHelper.GetMainWindow(updatedPros[i].Id, windowList);
                            if (loginHandle != IntPtr.Zero)
                            {
                                loginProcess = updatedPros[i];
                                mainProcess = updatedPros[1 - i];
                                break;
                            }
                        }

                        if (loginHandle != IntPtr.Zero)
                            break;
                    }

                    #region Timeout
                    if (time >= timeout)
                    {
                        if ((loginProcess != null || mainProcess != null))
                        {
                            try
                            {
                                loginProcess.Kill();
                            }
                            catch { };
                            try
                            {
                                mainProcess.Kill();
                            }
                            catch { };
                        }
                        StatusMsg = "启动QQ超时！";
                        return ErrType.OperationTimeout;
                    }

                    if (loginHandle == IntPtr.Zero || loginProcess == null || mainProcess == null)
                    {
                        StatusMsg = "检测QQ进程失败！";
                        return ErrType.OperationTimeout;
                    }

                    #endregion

                    //FileLog.WriteLog(string.Format("{0},Process started", this.Username));

                    #endregion
                    bool inputErr = false;
                    var func = new SendOrPostCallback((o) =>
                        {
                            count = 5;
                            string inputnum = null;
                            do
                            {
                                Utility.ForceShowWindow(loginHandle, true);
                                Utility.PostMouseEvent(loginHandle, new System.Windows.Point(200, 275), Utility.MouseEvent.LeftClick);
                                if (count != 5)
                                {
                                    //System.Threading.Thread.Sleep(500);
                                    Utility.PostMouseEvent(loginHandle, new System.Windows.Point(200, 275), Utility.MouseEvent.LeftClick);
                                    Utility.PostMouseEvent(loginHandle, new System.Windows.Point(200, 275), Utility.MouseEvent.LeftClick);
                                }

                                System.Threading.Thread.Sleep(500);

                                Utility.ForceShowWindow(loginHandle, true);
                                for (int i = 0; i < this.Username.Length; i++)
                                {
                                    Utility.Keybd_Sendkeys(this.Username.Substring(i, 1));
                                    System.Threading.Thread.Sleep(random.Next(100, 300));
                                }
                                //Utility.PostText(loginHandle, this.Username);
                                System.Threading.Thread.Sleep(500);

                                Utility.ForceShowWindow(loginHandle, true);
                                Utility.SendKeysForeGround((char)9);
                                System.Threading.Thread.Sleep(500);

                                inputnum = ClientHelper.GetInputedQQNumber(loginHandle);
                                System.Diagnostics.Debug.WriteLine("输入的QQ号结果：" + inputnum);
                            } while (inputnum != null && inputnum != this.Username && count-- > 0 && Utility.SetForegroundWindow(loginHandle) >= 0);

                            for (int i = 0; i < this.Password.Length; i++)
                            {
                                Utility.Keybd_Sendkeys(this.Password.Substring(i, 1));
                                System.Threading.Thread.Sleep(random.Next(100, 300));
                            }
                            System.Threading.Thread.Sleep(500);

                            //var forehandle = Utility.GetForegroundWindow();
                            //if (forehandle != IntPtr.Zero && forehandle != loginHandle)
                            //{
                            //    int foreprocess = 0;
                            //    Utility.GetWindowThreadProcessId(forehandle, out foreprocess);
                            //    //这样的情况 可能输入过程已经出错
                            //    if (foreprocess != 0 && foreprocess != this.process.Id)
                            //    {
                            //        inputErr = true;
                            //        return;
                            //    }
                            //}

                            //Utility.PostMouseEvent(loginHandle, new System.Windows.Point(253, 360), Utility.MouseEvent.LeftClick);
                            //Utility.Keybd_Sendkeys("{enter}");
                            Utility.Keybd_Sendkey(13);
                        });

                    if (this.syncContext != null && SynchronizationContext.Current == null)
                    {
                        this.syncContext.Send(func, null);
                    }
                    else
                    {
                        func(null);
                    }

                    if (inputErr)
                    {
                        StatusMsg = "输入过程可能出错，需要重新登录";
                        err = ErrType.PasswordErr;
                        ClientHelper.ExitProcess(loginProcess);
                        return err;
                    }

                    //IntPtr mainHandle = loginHandle;
                    time = 0;
                    while (loginProcess != null && !loginProcess.HasExited && time < timeout) // && mainHandle == loginHandle)
                    {


                        System.Threading.Thread.Sleep(2000);
                        time += 2;
                        windowList = Utility.EnumWindows();
                        //mainHandle = ClientHelper.GetMainWindow(loginProcess.Id, windowList);

                        ClientHelper.KillDirtyWindows(windowList);
                        ClientHelper.KillDirtyProcesses();

                        #region login success 不可能会进到这里，因为QQ登录后会另外启动一个线程
                        //if (mainHandle != IntPtr.Zero && mainHandle != loginHandle)
                        //{
                        //    err = ErrType.Success;
                        //    StatusMsg = "登录成功！";
                        //    return err;
                        //}

                        #endregion

                        #region 账号密码错误等错误 退出
                        string tmsg = null;
                        if (ClientHelper.CatchAccountErr(loginHandle, out tmsg))
                        {
                            StatusMsg = tmsg;
                            if (StatusMsg != null && StatusMsg.IndexOf("您登录的QQ数量已经达到了上限") >= 0)
                            {
                                err = ErrType.ReachMaxLogin;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("密码不正确") >= 0)
                            {
                                err = ErrType.PasswordErr;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("不能重复登录") >= 0)
                            {
                                err = ErrType.AccountErr;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("帐号不存在") >= 0)
                            {
                                err = ErrType.AccountErr;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("您的QQ暂时无法登录") >= 0)
                            {
                                err = ErrType.AccountProtect;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("暂时无法登录QQ，请稍后再试") >= 0)
                            {
                                err = ErrType.AccountProtect;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("登录超时") >= 0)
                            {
                                err = ErrType.OperationTimeout;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("请您输入密码后再登录") >= 0)
                            {
                                err = ErrType.PasswordErr;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("保护模式") >= 0)
                            {
                                err = ErrType.AccountProtect;
                            }
                            else if (StatusMsg != null && StatusMsg.IndexOf("当前设备需进行身份验证") >= 0)
                            {
                                err = ErrType.LoginDeviceVeify;
                            }
                            else
                            {
                                err = ErrType.LoginErr;
                            }
                            //Utility.CloseWindow(tmphandle);
                            ClientHelper.ExitProcess(loginProcess);
                            return err;
                        }
                        #endregion

                        #region 验证码
                        if (ClientHelper.CatchCaptchaDialog(loginHandle, out vcodeArgs))
                        {
                            if (vcodeArgs.Bytes == null)
                            {
                                RunLog.WriteLog(string.Format("QQ【{0}】抓取验证码失败！", this.Username));
                                continue;
                            }

                            //verifying
                            if (lastVcodeArgs != null && Utility.IsSameBitmap(lastVcodeArgs.Bytes as Bitmap, vcodeArgs.Bytes as Bitmap))
                                continue;

                            //last time vcode result error, report
                            if (lastVcodeArgs != null && !Utility.IsSameBitmap(lastVcodeArgs.Bytes as Bitmap, vcodeArgs.Bytes as Bitmap))
                            {
                                OnVcodeErrReport(lastVcodeArgs);
                            }

                            //vcode new
                            OnVcodeRequired(vcodeArgs);

                            if (!vcodeArgs.Handled)
                            {
                                err = ErrType.VcodeErr;
                                StatusMsg = "验证码打码失败！" + vcodeArgs.Msg ?? string.Empty;
                                //Utility.CloseWindow(tmphandle);
                                ClientHelper.ExitProcess(loginProcess);
                                return err;
                            }

                            //input vcode and wait check
                            if (lastVcodeArgs != null && lastVcodeArgs.Bytes != null)
                            {
                                lastVcodeArgs.Bytes.Dispose();
                            }
                            lastVcodeArgs = vcodeArgs;
                            time /= 2;



                            func = new SendOrPostCallback((o) =>
                            {
                                Utility.ForceShowWindow(loginHandle, true);
                                Utility.PostMouseEvent(loginHandle, new System.Windows.Point(194, 177), Utility.MouseEvent.LeftClick);
                                System.Threading.Thread.Sleep(500);

                                string code = vcodeArgs.Result ?? "null";

                                for (int i = 0; i < code.Length; i++)
                                {
                                    Utility.Keybd_Sendkeys(code.Substring(i, 1));
                                    System.Threading.Thread.Sleep(random.Next(100, 300));
                                }
                                System.Threading.Thread.Sleep(500);

                                //Utility.ForceShowWindow(mainHandle, false);
                                Utility.PostMouseEvent(loginHandle, new System.Windows.Point(325, 380), Utility.MouseEvent.LeftClick);
                            });

                            if (this.syncContext != null && SynchronizationContext.Current == null)
                            {
                                this.syncContext.Send(func, null);
                            }
                            else
                            {
                                func(null);
                            }
                        }
                        #endregion
                    }

                    #region 登录成功

                    count = 30;
                    bool succ = false;
                    do
                    {
                        succ = ClientHelper.GetLogonQQNickDic().ContainsKey(this.Username) || ClientHelper.GetLogonQQProcessDic().ContainsKey(this.Username);
                        if (!succ)
                            System.Threading.Thread.Sleep(1000);
                    } while (count-- > 0 && !succ && !mainProcess.HasExited);

                    if (succ)
                    {
                        //count = 30;
                        //succ = false;
                        //do
                        //{
                        //    succ = ClientHelper.GetLogonQQProcessDic().ContainsKey(this.Username);
                        //    if (!succ)
                        //        System.Threading.Thread.Sleep(1000);
                        //} while (count-- > 0 && !succ);

                        //if (succ)
                        //{
                        //    var p = Process.GetProcessById((int)ClientHelper.GetLogonQQProcessDic()[this.Username]);
                        //    System.Diagnostics.Debug.Assert(p.Id == mainProcess.Id);
                        //    this.mainProcess = p;
                        //}

                        //最小化
                        count = 30;
                        do
                        {
                            System.Threading.Thread.Sleep(1000);
                            this.MainHandle = ClientHelper.GetMainWindow(this.mainProcess.Id);
                            if (this.MainHandle != IntPtr.Zero)
                            {
                                if (this.MinimunOnLogon)
                                    Utility.ShowWindow(this.MainHandle, 6);
                                break;
                            }

                        } while (this.MainHandle == IntPtr.Zero && count-- > 0 && !this.mainProcess.HasExited);

                        if (this.MainHandle == IntPtr.Zero && this.LooseLogonCheck)
                        {
                            var wnd = Utility.FindWindows(@"^QQ$", "TXGuiFoundation").FirstOrDefault();
                            if (wnd != null)
                            {
                                this.mainProcess = Process.GetProcessById(wnd.ProcessId);
                                this.MainHandle = wnd.Handle;
                            }
                        }

                        if (this.MainHandle == IntPtr.Zero)
                        {
                            StatusMsg = "登录超时，未能识别主界面进程";
                            Exit(false);
                            return ErrType.OperationTimeout;
                        }

                        if (!DisableOnLineCount)
                        {
                            this.timer = new System.Threading.Timer(new TimerCallback((o) =>
                            {
                                try
                                {
                                    this.OnLineMinute++;
                                    if (this.MaxOnLineMinute > 0 && this.OnLineMinute >= this.MaxOnLineMinute || this.mainProcess.HasExited)
                                    {
                                        Exit();
                                    }
                                }
                                catch { }

                            }), null, 60000, 60000);
                        }
                        this.OnLineMinute = 0;

                        var handler = ClientLogon;
                        if (handler != null)
                            handler(this, new EventArgs());

                        StatusMsg = "登录成功";
                        return ErrType.Success;
                    }

                    #endregion

                    #region Timeout
                    if (time >= timeout)
                    {
                        ClientHelper.ExitProcess(mainProcess);
                        StatusMsg = "登录QQ超时！";
                        return ErrType.OperationTimeout;
                    }
                    #endregion


                    ClientHelper.ExitProcess(mainProcess);
                    err = ErrType.LoginErr;
                    StatusMsg = "登录失败！";
                    return err;
                }
                finally
                {
                    if (lastVcodeArgs != null && lastVcodeArgs.Bytes != null)
                        lastVcodeArgs.Bytes.Dispose();
                    if (vcodeArgs != null && vcodeArgs.Bytes != null)
                        vcodeArgs.Bytes.Dispose();
                }
            }
        }

        public ErrType DelFriends(DelFriendType delType, IntPtr friHwnd, bool checkCurrScreen = false)
        {
            #region Const point definition
            const int WNDSIZE_X = 1; //795;
            const int WNDSIZE_Y = 1; //538;

            const int ALLFRIENDS_X = 38;
            const int ALLFRIENDS_Y = 134;

            const int LASTLOGINTIME_X = 705;
            const int LASTLOGINTIME_Y = 128;

            const int DELFRIEND_X = 778;
            const int DELFRIEND_Y = 94;

            const int LISTITEMBASE_X = 672;
            const int LISTITEMBASE_Y = 149;
            const int LISTITEMBASE_W = 60;
            const int LISTITEMBASE_H = 32;

            const int CHECKBOXBASE_X = 209;
            const int CHECKBOXBASE_Y = 160;

            #endregion

            //检查好友管理器是否正常
            Utility.SetWindowPosition(friHwnd, WNDSIZE_X, WNDSIZE_Y);
            Utility.ForceShowWindow(friHwnd, true);
            Utility.SetForegroundWindow(friHwnd);

            var listCtl = Utility.GetAutoElementByPath(friHwnd, new int[] { 1, 0, 1, 0, 0, 3, 0, 1 });
            if (listCtl == null)
            {
                StatusMsg = "删除好友操作失败，不能识别的好友管理器窗口";
                return ErrType.OperationFailed;
            }

            //点击全部好友
            #region Init operation
            Utility.MouseClickMsg(friHwnd, ALLFRIENDS_X, ALLFRIENDS_Y);
            System.Threading.Thread.Sleep(500);
            #endregion

            #region 等待好友列表加载完成

            Bitmap b1 = null, b2 = null;
            int count = 4;// 30;
            int diff = 0;

            do
            {
                Utility.MouseMove(0, 0);
                #region Dispose previous

                if (b1 != null)
                {
                    b1.Dispose();
                }
                if (b2 != null)
                {
                    b2.Dispose();
                }

                #endregion

                //make b1
                Utility.MouseClickMsg(friHwnd, LASTLOGINTIME_X, LASTLOGINTIME_Y);
                System.Threading.Thread.Sleep(1000);
                Utility.ForceShowWindow(friHwnd, true);
                b1 = Utility.CaptureWindowScreenArea(friHwnd, LISTITEMBASE_X, LISTITEMBASE_Y, LISTITEMBASE_W, LISTITEMBASE_H);
                b1.ToGrey();
                b1.Thresholding();
                //b1.Save(Path.Combine(Application.StartupPath, "b1.bmp"));

                //make b2
                Utility.MouseClickMsg(friHwnd, LASTLOGINTIME_X, LASTLOGINTIME_Y);
                System.Threading.Thread.Sleep(1000);
                Utility.ForceShowWindow(friHwnd, true);
                b2 = Utility.CaptureWindowScreenArea(friHwnd, LISTITEMBASE_X, LISTITEMBASE_Y, LISTITEMBASE_W, LISTITEMBASE_H);
                b2.ToGrey();
                b2.Thresholding();
                //b2.Save(Path.Combine(Application.StartupPath, "b2.bmp"));

                if (!Utility.IsSameBitmap(b1, b2))
                {
                    diff++;
                }

            } while (count-- > 0 && diff < 2);

            if (diff < 2)
            {
                if (b1 != null) b1.Dispose();
                if (b2 != null) b2.Dispose();

                #region 截图当前屏，判断是不是当前屏都是7天以内的，如果是，则认为已经删除过
                Utility.ForceShowWindow(friHwnd, true);
                b1 = Utility.CaptureWindowScreenArea(friHwnd, LISTITEMBASE_X, LISTITEMBASE_Y, LISTITEMBASE_W, LISTITEMBASE_H * 12);
                b1.ToGrey();
                b1.Thresholding();
                //b1.Save(Path.Combine(Application.StartupPath, "b1.bmp"));
                #endregion

                if (checkCurrScreen)
                {
                    int scount = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        b2 = Utility.GetBitmapPart(b1, 0, 0, LISTITEMBASE_W, LISTITEMBASE_H, 0, i * LISTITEMBASE_H);
                        //b2.Save(Path.Combine(Application.StartupPath, "b2.bmp"));
                        if (ClientHelper.IsBitmapSimilarTo7(b2))//Utility.IsSameBitmap(_7bit, b2))
                        {
                            scount++;
                        }
                        b2.Dispose();
                    }

                    if (scount == 12)
                    {
                        StatusMsg = "无需删除好友";
                        return ErrType.Success;
                    }
                }

                StatusMsg = "等待好友管理器就绪超时";
                return ErrType.OperationTimeout;
            }
            #endregion

            #region 找出7天以内的标志图片
            Bitmap _7bit = null;
            try
            {
                //b1.ToGrey();
                //b1.Thresholding();
                //b2.ToGrey();
                //b2.Thresholding();

                //int b1c = b1.TotalBlackPixels();
                //int b2c = b2.TotalBlackPixels();

                //b1.Save(Path.Combine(Application.StartupPath, "b1_.bmp"));
                //b2.Save(Path.Combine(Application.StartupPath, "b2_.bmp"));

                if (ClientHelper.IsBitmapSimilarTo7(b2))
                {
                    _7bit = b2;
                    b1.Dispose();
                }
                else if (ClientHelper.IsBitmapSimilarTo7(b1))
                {
                    _7bit = b1;
                    b2.Dispose();
                }
                else
                {
                    b1.Dispose();
                    b2.Dispose();
                }
            }
            catch { }

            if (_7bit == null)
            {
                StatusMsg = "删除好友初始化失败";
                return ErrType.OperationTimeout;
            }
            //_7bit.Save(Path.Combine(Application.StartupPath, "_7bit.bmp"));
            #endregion

            bool complete = false;
            while (true && !HasExited)
            {
                #region 让7天以内的排最上边，然后再让他排下去
                count = 4;
                Utility.MouseClickMsg(friHwnd, ALLFRIENDS_X, ALLFRIENDS_Y);
                System.Threading.Thread.Sleep(500);
                bool ready = false;
                do
                {
                    Utility.MouseClickMsg(friHwnd, LASTLOGINTIME_X, LASTLOGINTIME_Y);
                    System.Threading.Thread.Sleep(1000);
                    Utility.ForceShowWindow(friHwnd, true);
                    b1.Dispose();
                    b1 = Utility.CaptureWindowScreenArea(friHwnd, LISTITEMBASE_X, LISTITEMBASE_Y, LISTITEMBASE_W, LISTITEMBASE_H);
                    b1.ToGrey();
                    b1.Thresholding();
                    //b1.Save(Path.Combine(Application.StartupPath, "b1.bmp"));
                    if (ClientHelper.IsBitmapSimilarTo7(b1))//Utility.IsSameBitmap(_7bit, b1))
                    {
                        ready = true;
                        break;
                    }
                } while (!ready && count-- > 0);
                b1.Dispose();

                if (!ready)
                {
                    _7bit.Dispose();
                    StatusMsg = "删除好友初始化失败";
                    return ErrType.OperationFailed;
                }
                Utility.MouseClickMsg(friHwnd, LASTLOGINTIME_X, LASTLOGINTIME_Y);
                System.Threading.Thread.Sleep(1000);
                #endregion

                #region 截图当前屏，找出所有不是7天以内的，打标记
                Utility.ForceShowWindow(friHwnd, true);
                b1 = Utility.CaptureWindowScreenArea(friHwnd, LISTITEMBASE_X, LISTITEMBASE_Y, LISTITEMBASE_W, LISTITEMBASE_H * 12);
                b1.ToGrey();
                b1.Thresholding();
                //b1.Save(Path.Combine(Application.StartupPath, "b1.bmp"));
                #endregion

                bool hasCheck = false;
                for (int i = 0; i < 12; i++)
                {
                    b2 = Utility.GetBitmapPart(b1, 0, 0, LISTITEMBASE_W, LISTITEMBASE_H, 0, i * LISTITEMBASE_H);
                    //b2.Save(Path.Combine(Application.StartupPath, "b2.bmp"));
                    if (ClientHelper.IsBitmapSimilarTo7(b2))//Utility.IsSameBitmap(_7bit, b2))
                    {
                        complete = true;
                        break;
                    }
                    b2.Dispose();
                    hasCheck = true;
                    Utility.MouseClickMsg(friHwnd, CHECKBOXBASE_X, CHECKBOXBASE_Y + LISTITEMBASE_H * i);
                    System.Threading.Thread.Sleep(random.Next(500, 1000));
                }
                b1.Dispose();
                b2.Dispose();


                if (!hasCheck)
                {
                    if (complete)
                    {
                        StatusMsg = "删除好友完成";
                        return ErrType.Success;
                    }
                    else
                    {
                        StatusMsg = "删除好友出错";
                        return ErrType.OperationFailed;
                    }
                }

                System.Threading.Thread.Sleep(500);
                Utility.MouseClickMsg(friHwnd, DELFRIEND_X, DELFRIEND_Y);

                #region 确认删除好友
                List<Utility.Win32WindowInfo> mlist = null;

                count = 4;
                do
                {
                    mlist = Utility.FindWindows("删除好友", this.mainProcess.Id, "TXGuiFoundation");
                    if (mlist.Count == 0)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                } while ((mlist == null || mlist.Count == 0) && count-- > 0);

                if (mlist == null || mlist.Count == 0)
                {
                    StatusMsg = "删除好友出错";
                    return ErrType.OperationFailed;
                }

                foreach (var item in mlist)
                {
                    Utility.PostKey(item.Handle, 13, false);
                }

                count = 10;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    mlist = Utility.FindWindows("删除好友", this.mainProcess.Id, "TXGuiFoundation");
                } while (mlist.Count > 0 && count-- > 0);

                if (mlist.Count > 0)
                {
                    StatusMsg = "删除好友出错";
                    return ErrType.OperationFailed;
                }
                #endregion

                if (complete)
                {
                    StatusMsg = "删除好友完成";
                    return ErrType.Success;
                }

                System.Threading.Thread.Sleep(1000);
            }

            return ErrType.OperationFailed;
        }

        public ErrType AddFriend(string targetQQ, string verifyMsg = null)
        {
            IntPtr profile;
            ErrType ret = ErrType.Unknown;

            int count = 2;
            do
            {
                ret = SearchQQ(targetQQ, out profile);
            } while (ret == ErrType.OperationTimeout && count-- > 0);

            System.Diagnostics.Debug.WriteLine("ret:{0},profile:{1}", ret, profile);
            if (ret != ErrType.Success)
                return ret;
            ret = AddFriendFromProfileWindow(profile, verifyMsg);
            Utility.CloseWindow(profile);

            return ret;
        }

        public ErrType OpenSearchWindow(out IntPtr handle, bool waitInitComplete = true)
        {
            #region Const point definition

            const int SEARCHBUTTON_X = 613;
            const int SEARCHBUTTON_Y = 117;
            const int CHECKPOINTOFFSET_W = 10;
            const int CHECKPOINTOFFSET_H = 36;

            #endregion

            handle = IntPtr.Zero;
            Utility.Win32WindowInfo searchHwnd = null;

            System.Diagnostics.Debug.Assert(this.MainHandle != IntPtr.Zero);

            Utility.ForceShowWindow(this.MainHandle, true);
            Utility.SetForegroundWindow(this.MainHandle);

            var ctl = ClientHelper.GetSearchEntry(this.MainHandle);

            if (ctl == null)
            {
                this.MainHandle = ClientHelper.GetMainWindow(this.mainProcess.Id);
                if (this.MainHandle == IntPtr.Zero && this.LooseLogonCheck)
                {
                    var wnd = Utility.FindWindows("^QQ$", "TXGuiFoundation").FirstOrDefault();
                    if (wnd != null)
                    {
                        this.MainHandle = wnd.Handle;
                        this.mainProcess = Process.GetProcessById(wnd.ProcessId);
                    }
                }
                ctl = ClientHelper.GetSearchEntry(this.MainHandle);
            }

            if (ctl == null)
            {
                //statusMsg = "打开查找窗口失败，未找到打开入口";
                //return ErrType.OperationFailed;

                //尝试点击坐标
                var mh = Utility.GetAutomationElementFromHandle(this.MainHandle);
                if (mh != null)
                {
                    var rt = mh.Current.BoundingRectangle;
                    Utility.MouseClickMsg(this.MainHandle, (int)(rt.Width - 120), (int)(rt.Height - 20));
                }
                else
                {
                    Utility.SetWindowPosition(this.MainHandle, 0, 0);
                    System.Threading.Thread.Sleep(1000);
                    Utility.MouseClickMsg(this.MainHandle, 190, 518);
                }
            }
            else
            {
                var bound = ctl.Current.BoundingRectangle;
                Utility.MouseClick((int)(bound.X + 20), (int)(bound.Y + bound.Height / 2));
            }

            int count = 5;
            do
            {
                System.Threading.Thread.Sleep(1000);
                var list = Utility.FindWindows("查找", this.mainProcess.Id, "TXGuiFoundation");
                if (list.Count > 0)
                {
                    searchHwnd = list[0];
                    break;
                }
            } while (count-- > 0 && searchHwnd == null);

            if (searchHwnd == null)
            {
                statusMsg = "打开查找窗口失败，等待超时";
                return ErrType.OperationTimeout;
            }
            handle = searchHwnd.Handle;
            statusMsg = "打开查找窗口成功";


            if (waitInitComplete)
            {
                count = 30;
                bool finish = false;
                do
                {
                    Utility.MouseClickMsg(handle, 230, 40);
                    System.Threading.Thread.Sleep(1000);
                    Utility.MouseClickMsg(handle, 230, 40);
                    Utility.ForceShowWindow(handle, true);
                    Utility.SetForegroundWindow(handle);
                    var bitmap = Utility.CaptureWindowScreenArea(handle, SEARCHBUTTON_X - CHECKPOINTOFFSET_W, SEARCHBUTTON_Y - CHECKPOINTOFFSET_H, CHECKPOINTOFFSET_W, CHECKPOINTOFFSET_H);
                    if (bitmap != null)
                    {
                        finish = bitmap.GetPixel(bitmap.Width - 1, 0) != bitmap.GetPixel(bitmap.Width - 1, bitmap.Height - 1);
                        //bitmap.Save(Path.Combine(Application.StartupPath,"\\check.bmp"));
                        bitmap.Dispose();
                        bitmap = null;
                    }
                } while (!finish && count-- > 0);
                Debug.Assert(finish);
            }

            return ErrType.Success;
        }

        /// <summary>
        /// For test
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="mainHandle"></param>
        public void TestEntry(int processId, IntPtr mainHandle)
        {
            this.mainProcess = Process.GetProcessById(processId);
            this.MainHandle = mainHandle;

            AddFriend("2343243", "你好");
        }

        public ErrType SearchQQ(string qq, out IntPtr profileHandle)
        {
            profileHandle = IntPtr.Zero;

            #region Const point definition
            const int PROFILEPOINT_X = 55;
            const int profilepoint_Y = 287;
            var firstResultRect = new Rectangle(18, 209, 200, 85);

            #endregion

            int count;

            IntPtr searchHandle = IntPtr.Zero;
            ErrType ret = ErrType.Unknown;
            count = 4;

            while (ret != ErrType.Success && count-- > 0)
            { 
                ret = OpenSearchWindow(out searchHandle);
            }

            if (ret != ErrType.Success)
            {
                return ret;
            }

            try
            {
                ClientHelper.KillDirtyProcesses();
                ClientHelper.KillDirtyWindows();
                System.Threading.Thread.Sleep(5000);
                //Utility.ForceShowWindow(searchHandle, true);
                //Utility.SetForegroundWindow(searchHandle);
                //Utility.MouseClickMsg(searchHandle, 100, 100);
                //Utility.PostText(searchHandle, qq);

                Utility.ForceShowWindow(searchHandle, true);
                Utility.SetForegroundWindow(searchHandle);
                Utility.MouseClickMsg(searchHandle, 100, 100);
                Utility.PostText(searchHandle, string.Empty);

                System.Threading.Thread.Sleep(500);

                Utility.ForceShowWindow(searchHandle, true);
                for (int i = 0; i < qq.Length; i++)
                {
                    Utility.Keybd_Sendkeys(qq.Substring(i, 1));
                    System.Threading.Thread.Sleep(random.Next(100, 300));
                }
                System.Threading.Thread.Sleep(500);

                var b1 = Utility.CaptureWindowScreenArea(searchHandle, firstResultRect.X, firstResultRect.Y, firstResultRect.Width, firstResultRect.Height);
                Bitmap b2 = null;
                bool same = false;
                Utility.PostKey(searchHandle, 13, false);
                count = 5;
                do
                {
                    Utility.ForceShowWindow(searchHandle, true);
                    Utility.SetForegroundWindow(searchHandle);
                    System.Threading.Thread.Sleep(1000);
                    b2 = Utility.CaptureWindowScreenArea(searchHandle, firstResultRect.X, firstResultRect.Y, firstResultRect.Width, firstResultRect.Height);
                } while ((same = Utility.IsSameBitmap(b1, b2)) && count-- > 0);
                b1.Dispose();

                if (same)
                {
                    b2.Dispose();
                    statusMsg = "查找好友等待结果超时";
                    return ErrType.OperationTimeout;
                }

                b2.ToGrey();
                b2.Thresholding();
                if (b2.TotalBlackPixels() == 0)
                {
                    b2.Dispose();
                    statusMsg = "没有对应的好友查找结果";
                    return ErrType.OperationFailed;
                }
                b2.Dispose();

                //找到好友，点击资料
                Utility.ForceShowWindow(searchHandle, false);
                Utility.SetForegroundWindow(searchHandle);
                Utility.MouseClickMsg(searchHandle, PROFILEPOINT_X, profilepoint_Y);
                count = 5;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    var wnd = Utility.FindWindows("^.*的资料$", this.mainProcess.Id, "TXGuiFoundation").FirstOrDefault();
                    if (wnd != null)
                    {
                        profileHandle = wnd.Handle;
                    }
                } while (profileHandle == IntPtr.Zero && count-- > 0);

                if (profileHandle == IntPtr.Zero)
                {
                    statusMsg = "打开好友资料夹失败超时";
                    return ErrType.OperationTimeout;
                }

                statusMsg = "打开好友资料夹成功";
                return ErrType.Success;
            }
            finally
            {
                Utility.CloseWindow(searchHandle);
            }
        }

        public ErrType AddFriendFromProfileWindow(IntPtr profileHandle, string verifyMsg = null)
        {
            Utility.ForceShowWindow(profileHandle, false);
            Utility.SetForegroundWindow(profileHandle);
            const int VERIFYMSGINPUT_X = 240;
            const int VERIFYMSGINPUT_Y = 110;
            const int NEXTSTEPBUTTON_X = 342;
            const int NEXTSTEPBUTTON_Y = 344;

            #region 准备工作
            var ac = Utility.GetAutomationElementFromHandle(profileHandle);
            if (ac == null)
            {
                statusMsg = "无法识别资料夹窗口";
                return ErrType.OperationFailed;
            }

            var tmp = Utility.GetAutoElementByPath(ac, new[] { 2, 0, 0, 0 });
            if (tmp == null)
            {
                statusMsg = "无法识别资料夹窗口";
                return ErrType.OperationFailed;
            }

            var addFriendButton = Utility.GetAutoElementByPath(ac, new[] { 2, 0, 0, 0, 0, 0, 2, 2 });
            if (addFriendButton == null)
            {
                statusMsg = "无法识别资料夹窗口";
                return ErrType.OperationFailed;
            }

            var children = addFriendButton.FindAll(TreeScope.Children, Condition.TrueCondition);
            bool needAdd = false;
            foreach (AutomationElement item in children)
            {
                if (item.Current.Name == "加为好友")
                {
                    needAdd = true;
                    addFriendButton = item;
                    break;
                }
            }

            if (!needAdd)
            {
                statusMsg = "该好友无需添加";
                return ErrType.OperationFailed;
            }

            children = tmp.FindAll(TreeScope.Children, Condition.TrueCondition);
            AutomationElement tabCtl = null;
            foreach (AutomationElement item in children)
            {
                if (item.Current.ControlType == ControlType.Tab)
                {
                    tabCtl = item;
                    break;
                }
            }
            if (tabCtl == null)
            {
                statusMsg = "无法识别资料夹窗口";
                return ErrType.OperationFailed;
            }
            AutomationElement profileTab = null, newsTab = null, albumTab = null, tagTab = null;
            children = tabCtl.FindAll(TreeScope.Children, Condition.TrueCondition);
            foreach (AutomationElement item in children)
            {
                switch (item.Current.Name)
                {
                    case "资料":
                        profileTab = item;
                        break;
                    case "动态":
                        newsTab = item;
                        break;
                    case "相册":
                        albumTab = item;
                        break;
                    case "标签":
                        tagTab = item;
                        break;
                    default:
                        break;
                }
            }
            #endregion

            #region 随机移动

            for (int i = 0; i < 4; i++)
            {
                AutomationElement ame = null;
                switch (random.Next(0, 4))
                {
                    case 0:
                        ame = profileTab;
                        break;
                    case 1:
                        ame = newsTab;
                        break;
                    case 2:
                        ame = albumTab;
                        break;
                    case 3:
                        ame = tagTab;
                        break;
                    default:
                        ame = null;
                        break;
                }
                if (ame == null)
                    continue;

                Utility.ForceShowWindow(profileHandle, true);
                Utility.SetForegroundWindow(profileHandle);
                var rect = ame.Current.BoundingRectangle;
                var point = Utility.RandomMoveMouse(profileHandle, new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height), relative: false);
                Utility.MouseClick(point.X, point.Y);
                System.Threading.Thread.Sleep(random.Next(3000, 6000));
            }
            #endregion

            #region 点击加好友
            var pos = addFriendButton.Current.BoundingRectangle;
            Utility.ForceShowWindow(profileHandle, true);
            Utility.ForceShowWindow(profileHandle, false);
            Utility.SetForegroundWindow(profileHandle);
            Utility.MouseClick((int)pos.X, (int)pos.Y);
            #endregion

            #region 等待好友弹窗
            int count = 5;
            AutomationElement addWindow = null;
            do
            {
                System.Threading.Thread.Sleep(1000);
                var hwnd = Utility.FindWindows("^.* - 添加好友$", this.mainProcess.Id, "TXGuiFoundation").FirstOrDefault();
                if (hwnd != null)
                {
                    addWindow = Utility.GetAutomationElementFromHandle(hwnd.Handle);
                    Utility.ForceShowWindow((IntPtr)addWindow.Current.NativeWindowHandle, true);
                }
            } while (addWindow == null && count-- > 0);

            if (addWindow == null)
            {
                statusMsg = "等待添加好友确认窗口超时";
                return ErrType.OperationFailed;
            }
            #endregion

            #region 判断对方的加好友类型：直接加、验证问题、验证信息等情况
            AddFriendVerifyType verifyType = AddFriendVerifyType.Unknown;

            count = 30;
            do
            {
                System.Threading.Thread.Sleep(1000);
                var ctl = Utility.GetAutoElementByPath(addWindow, new[] { 1, 0, 1, 0, 0, 0, 0, 0, 0 });
                if (ctl != null && ctl.Current.Name.IndexOf("验证问题") >= 0) //验证问题，跳过
                {
                    verifyType = AddFriendVerifyType.VerifyQuestion;
                }
                else if (ctl != null && ctl.Current.Name.StartsWith("备注姓名"))
                {
                    verifyType = AddFriendVerifyType.Any;
                }
                else if (ctl != null && ctl.Current.Name.IndexOf("验证信息") >= 0)
                {
                    verifyType = AddFriendVerifyType.VerifyMessage;
                }
                else
                {
                    var captchaWnd = Utility.FindWindows("身份验证", this.mainProcess.Id, "TXGuiFoundation").FirstOrDefault();
                    if (captchaWnd != null)
                    {
                        statusMsg = "添加好友出现验证码";
                        Utility.CloseWindow(captchaWnd.Handle);
                        Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                        return ErrType.AddFriendLimit;
                    }

                    var ctl1 = Utility.GetAutoElementByPath(addWindow, new[] { 1, 0, 1, 0, 0, 0, 0, 0, 1, 0 });
                    if (ctl1 != null && ctl1.Current.Name.StartsWith("对方拒绝被添加"))
                    {
                        verifyType = AddFriendVerifyType.Deny;
                    }
                    else
                    {
                        verifyType = AddFriendVerifyType.Unknown;
                    }
                }
            } while (verifyType == AddFriendVerifyType.Unknown && count-- > 0);

            if (verifyType == AddFriendVerifyType.Unknown)
            {
                Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                statusMsg = "不能识别的好友验证类型";
                return ErrType.OperationFailed;
            }
            if (verifyType == AddFriendVerifyType.VerifyQuestion)
            {
                Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                statusMsg = "对方需要你回答问题";
                return ErrType.OperationFailed;
            }


            #endregion

            #region 确认加好友
            AutomationElement nextButton = Utility.GetAutoElementByPath(addWindow, new[] { 0, 0, 0 });
            if (nextButton == null || nextButton.Current.Name != "下一步")
            {
                Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                statusMsg = "不能识别的添加好友窗口";
                return ErrType.OperationFailed;
            }

            switch (verifyType)
            {
                case AddFriendVerifyType.VerifyMessage:
                    Utility.ForceShowWindow((IntPtr)addWindow.Current.NativeWindowHandle, true);
                    Utility.ForceShowWindow((IntPtr)addWindow.Current.NativeWindowHandle, false);
                    Utility.SetForegroundWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                    Utility.MouseClickMsg((IntPtr)addWindow.Current.NativeWindowHandle, VERIFYMSGINPUT_X, VERIFYMSGINPUT_Y);
                    Utility.MouseClickMsg((IntPtr)addWindow.Current.NativeWindowHandle, VERIFYMSGINPUT_X, VERIFYMSGINPUT_Y);
                    Utility.MouseClickMsg((IntPtr)addWindow.Current.NativeWindowHandle, VERIFYMSGINPUT_X, VERIFYMSGINPUT_Y);
                    Utility.PostText((IntPtr)addWindow.Current.NativeWindowHandle, verifyMsg ?? "找你有点事");
                    System.Threading.Thread.Sleep(1000);
                    break;
                case AddFriendVerifyType.Any:
                    break;
                default:
                    Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                    statusMsg = "不能识别的好友验证类型";
                    return ErrType.OperationFailed;
            }
            Utility.MouseClickMsg((IntPtr)addWindow.Current.NativeWindowHandle, NEXTSTEPBUTTON_X, NEXTSTEPBUTTON_Y);
            #endregion

            #region 等待结果
            count = 30;
            do
            {
                System.Threading.Thread.Sleep(1000);
                var ctl = Utility.GetAutoElementByPath(addWindow, new[] { 1, 0, 1, 0, 0, 0, 0, 0, 0 });
                var ctl1 = Utility.GetAutoElementByPath(addWindow, new[] { 1, 0, 1, 0, 0, 0, 0, 0, 1, 0 });
                if (ctl1 != null && ctl1.Current.Name.IndexOf("成功添加") >= 0)
                {
                    statusMsg = "添加好友成功";
                    Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                    return ErrType.Success;
                }
                else if (ctl1 != null && ctl1.Current.Name.IndexOf("好友添加请求已经发送成功") >= 0)
                {
                    statusMsg = "好友添加请求发送成功";
                    Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                    return ErrType.Success;
                }
                else if (ctl != null && ctl.Current.Name.StartsWith("备注姓名"))
                {
                    var err = Utility.GetAutoElementByPath(addWindow, new[] { 1, 0, 1, 0, 0, 0, 2, 0, 1 });
                    if (err != null && !string.IsNullOrEmpty(err.Current.Name))// && err.Current.Name.StartsWith("抱歉"))
                    {
                        statusMsg = err.Current.Name;
                        Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
                        return ErrType.AddFriendLimit;
                    }
                    Utility.MouseClickMsg((IntPtr)addWindow.Current.NativeWindowHandle, NEXTSTEPBUTTON_X, NEXTSTEPBUTTON_Y);
                    continue;
                }

            } while (count-- > 0);
            #endregion

            statusMsg = "添加好友失败，操作超时";
            Utility.CloseWindow((IntPtr)addWindow.Current.NativeWindowHandle);
            return ErrType.OperationFailed;

        }

    }

    public enum ErrType
    {
        Success,
        Unknown,
        ExePathErr,
        VcodeErr,
        LoginErr,
        AccountErr,
        PasswordErr,
        ReachMaxLogin,
        OperationTimeout,
        AccountProtect,  //should stop the work
        OperationFailed,
        AddFriendLimit,
        LoginDeviceVeify,
    }

    public enum DelFriendType
    {
        MoreThen7Days,
        All
    }

    public enum AddFriendVerifyType
    {
        Unknown,
        Any,
        VerifyMessage,
        VerifyQuestion,
        Deny
    }
}
