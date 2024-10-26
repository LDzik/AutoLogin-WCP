// using System;
// using System.Collections.Generic;
// using System.Drawing;
// using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using Microsoft.Win32.SafeHandles;

using Lithnet.CredentialProvider;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("MyCredentialProvider")]
[Guid("5ADA2295-6DE3-4267-9ED3-D1ED40108E9C")]
public class MyCredentialProvider : CredentialProviderBase
{
    public override bool IsUsageScenarioSupported(UsageScenario cpus, CredUIWinFlags dwFlags)
    {
        switch (cpus)
        {
            case UsageScenario.Logon:
            case UsageScenario.UnlockWorkstation:
            case UsageScenario.CredUI:
            case UsageScenario.ChangePassword:
                return true;

            default:
                return false;
        }
    }

    public override IEnumerable<ControlBase> GetControls(UsageScenario cpus)
    {

        // var tileImage = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("MyCredentialProvider.Resources.TileIcon.png"));
        // var userImage = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("MyCredentialProvider.Resources.UserIcon.png"));

        // yield return new CredentialProviderLogoControl("ImageCredentialProvider", "Credential provider logo", tileImage);
        // yield return new CredentialProviderLogoControl("UserTileImage", "User tile image", userImage);

        yield return new CredentialProviderLabelControl("CredProviderLabel", "My first credential provider");

        var infoLabel = new SmallLabelControl("InfoLabel", "Enter your username and password please!");
        infoLabel.State = FieldState.DisplayInSelectedTile;
        yield return infoLabel;


        // yield return new CheckboxControl("Checkbox", "A checkbox");
        // yield return new SmallLabelControl("LabelCheckbox", "The check box is currently unchecked");
        // yield return new CommandLinkControl("CommandLinkCheckbox", "Click this link to change the check box value in code behind");

        // yield return new ComboboxControl("Combobox", "Items to choose from:");
        // yield return new SmallLabelControl("ComboSelectedItemLabel", "This is the currently selected item: <none>");
        // yield return new CommandLinkControl("CommandLinkComboboxAdd", "Add a random item to the combo box");
        // yield return new CommandLinkControl("CommandLinkComboboxRemove", "Remove the last item from the combo box");



        yield return new TextboxControl("UsernameField", "Username");
        var password = new SecurePasswordTextboxControl("PasswordField", "Password");
        yield return password;

        if (cpus == UsageScenario.ChangePassword)
        {
            var confirmPassword = new SecurePasswordTextboxControl("ConfirmPasswordField", "Confirm password");
            yield return confirmPassword;
            yield return new SubmitButtonControl("SubmitButton", "Submit", confirmPassword);
        }
        else
        {
            yield return new SubmitButtonControl("SubmitButton", "Submit", password);
        }
    }

    public override bool ShouldIncludeUserTile(CredentialProviderUser user)
    {
        return true;
    }

    public override bool ShouldIncludeGenericTile()
    {
        return true;
    }

    public override CredentialTile CreateGenericTile()
    {
        return new MyTile(this);
    }

    public override CredentialTile2 CreateUserTile(CredentialProviderUser user)
    {
        return new MyTile(this, user);
    }
}

public class MyTile : CredentialTile2
{
    private TextboxControl UsernameControl;
    private SecurePasswordTextboxControl PasswordControl;
    private SecurePasswordTextboxControl PasswordConfirmControl;

    public MyTile(CredentialProviderBase credentialProvider) : base(credentialProvider)
    {
    }

    public MyTile(CredentialProviderBase credentialProvider, CredentialProviderUser user) : base(credentialProvider, user)
    {
    }

    public string Username
    {
        get => UsernameControl.Text;
        set => UsernameControl.Text = value;
    }

    public SecureString Password
    {
        get => PasswordControl.Password;
        set => PasswordControl.Password = value;
    }

    public SecureString ConfirmPassword
    {
        get => PasswordConfirmControl.Password;
        set => PasswordConfirmControl.Password = value;
    }

