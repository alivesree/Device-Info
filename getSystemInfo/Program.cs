using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace getSystemInfo
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Genrated System ID      :   "+getUniqueID("C"));

            Console.WriteLine("IP Address              :   " + GetAllLocalIPv4());
            Console.WriteLine("Computer Name           :   "+GetMachineName());
            Console.WriteLine("Computer Description    :   "+GetComputerDescription());
            //Console.WriteLine("Lan MAC    :   " + GetMacAddress().Item1);
            //Console.WriteLine("WLAN MAC    :   " + GetMacAddress().Item2);
            Console.WriteLine("\nCurrent connected MAC    :   " + GetMacAddress().Item3);
            Console.WriteLine("\nInstalled Printers       => \n" +string.Join("\t\n", GetAllPrinterInstalled()));
  

            Console.ReadLine();
        }
        public static Tuple<string, string, string> GetMacAddress()
        {
            string lanMAC = "", wlanMAC = "", currentMAC = "";
            Console.WriteLine("\nMAC Address   =>   ");
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if(!string.IsNullOrWhiteSpace(nic.GetPhysicalAddress().ToString()))
                Console.WriteLine(nic.Name+"    :" + nic.GetPhysicalAddress());
                //// Only consider Ethernet network interfaces
                //if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                //    lanMAC = nic.GetPhysicalAddress().ToString();
                //// Only consider WIFI network interfaces
                //if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                //    wlanMAC = nic.GetPhysicalAddress().ToString();
                if (nic.OperationalStatus == OperationalStatus.Up && (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet || nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    currentMAC = nic.GetPhysicalAddress().ToString();
            }
            return new Tuple<string, string, string>(lanMAC, wlanMAC, currentMAC);
        }
        public static List<string> GetAllPrinterInstalled()
        {
            List<string> p = new List<string>();
            foreach (var item in PrinterSettings.InstalledPrinters)
                p.Add(item.ToString());
            return p;
        }
        public static string GetAllLocalIPv4()
        {
            string ipAddrList = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList += ip.Address.ToString() + ",";
                        }
                    }
                }
            }
            if (ipAddrList.Length > 0)
                return ipAddrList.Substring(0, ipAddrList.Length - 1);
            return ipAddrList;
        }
        public static string GetMachineName()
        {
            return Environment.MachineName;
        }

        public static string GetComputerDescription()
        {
            string key = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lanmanserver\parameters";
            return (string)Registry.GetValue(key, "srvcomment", null);
        }
        public static string getUniqueID(string drive)
        {
            // return"678FEBFBEDF1AFDFBFF";
            if (drive == string.Empty || drive == null)
            {
                //Find first drive
                foreach (DriveInfo compDrive in DriveInfo.GetDrives())
                {
                    if (compDrive.IsReady)
                    {
                        drive = compDrive.RootDirectory.ToString();
                        break;
                    }
                }
            }

            if (drive.EndsWith(":\\"))
            {
                //C:\ -> C
                drive = drive.Substring(0, drive.Length - 2);
            }

            string volumeSerial = getVolumeSerial(drive);
            string cpuID = getCPUID();

            //Mix them up and remove some useless 0's
            return cpuID.Substring(13) + cpuID.Substring(1, 4) + volumeSerial + cpuID.Substring(4, 4);
        }


        private static string getVolumeSerial(string drive)
        {
            ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            disk.Get();

            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            return volumeSerial;
        }
        private static string getCPUID()
        {
            string cpuInfo = "";

            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();

            foreach (ManagementObject managObj in managCollec)
            {
                if (cpuInfo == "")
                {
                    //Get only the first CPU's ID
                    cpuInfo = managObj.Properties["processorID"].Value.ToString();
                    break;
                }
            }

            return cpuInfo;
        }
    }
}
