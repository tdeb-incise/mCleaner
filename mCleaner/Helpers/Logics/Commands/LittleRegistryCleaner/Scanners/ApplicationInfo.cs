﻿using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

namespace mCleaner.Logics.Commands.LittleRegistryCleaner.Scanners
{
    public class ApplicationInfo : ScannerBase
    {
        public ApplicationInfo()
        {
            
        }
        static ApplicationInfo _i = new ApplicationInfo();
        public static ApplicationInfo I { get { return _i; } }


        public override void Preview()
        {
            this.BadKeys.Clear();

            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall"))
            {
                if (regKey == null)
                    return;

                foreach (string strProgName in regKey.GetSubKeyNames())
                {
                    using (RegistryKey regKey2 = regKey.OpenSubKey(strProgName, true))
                    {
                        if (regKey2 != null)
                        {
                            ProgressWorker.I.EnQ("Scanning " + regKey2.ToString());

                            // check entries if it has "DisplayName"
                            // without it, it's not a valid UninstallKey

                            string[] ValueNames = regKey2.GetValueNames();
                            if (!ValueNames.Contains("DisplayName"))
                            {
                                this.BadKeys.Add(new InvalidKeys()
                                {
                                    Root = Registry.LocalMachine,
                                    Subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall",
                                    Key = strProgName
                                });
                            }
                        }
                    }
                }
            }

            // didn't have these keys in my local machine
            // TODO: What are these?
            //checkARPCache(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Management\ARPCache\"));
            //checkARPCache(Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Management\ARPCache\"));
        }

        public override void Clean()
        {
            try
            {
                Preview();

                foreach (InvalidKeys badkey in this.BadKeys)
                {
                    try
                    {
                        using (RegistryKey root = badkey.Root.OpenSubKey(badkey.Subkey, true))
                        {
                            if (root != null)
                            {
                                BackUpRegistrykey(badkey);
                                root.DeleteSubKey(badkey.Key);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        
    }
}
