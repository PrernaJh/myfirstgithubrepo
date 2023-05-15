using System;
using System.Collections.Generic;
using System.Text;
using SPS.PP.ESM.WiFi.Decoding;

namespace HoneywellScanner
{
    // MENU Header
    public enum MenuHeader
    {
        // Unknown Header
        EMH_UNKNOWN = -1,
        // [SYN] M
        EMH_M,
        // [SYN] Y
        EMH_Y,
        // None header
        EMH_NONE,
    };

    // MENU terminator
    public enum MenuTerminator
    {
        // Unknown terminator
        EMT_UNKNOWN = -1,
        // !
        EMT_RAM,
        // .
        EMT_ROM,
        // @
        EMT_CUSTOM_DEFAULT,
        // None terminator
        EMT_NONE,
    };

    public class WifiScanner
    {
        public ScannerInfo_WiFi info = new ScannerInfo_WiFi();
        public string nickName;
    };

    public class SDKUtility
    {
        private static string[] sdkResultMsgs = new string[]
        {
            "Not Initilized",
            "Success",
            "WiFi service was not started",
            "WiFi service was already started",
            "An invalid parameter was specified.",
            "Function unsupported.",
            "An exception has occured in decode library",
            "SDK is busy with flashing firmware",
            "The specified firmware is not found"
        };

        // symbol code settings
        private const string enableCode = "enable code";
        private const string minimumLength = "minimum length";
        private const string maximumLength = "maximum length";
        private const string enableAppend = "enable append";
        private const string enableCheckDigitTransmit = "enable check digit transmit";
        private const string enableStartStopTransmit = "enable start/stop transmit";
        private const string enable2CharAddenda = "enable 2 char addenda";
        private const string enable5CharAddenda = "enable 5 char addenda";
        private const string enableAddendaRequired = "enable addenda required";
        private const string enableAddendaSeparator = "enable addenda separator";
        private const string enableCheckDigitMode = "enable check digit mode";
        private const string enableFullAscii = "enable full Ascii";
        private const string enableBase32 = "enable base 32";
        private const string enableConcat = "enable concat";

        private const string CMD_SYN_M = "\x16\x4d\x0d"; // SYN M CR
        private const string CMD_SYN_Y = "\x16\x59\x0d"; // SYN Y CR
        private const string CMD_RAM = "\x21"; // !
        private const string CMD_ROM = "\x2e"; // .

        public List<SymbolCode> SymbolCodeList;

        public SDKUtility()
        {
            SymbolCodeList = new List<SymbolCode>();
        }

