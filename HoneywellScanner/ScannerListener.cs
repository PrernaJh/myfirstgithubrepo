using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using ParcelPrepGov.API.Client.Data;
using ParcelPrepGov.API.Client.Data.Constants;
using ParcelPrepGov.API.Client.Interfaces;
using ParcelPrepGov.API.Client.Services;
using SPS.PP.ESM.WiFi.Decoding;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HoneywellScanner
{
    public enum ScannerMode
    {
        Login,
        AssignActiveContainer,
        AssignNewContainer,
        ReplaceContainer
    }

    public class ScannerState
    {
        public ScannerState(IDictionary<string,string> aliases)
        {
            Aliases = aliases;
        }
        public bool SetMode(string alias)
        {
            ScannerMode mode;
            if (!Aliases.TryGetValue(alias, out var barcode))
                barcode = alias;

            if (barcode == "Login" || barcode == "Logoff")
                mode = ScannerMode.Login;
            else if (barcode == "AssignActiveContainer")
                CurrentApplication = mode = ScannerMode.AssignActiveContainer;
            else if (barcode == "AssignNewContainer")
                CurrentApplication = mode = ScannerMode.AssignNewContainer;
            else if (barcode == "ReplaceContainer")
                CurrentApplication = mode = ScannerMode.ReplaceContainer;
            else
                return false;
            SetMode(mode);
            return true;
        }

        public void SetMode(ScannerMode mode)
        {
            lock (this)
            {
                ApiWrapper.ShowStatusAlert(ConnID, ScannerStatus_WiFi.ssNormal_WiFi);
                if (mode == ScannerMode.Login || SiteName == null)
                {
                    ShowText(TextColors_WiFi.DefaultColor_WiFi, TextFontSizes_WiFi.LargeBold_WiFi, "Login", "Scan Badge");
                    SiteName = null;
                    Mode = ScannerMode.Login;
                    return;
                }
                Mode = mode;
                StartActivityTimer();
                switch (mode)
                {
                    case ScannerMode.AssignActiveContainer:
                        ShowText(TextColors_WiFi.DefaultColor_WiFi, TextFontSizes_WiFi.LargeBold_WiFi, "Assign Active", "Scan Package");
                        break;
                    case ScannerMode.AssignNewContainer:
                        if (PackageId == null)
                            ShowText(TextColors_WiFi.DefaultColor_WiFi, TextFontSizes_WiFi.LargeBold_WiFi, "Assign New", "Scan Package");
                        else
                            ShowText(TextColors_WiFi.DefaultColor_WiFi, TextFontSizes_WiFi.LargeBold_WiFi, "Assign New", "Scan Container");
                        break;
                    case ScannerMode.ReplaceContainer:
                        if (ContainerId == null)
                            ShowText(TextColors_WiFi.DefaultColor_WiFi, TextFontSizes_WiFi.LargeBold_WiFi, "Replace Cont.", "Scan Old");
                        else
                            ShowText(TextColors_WiFi.DefaultColor_WiFi, TextFontSizes_WiFi.LargeBold_WiFi, "Replace Cont.", "Scan New");
                        break;
                }

            }
        }

        public readonly IDictionary<string, string> Aliases;
        public uint ConnID { get; set; }
        public string SiteName { get; set; }
        public string Username { get; set; }
        public string MachineId { get; set; }
        public HonScannerAPIWrapper ApiWrapper { get; set; }
        public ScannerMode Mode { get; set; }
        public ScannerMode CurrentApplication { get; set; }
       
        public string PackageId { get; set; }
        public string ContainerId { get; set; }

        public Timer ApplicationTimer { get; set; }
        public Timer ActivityTimer { get; set; }
        public uint ActivityTimeout { get; set; }

        private void ShowText(TextColors_WiFi color, TextFontSizes_WiFi size, string topLine, string bottomLine)
        {
            ApiWrapper.SetDisplayColor(ConnID, TextColorType_WiFi.BgColor_WiFi, TextColors_WiFi.DefaultColor_WiFi);

            ApiWrapper.SetTextSize(ConnID, TextLineType_WiFi.UpLine_WiFi, size);
            ApiWrapper.SetDisplayColor(ConnID, TextColorType_WiFi.FgColorUpLine_WiFi, color);
            ApiWrapper.SetDisplayText(ConnID, TextLineType_WiFi.UpLine_WiFi, topLine);

            ApiWrapper.SetTextSize(ConnID, TextLineType_WiFi.BottomLine_WiFi, size);
            ApiWrapper.SetDisplayColor(ConnID, TextColorType_WiFi.FgColorBottomLine_WiFi, color);
            ApiWrapper.SetDisplayText(ConnID, TextLineType_WiFi.BottomLine_WiFi, bottomLine);
        }

        public void GoodScan()
        {
            ApiWrapper.ShowStatusAlert(ConnID, ScannerStatus_WiFi.ssGoodScan_WiFi);
            PackageId = null;
            ContainerId = null;
            StartApplicationTimer();
        }

        public void BadScan(string topLine, string bottomLine)
        {
            ShowText(TextColors_WiFi.Red_WiFi, TextFontSizes_WiFi.LargeBold_WiFi, topLine, bottomLine);
            PackageId = null;
            ContainerId = null;
        }

        private void StartApplicationTimer()
        {
            lock (this)
            {
                EndApplicationTimer();
                ApplicationTimer = new Timer(ApplicationTimerFired, this, 2000, 0);
            }
        }

        private void EndApplicationTimer()
        {
            lock (this)
            {
                if (ApplicationTimer != null)
                {
                    ApplicationTimer.Dispose();
                    ApplicationTimer = null;
                }
            }
        }

        private static void ApplicationTimerFired(object obj)
        {
            var state = (ScannerState)obj;
            lock (state)
            {
                state.EndApplicationTimer();
                state.SetMode(state.CurrentApplication);
            }
        }

        private void StartActivityTimer()
        {
            lock (this)
            {
                if (ActivityTimeout != 0)
                {
                    EndActivityTimer();
                    ActivityTimer = new Timer(ActivityTimerFired, this, ActivityTimeout*60*1000, 0);
                }
            }
        }

        private void EndActivityTimer()
        {
            lock (this)
            {
                if (ActivityTimer != null)
                {
                    ActivityTimer.Dispose();
                    ActivityTimer = null;
                }
            }
        }

        private static void ActivityTimerFired(object obj)
        {
            var state = (ScannerState)obj;
            lock (state)
            {
                state.EndActivityTimer();
                state.SetMode(ScannerMode.Login);
            }
        }
    }

    public class ScannerListener
    {
        private readonly ILogger<ScannerListener> logger;
        private readonly IConfiguration configuration;
        
        private readonly uint activityTimeout;
        private AccountLogin serviceLogin;

        private readonly IAccountService accountService;
        private readonly IContainerService containerService;


        private readonly HonScannerAPIWrapper apiWrapper;
        private readonly SDKUtility utility;

        private OnConnectCallback onConnCB;
        private OnDisconnectCallback onDisconnCB;
        private OnDecodeCallback onDecodeCB;
        private OnPressButtonCallback onPressCB;
        private OnGetSymbPropCallback onSymbPropCB;
        private OnSendMenuCmdCallback onMenuCmdCB;

        private ConcurrentDictionary<uint, ScannerInfo_WiFi> connections = new ConcurrentDictionary<uint, ScannerInfo_WiFi>();
        private ConcurrentDictionary<uint, ScannerState> states = new ConcurrentDictionary<uint, ScannerState>();

        public ScannerListener(IServiceProvider serviceProvider, string serverIp, uint serverPort, uint activityTimeout)
        {
            logger = serviceProvider.GetRequiredService<ILogger<ScannerListener>>();
            configuration = serviceProvider.GetRequiredService<IConfiguration>();
            
            accountService = serviceProvider.GetRequiredService<IAccountService>();
            containerService = serviceProvider.GetRequiredService<IContainerService>();

            apiWrapper = new HonScannerAPIWrapper();
            utility = new SDKUtility();

            onConnCB = new OnConnectCallback(OnConnect);
            onDisconnCB = new OnDisconnectCallback(OnDisconnect);
            onDecodeCB = new OnDecodeCallback(OnDecode);
            onPressCB = new OnPressButtonCallback(OnButtonPress);
            onSymbPropCB = new OnGetSymbPropCallback(OnGetSymbProp);
            onMenuCmdCB = new OnSendMenuCmdCallback(OnCmdRespond);


            this.activityTimeout = activityTimeout;
            logger.LogInformation($"Scanner Activity Timeout: {activityTimeout} minutes");
            
            logger.LogInformation($"LocalIP: {GetLocalIPAddress()}");
            serverIp = ! string.IsNullOrEmpty(serverIp) ? serverIp : GetLocalIPAddress();
            logger.LogInformation($"Endpoint: {serverIp}:{serverPort}");
            var code = apiWrapper.StartServer(serverIp, serverPort);
            if (code == ResultCode_WiFi.Success)
            {
                logger.LogInformation($"StartServer: {utility.APIResultToString(code)}");
                apiWrapper.RegResponseCallbacks(onConnCB, onDisconnCB, onDecodeCB, onPressCB, onSymbPropCB, onMenuCmdCB);
                ServiceLogin(null);
            }
            else
            {
                logger.LogError($"StartServer: {utility.APIResultToString(code)}");
            }
        }

        private bool ServiceLogin(ScannerState state)
        {
            lock (this)
            {
                var username = configuration.GetSection("Api").GetSection("Username").Value;
                var password = configuration.GetSection("Api").GetSection("Password").Value;
                if (serviceLogin?.cookie != null)
                {
                    var user = accountService.GetUser(username, serviceLogin);
                    if (!string.IsNullOrEmpty(user.SiteName))
                        return true;
                }
                logger.LogInformation($"Attempt service login as: {username}");
                serviceLogin = accountService.PostAccountLogin(username, password);
            }
            if (serviceLogin?.cookie != null)
            {
                logger.LogInformation($"Service Login: succeed as: {serviceLogin.username}");
            }
            else
            {
                logger.LogError($"Service login failed as: {serviceLogin.username}");
                state?.BadScan("Login Failure", "Log001");
            }
            return serviceLogin?.cookie != null;
        }

        private static string GetLocalIPAddress()
        {
            string localIP = string.Empty;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        private string GetScannerIPAddress(uint connID)
        {
            var scannerIP = string.Empty;
            if (connections.TryGetValue(connID, out var info))
            {
                int addrLen = 64;
                ushort port = 0;
                var sbClientIP = new StringBuilder(addrLen);
                apiWrapper.GetClientAddress(info.ConnID, sbClientIP, ref addrLen, ref port);
                scannerIP = sbClientIP.ToString();
            }
            return scannerIP;
        }

        private string GetSerialNum(uint connID)
        {
            if (connections.TryGetValue(connID, out var info))
                return info.SerialNum;
            return string.Empty;
        }

        private ScannerState GetScannerState(uint connID)
        {
            if (!states.TryGetValue(connID, out var state))
            {
                var aliases = Configuration.GetSection("Aliases").Get<IDictionary<string, string>>();
                state = new ScannerState(aliases) { 
                    ApiWrapper = apiWrapper, 
                    ConnID = connID, 
                    Mode = ScannerMode.AssignNewContainer,
                    ActivityTimeout = activityTimeout
                };
                states.TryAdd(connID, state);
            }
            return state;
        }


        private void OnConnect(ScannerInfo_WiFi info)
        {
            connections[info.ConnID] = info;
            logger.LogInformation($"Scanner[{info.ConnID}][{info.SerialNum}] - {GetScannerIPAddress(info.ConnID)} connected");
            var state = GetScannerState(info.ConnID);
            int addrLen = 64;
            ushort port = 0;
            StringBuilder clientIP = new StringBuilder(addrLen);
            apiWrapper.GetClientAddress(info.ConnID, clientIP, ref addrLen, ref port);
            state.MachineId = GetSerialNum(info.ConnID);
            state.SetMode(ScannerMode.Login);
        }

        private void OnDisconnect(uint connID)
        {
            if (connections.ContainsKey(connID))
            {
                logger.LogInformation($"Scanner[{connID}][{GetSerialNum(connID)}] - {GetScannerIPAddress(connID)} disconnected");
                connections.TryRemove(connID, out var _);
            }
        }

        private void OnCmdRespond(MenuCmdResponse resp)
        {
            string str = resp.Response
                .Replace("\x6", "[ACK]")
                .Replace("\x5", "[ENQ]")
                .Replace("\x15", "[NAK]");
            logger.LogInformation($"Scanner[{resp.ConnID}][{GetSerialNum(resp.ConnID)}] - Menu command returned {str}");
        }

        private void OnButtonPress(ButtonPressNotify nfy)
        {
            logger.LogInformation(utility.ButtonPressResultToString(nfy, GetSerialNum(nfy.ConnID)));
        }

        private void OnGetSymbProp(SymbPropResponse resp)
        {
            logger.LogInformation(utility.SymbPropResponseToString(resp, GetSerialNum(resp.ConnID)));
        }

        private void OnDecode(DecodeResult_WiFi res)
        {
            logger.LogInformation(utility.DecodeResultToString(res, GetSerialNum(res.ConnID)));
            var barcode = res.Message.Replace(" ", "").Trim();
            var state = GetScannerState(res.ConnID);
            if (! ServiceLogin(state))
                return;
            if (state.SetMode(barcode))
                return;
            switch (state.Mode)
            {
                case ScannerMode.Login:
                    UserLogin(state, barcode);
                    break;
                case ScannerMode.AssignActiveContainer:
                    AssignActiveContainer(state, barcode);
                    break;
                case ScannerMode.AssignNewContainer:
                    if (state.PackageId == null)
                        ScanPackage(state, barcode);
                    else
                        AssignNewContainer(state, barcode);
                    break;
                case ScannerMode.ReplaceContainer:
                    if (state.ContainerId == null)
                        ScanContainer(state, barcode);
                    else
                        ReplaceContainer(state, barcode);
                    break;
            }
        }

        private AccountLogin BuildAccountLogin(ScannerState state)
        {
            var accountLogin = new AccountLogin() { username = state.Username, cookie = serviceLogin.cookie };
            return accountLogin;
        }

        private void UserLogin(ScannerState state, string barcode)
        {
            var username = barcode;
            logger.LogInformation($"Attempt user login as: {username}");
            var user = accountService.GetUser(barcode, BuildAccountLogin(state));
            if (string.IsNullOrEmpty(user.SiteName))
            {
                logger.LogError($"User login failed: User not found: {username}, Error Code: Log002");
                state.BadScan("Login Failure", "Log002");
            }
            else if (user.SiteName == SiteConstants.AllSites)
            {
                logger.LogError($"User login failed: User must be assigned to a site: {username}, Error Code: Log003");
                state.BadScan("Login Failure", "Log003");
            }
            else
            {
                state.SiteName = user.SiteName;
                state.CurrentApplication = ScannerMode.AssignActiveContainer;
                logger.LogInformation($"User login succeed as: {username}");
                state.GoodScan();
            }
        }

        private void ScanPackage(ScannerState state, string barcode)
        {
             state.PackageId = barcode;
             state.SetMode(ScannerMode.AssignNewContainer);
        }

        private void AssignActiveContainer(ScannerState state, string barcode)
        {
            var packageId = barcode;

            AssignContainer assignContainer = new AssignContainer();
            assignContainer.packageId = packageId;
            assignContainer.machineId = state.MachineId;
            assignContainer.siteName = state.SiteName;
            containerService.PostAssignActiveContainer(ref assignContainer, BuildAccountLogin(state));
            if (assignContainer.response.IsSuccessful)
            {
                logger.LogInformation($"Package: {packageId} assigned to active container for site: {state.SiteName}");
                state.GoodScan();
            }
            else
            {
                logger.LogError($"Package: {packageId} not assigned to active container for site: {state.SiteName}: {assignContainer.response.Message}");
                state.BadScan("Assign Failed", assignContainer.response.ErrorCode);
            }
        }

        private void AssignNewContainer(ScannerState state, string barcode)
        {
            var packageId = state.PackageId;
            var newContainerId = barcode;

            AssignContainer assignContainer = new AssignContainer();
            assignContainer.packageId = packageId;
            assignContainer.newContainerId = newContainerId;
            assignContainer.machineId = state.MachineId;
            assignContainer.siteName = state.SiteName;
            containerService.PostAssignNewContainer(ref assignContainer, BuildAccountLogin(state));
            if (assignContainer.response.IsSuccessful)
            {
                logger.LogInformation($"Package: {packageId} assigned to container: {newContainerId} for site: {state.SiteName}");
                state.GoodScan();
            }
            else
            {
                logger.LogError($"Package: {packageId} not assigned to container: {newContainerId} for site: {state.SiteName}: {assignContainer.response.Message}");
                state.BadScan("Assign Failed", assignContainer.response.ErrorCode);
            }
        }

        private void ScanContainer(ScannerState state, string barcode)
        {
            state.ContainerId = barcode;
            state.SetMode(ScannerMode.ReplaceContainer);
        }

        private void ReplaceContainer(ScannerState state, string barcode)
        {
            var oldContainterId = state.ContainerId;
            var newContainterId = barcode;

            ReplaceContainer replaceContainer = new ReplaceContainer();
            replaceContainer.oldContainerId = oldContainterId;
            replaceContainer.newContainerId = newContainterId;
            replaceContainer.machineId = state.MachineId;
            replaceContainer.siteName = state.SiteName;
            containerService.PostReplaceContainer(ref replaceContainer, BuildAccountLogin(state));
            if (replaceContainer.response.IsSuccessful)
            {
                logger.LogInformation($"Container: {oldContainterId} replace by container: {newContainterId} for site: {state.SiteName}");
                state.GoodScan();
            }
            else
            {
                logger.LogError($"Container: {oldContainterId} not replaced by container: {newContainterId} for site: {state.SiteName}: {replaceContainer.response.Message}");
                state.BadScan("Replace Failed", replaceContainer.response.ErrorCode);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static IConfigurationRoot Configuration;
        private static ScannerListener Listener;

        private static IConfigurationRoot BuildConfiguration()
        {
            var appSettings = "appsettings.json";
            var configBuilder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(appSettings, optional: false, reloadOnChange: true)
               .AddEnvironmentVariables();

            return configBuilder.Build();
        }

        private static LogLevel LogLevelFromString(string level)
        {
            if (level == "Trace")
                return LogLevel.Trace;
            else if (level == "Debug")
                return LogLevel.Debug;
            else if (level == "Information")
                return LogLevel.Information;
            else if (level == "Warning")
                return LogLevel.Warning;
            else if (level == "Error")
                return LogLevel.Error;
            else if (level == "Critical")
                return LogLevel.Critical;
            else return LogLevel.Information;
        }

        private static bool TestEndpoint(string ip, int port)
        {
            try
            {
                using (var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    if (!Regex.IsMatch(ip, @"^[0-9.]+$"))
                    {
                        foreach (var address in Dns.GetHostAddresses(ip))
                        {
                            ip = address.ToString();
                            if (address.AddressFamily == AddressFamily.InterNetwork)
                                break;
                        }
                    }
                    EndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
                    Console.WriteLine($"Connecting to: {ip}:{port}");
                    s.Connect(ep);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Failed: {ex}");
                return false;
            }
        }

        public static async Task RunAsync(IDictionary<string,string> argOverrides, CancellationToken cancellationToken)
        {
            if (Listener == null)
            {
                var builder = new HostBuilder();

                var appSettings = string.Empty;
                Configuration = BuildConfiguration();
                foreach (var arg in argOverrides)
                {
                    if (! string.IsNullOrEmpty(arg.Value))
                        Configuration.GetSection(arg.Key.Replace(".", ":")).Value = arg.Value;
                }

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddApplicationInsights();
                    logging.AddConsole();
                    logging.AddDebug();
                    // For some unknown reason the ApplicationInsightsLogProvider doesn't correctly pick this up from appsettings,
                    //  so we need to do this:
                    logging.AddFilter<ApplicationInsightsLoggerProvider>("",
                        LogLevelFromString(Configuration["Logging:ApplicationInsights:LogLevel:Default"]));
                });

                builder.ConfigureServices(services =>
                {
                    // Application Insights.
                    services.AddApplicationInsightsTelemetryWorkerService();

                    services.AddSingleton<IConfiguration>(Configuration);
                    services.AddSingleton<IErrorLogger, ErrorLogger>();
                    services.AddSingleton<IAccountService, AccountService>();
                    services.AddSingleton<IContainerService, ContainerService>();
                });

                using (var host = builder.Build())
                {
                    var logger = host.Services.GetService<ILoggerFactory>().CreateLogger("ParcelPrepGov.HoneywellScanner.Listener");

                    var apiUrl = Configuration.GetSection("Api").GetValue<string>("Url");
                    logger.LogInformation($"Starting: Api:Url: {apiUrl}");
                    var apiHost = apiUrl.Substring("https://".Length).Trim('/');
                    int apiPort = 443;
                    var parts = apiHost.Split(':');
                    if (parts.Length > 1)
                    {
                        apiHost = parts[0];
                        apiPort = int.Parse(parts[1]);
                    }
                    if (TestEndpoint(apiHost, apiPort))
                        logger.LogInformation($"Connected to {apiHost}:{apiPort}");
                    else
                        logger.LogError($"Can't connect to {apiHost}:{apiPort}");


                    string ip = Configuration.GetSection("HoneywellScanner").GetValue<string>("Ip");
                    uint port = Configuration.GetSection("HoneywellScanner").GetValue<uint>("Port");
                    uint activityTimeout = Configuration.GetSection("HoneywellScanner").GetValue<uint>("ActivityTimeout");
                    if (port != 0)
                    {
                        Listener = new ScannerListener(host.Services, ip, port, activityTimeout);

                    }
                    await host.RunAsync(cancellationToken);
                }
            }
        }
    }
}