    public override void Initialize()
    {
        if (UsageScenario == UsageScenario.ChangePassword)
        {
            this.PasswordConfirmControl = this.Controls.GetControl<SecurePasswordTextboxControl>("ConfirmPasswordField");
        }

        this.PasswordControl = this.Controls.GetControl<SecurePasswordTextboxControl>("PasswordField");
        this.UsernameControl = this.Controls.GetControl<TextboxControl>("UsernameField");

        Username = this.User?.QualifiedUserName;
    }

    protected override CredentialResponseBase GetCredentials()
    {

        if (!IsUsbDeviceConnected())
        //if (true)
        {
            return new CredentialResponseInsecure()
            {
                IsSuccess = false,
                StatusText = "Insert correct USB Drive" // DriveTesting()
            };
        }

        string username;
        string domain;

        if (Username.Contains("\\"))
        {
            domain = Username.Split('\\')[0];
            username = Username.Split('\\')[1];
        }
        else
        {
            username = Username;
            domain = Environment.MachineName;
        }

        var spassword = Controls.GetControl<SecurePasswordTextboxControl>("PasswordField").Password;

        return new CredentialResponseSecure()
        {
            IsSuccess = true,
            Password = spassword,
            Domain = domain,
            Username = username
        };
    }

    private bool IsUsbDeviceConnected()
    {
        var driveInfo = DriveInfo.GetDrives()
            .FirstOrDefault(drive => drive.DriveType == DriveType.Removable && !drive.IsReady);
        
        if (driveInfo != null)
        {
            List<string> drives = USBManager.GetUSBDrive();
            foreach (string drive in drives)
            {
                string usbData = USBManager.ReadUSBKey(drive, 41);

                if (!string.IsNullOrEmpty(usbData))
                {
                    // return USBManager.CheckUSBKey(usbData);
                    bool isValid = USBManager.CheckUSBKey(usbData);
                    if (isValid)
                    {
                        return true;

                        string data = "6603691B6B7EE37F3CA4195BC605F3BAB777457F43F91EB65CEFDCCAB07D070E";
                        string key = "36DB0D6DAC8388994EF40DE6E5A32D9EE1DDBEE7C8D17C6D7D345CABBC5CA15C";
                        string iv = "DD25843EB39A529F3FC235BC7C60B997";

                        UserManager userManager = new UserManager();

                        string userData =  userManager.DecodeData(data, key, iv);

                        string[] parts = userData.Split(new[] { UserManager.Separator }, StringSplitOptions.None);

                        //return parts;
                    }
                }
            }
        }

        return false;
    }

     private string DriveTesting()
    {
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        
        if (allDrives.Length > 0)
        {
            string drivesStr = "";

            // foreach (DriveInfo d in allDrives)
            // {
            //     //drivesStr += d.Name + "#" + d.DriveType + " ";


            // }

           

            List<string> drives = USBManager.GetUSBDrive();
            foreach (string drive in drives)
            {
                string usbData = USBManager.ReadUSBKey(drive, 41);

                if (!string.IsNullOrEmpty(usbData))
                {
                    bool isValid = USBManager.CheckUSBKey(usbData);
                    //return usbData + "#" + isValid;


                    string username = "admin";
                    string password = "zaq1@WSX";

                    string data = "6603691B6B7EE37F3CA4195BC605F3BAB777457F43F91EB65CEFDCCAB07D070E";
                    string key = "36DB0D6DAC8388994EF40DE6E5A32D9EE1DDBEE7C8D17C6D7D345CABBC5CA15C";
                    string iv = "DD25843EB39A529F3FC235BC7C60B997";

                    UserManager userManager = new UserManager();

                    string userData =  userManager.DecodeData(data, key, iv);

                    // return userData;

                    // string userDataHex = userManager.EncodeData(username, password, key, iv);

                    // Console.WriteLine($"userDataHex: {userDataHex}");

                    // return userDataHex;


                    string[] parts = userData.Split(new[] { UserManager.Separator }, StringSplitOptions.None);



                    return parts[0] + " " + parts[1];
                }

                

                //drivesStr += usbData + "#" + isValid + " ";
            }

            //return drivesStr;
        }

        return "none";
    }
    //USBManager.GetUSBDrive();

   
}