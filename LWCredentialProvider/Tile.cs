// using System.Runtime.InteropServices;
// using System.Security;
// using System.IO;
// using System.Management;
// using Microsoft.Win32.SafeHandles;

// using Lithnet.CredentialProvider;

// namespace LWCredentialProvider
// {
//     public class MyTile : CredentialTile2
//     {
//         public MyTile(CredentialProviderBase credentialProvider) : base(credentialProvider)
//         {
//         }

//         public MyTile(CredentialProviderBase credentialProvider, CredentialProviderUser user) : base(credentialProvider, user)
//         {
//         }

//         public string Username;

//         protected override CredentialResponseBase GetCredentials()
//         {
//             Object[] usbData = IsUsbDeviceConnected();

//             if (!(bool)usbData[0])
//             //if (true)
//             {
//                 return new CredentialResponseInsecure()
//                 {
//                     IsSuccess = false,
//                     StatusText = DriveTesting(), // "Insert correct USB Drive" // DriveTesting()
//                     //Username = "aaaa"
//                 };
//             }
//             Username = (string)usbData[1];
//             // Password = (SecureString)usbData[2];
//             var spassword = new SecureString();
//             foreach (char c in (string)usbData[2]){
//                 spassword.AppendChar(c);
//             }

//             // Initialize();

//             string username;
//             string domain;

//             if (Username.Contains("\\"))
//             {
//                 domain = Username.Split('\\')[0];
//                 username = Username.Split('\\')[1];
//             }
//             else
//             {
//                 username = Username;
//                 domain = Environment.MachineName;
//             }

//             //var spassword = Controls.GetControl<SecurePasswordTextboxControl>("PasswordField").Password;

//             return new CredentialResponseSecure()
//             {
//                 IsSuccess = true,
//                 Password = spassword,
//                 Domain = domain,
//                 Username = username
//             };
//         }

//         private object[] IsUsbDeviceConnected()
//         {
//             var driveInfo = DriveInfo.GetDrives()
//                 .FirstOrDefault(drive => drive.DriveType == DriveType.Removable && !drive.IsReady);
            
//             if (driveInfo != null)
//             {
//                 List<string> drives = USBManager.GetUSBDrive();
//                 foreach (string drive in drives)
//                 {
//                     string usbData = USBManager.ReadUSBKey(drive, 41);

//                     if (!string.IsNullOrEmpty(usbData))
//                     {
//                         // return USBManager.CheckUSBKey(usbData);
//                         bool isValid = USBManager.CheckUSBKey(usbData);
//                         if (isValid)
//                         {
//                             // return true;

                            

//                             string data = "6603691B6B7EE37F3CA4195BC605F3BAB777457F43F91EB65CEFDCCAB07D070E";
//                             string key = "36DB0D6DAC8388994EF40DE6E5A32D9EE1DDBEE7C8D17C6D7D345CABBC5CA15C";
//                             string iv = "DD25843EB39A529F3FC235BC7C60B997";

//                             UserManager userManager = new UserManager();

//                             string userData = userManager.DecodeData(data, key, iv);

//                             string[] parts = userData.Split(new[] { UserManager.Separator }, StringSplitOptions.None);

//                             Object[] usbObject = [true, parts[0], parts[1]];
                            
//                             return usbObject;
//                         }
//                     }
//                 }
//             }

//             return [false];
//         }

//         private string DriveTesting()
//         {
//             DriveInfo[] allDrives = DriveInfo.GetDrives();
            
//             if (allDrives.Length > 0)
//             {
//                 //string drivesStr = "";

//                 // foreach (DriveInfo d in allDrives)
//                 // {
//                 //     //drivesStr += d.Name + "#" + d.DriveType + " ";


//                 // }

            

//                 List<string> drives = USBManager.GetUSBDrive();
//                 foreach (string drive in drives)
//                 {
//                     string usbData = USBManager.ReadUSBKey(drive, 41);

//                     if (!string.IsNullOrEmpty(usbData))
//                     {
//                         bool isValid = USBManager.CheckUSBKey(usbData);
//                         //return usbData + "#" + isValid;


//                         // string username = "admin";
//                         // string password = "zaq1@WSX";

//                         string data = "6603691B6B7EE37F3CA4195BC605F3BAB777457F43F91EB65CEFDCCAB07D070E";
//                         string key = "36DB0D6DAC8388994EF40DE6E5A32D9EE1DDBEE7C8D17C6D7D345CABBC5CA15C";
//                         string iv = "DD25843EB39A529F3FC235BC7C60B997";

//                         UserManager userManager = new UserManager();

//                         string userData =  userManager.DecodeData(data, key, iv);

//                         // return userData;

//                         // string userDataHex = userManager.EncodeData(username, password, key, iv);

//                         // Console.WriteLine($"userDataHex: {userDataHex}");

//                         // return userDataHex;


//                         string[] parts = userData.Split(new[] { UserManager.Separator }, StringSplitOptions.None);



//                         return parts[0] + " " + parts[1];
//                     }

                    

//                     //drivesStr += usbData + "#" + isValid + " ";
//                 }

//                 //return drivesStr;
//             }

//             return "none";
//         }
//         //USBManager.GetUSBDrive();

    
//     }
// }