        public void SetupSymbolCodes()
        {
            //EAN-8
            SymbolCode ean8 = new SymbolCode("EAN-8");
            ean8.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)RetailCodes.EAN_8_Enabled,
                    enableCode,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            ean8.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)RetailCodes.EAN_8_ChKDigitXmit,
                    enableCheckDigitTransmit,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            ean8.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)RetailCodes.EAN_8_2DigitAddendaEnabled,
                    enable2CharAddenda,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            ean8.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)RetailCodes.EAN_8_5DigitAddendaEnabled,
                    enable5CharAddenda,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            ean8.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)RetailCodes.EAN_8_AddendaReq,
                    enableAddendaRequired,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            ean8.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)RetailCodes.EAN_8_AddendaSeparator,
                    enableAddendaSeparator,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            SymbolCodeList.Add(ean8);

            //Code 128
            SymbolCode code128 = new SymbolCode("Code 128");
            code128.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code128Enabled,
                    enableCode,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            code128.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code128MinLength,
                    minimumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            code128.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code128MaxLength,
                    maximumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            //code128.Properties.Add(
            //    new SymbolCodeProperty(
            //        (UInt32)LinearCodes.Code128AppendEnabled,
            //        enableAppend,
            //        SymbolCodeProperty.PropertyType.EnableProperty));
            SymbolCodeList.Add(code128);

            //Code 39
            SymbolCode code39 = new SymbolCode("Code 39");
            code39.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code39Enabled,
                    enableCode,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            code39.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code39MinLength,
                    minimumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            code39.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code39MaxLength,
                    maximumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            code39.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code39CheckDigitMode,
                    enableCheckDigitMode,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            code39.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code39FullAsciiEnable,
                    enableFullAscii,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            code39.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code39StartStopTransmit,
                    enableStartStopTransmit,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            code39.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code39Base32Enabled,
                    enableBase32,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            SymbolCodeList.Add(code39);

            //Standard 2 of 5 (3 bar) - Industrial 2 of 5
            SymbolCode industrial2_5 = new SymbolCode("Standard(Industrial) 2 of 5");
            industrial2_5.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Standard25Enabled,
                    enableCode,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            industrial2_5.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Standard25MinLength,
                    minimumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            industrial2_5.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Standard25MaxLength,
                    maximumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            SymbolCodeList.Add(industrial2_5);

            //Code 93
            SymbolCode code93 = new SymbolCode("Code 93");
            code93.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code93Enabled,
                    enableCode,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            code93.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code93MinLength,
                    minimumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            code93.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.Code93MaxLength,
                    maximumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            //code93.Properties.Add(
            //    new SymbolCodeProperty(
            //        (UInt32)LinearCodes.Code93AppendEnable,
            //        enableAppend,
            //        SymbolCodeProperty.PropertyType.EnableProperty));
            SymbolCodeList.Add(code93);

            //Codabar
            SymbolCode codabar = new SymbolCode("Codabar");
            codabar.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.CodabarEnabled,
                    enableCode,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            codabar.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.CodabarMinLength,
                    minimumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            codabar.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.CodabarMaxLength,
                    maximumLength,
                    SymbolCodeProperty.PropertyType.MinMaxProperty));
            codabar.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.CodabarStartStopXmit,
                    enableStartStopTransmit,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            codabar.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.CodabarCheckDigitMode,
                    enableCheckDigitMode,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            codabar.Properties.Add(
                new SymbolCodeProperty(
                    (UInt32)LinearCodes.CodabarConcatEnable,
                    enableConcat,
                    SymbolCodeProperty.PropertyType.EnableProperty));
            SymbolCodeList.Add(codabar);
        }

        public string DecodeResultToString(DecodeResult_WiFi res, string serialNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine().AppendFormat("Scanner [{0}][{1}]", res.ConnID, serialNum).AppendLine()
            .Append("[Decode Result]").AppendLine()
            .AppendFormat("Code ID: {0}", res.CodeID).AppendLine()
            .AppendFormat("Aim ID: ]{0}{1}", res.AimID, res.AimModifier).AppendLine()
            .AppendFormat("Length: {0}", res.Length).AppendLine()
            .AppendFormat("Decode data: {0}", res.Message).AppendLine().AppendLine();

            return sb.ToString();
        }

        public string ButtonPressResultToString(ButtonPressNotify nfy, string serialNum)
        {
            string button = string.Empty;
            switch (nfy.WhichButtonPressed)
            {
                case ButtonPressFlag_WiFi.LeftButtonPressed_WiFi:
                    button = "Left button is ";
                    break;
                case ButtonPressFlag_WiFi.RightButtonPressed_WiFi:
                    button = "Right button is ";
                    break;
                case ButtonPressFlag_WiFi.BothButtonsPressed_WiFi:
                    button = "Left and right buttons are ";
                    break;
                case ButtonPressFlag_WiFi.NoButtonPressed_WiFi:
                default:
                    button = "No button is ";
                    break;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine().AppendFormat("Scanner[{0}][{1}]", nfy.ConnID, serialNum).AppendLine()
                .Append(button).Append("pressed").AppendLine();

            return sb.ToString();
        }

        public bool GetSymbolCodeDesc(uint propID, out string codeDesc, out string propDesc)
        {
            foreach (SymbolCode code in SymbolCodeList)
            {
                foreach (SymbolCodeProperty prop in code.Properties)
                {
                    if (prop.ID == propID)
                    {
                        codeDesc = code.Description;
                        propDesc = prop.Description;
                        return true;
                    }
                }
            }

            codeDesc = "";
            propDesc = "";
            return false;
        }

        public string SymbPropResponseToString(SymbPropResponse resp, string serialNum)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine().AppendFormat("Scanner[{0}][{1}]", resp.ConnID, serialNum).AppendLine();

            string codeDesc, propDesc;
            if (resp.Successful && GetSymbolCodeDesc(resp.PropID, out codeDesc, out propDesc))
            {
                sb.AppendFormat("Symbol Code: {0}", codeDesc).AppendLine()
                    .AppendFormat("Property: {0}", propDesc).AppendLine()
                    .AppendFormat("Value: {0}", resp.Value).AppendLine();
            }
            else
                sb.Append("Failed").AppendLine();

            return sb.ToString();
        }

        public string APIResultToString(ResultCode_WiFi res)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[SDK API Invoke Result] ")
              .Append(sdkResultMsgs[(int)res + 1]);

            return sb.ToString();
        }

        public string ApiInvokeMessage(string functionName, bool start = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("********************* {0} invoke {1} *********************",
                functionName, (start ? "start" : "end"));
            return sb.ToString();
        }

        public bool IsEmptyIpMask(string ip)
        {
            return string.IsNullOrWhiteSpace(ip) || ip == "...";
        }

        public string BuildCmd(string cmd, MenuHeader header, MenuTerminator term)
        {
            string cmdHeader = string.Empty;
            string cmdTerm = string.Empty;
            string cmdText = string.Empty;

            switch (header)
            {
                case MenuHeader.EMH_M:
                    cmdHeader = CMD_SYN_M;
                    break;
                case MenuHeader.EMH_Y:
                    cmdHeader = CMD_SYN_Y;
                    break;
            }

            switch (term)
            {
                case MenuTerminator.EMT_RAM:
                    cmdTerm = CMD_RAM;
                    break;
                case MenuTerminator.EMT_ROM:
                    cmdTerm = CMD_ROM;
                    break;
            }

            cmdText = cmdHeader + cmd + cmdTerm;

            if (header == MenuHeader.EMH_NONE &&
                term == MenuTerminator.EMT_CUSTOM_DEFAULT) // raw cmd text
            {
                string asciiCmd = string.Empty;
                var cmdList = cmdText.Split(new string[] { @"\x", @"\X" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < cmdList.Length; ++i)
                {
                    if (cmdList[i].Length > 2)
                    {
                        asciiCmd += HexToAscii(cmdList[i].Substring(0, 2));
                        asciiCmd += cmdList[i].Substring(2);
                    }
                    else
                        asciiCmd += HexToAscii(cmdList[i]);
                }

                cmdText = asciiCmd;
            }
            return cmdText;
        }

        private string HexToAscii(string hexString)
        {
            try
            {
                string ascii = string.Empty;
                uint decval = Convert.ToUInt32(hexString, 16);
                char character = Convert.ToChar(decval);
                ascii += character;

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }
    }
}

