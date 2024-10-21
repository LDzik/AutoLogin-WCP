using Lithnet.CredentialProvider;
using System.Runtime.InteropServices;
using System.Security;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("TestCredentialProvider")]
[Guid("00000000-0000-0000-0000-000000000000")]
public class TestCredentialProvider : CredentialProviderBase
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
        yield return new CredentialProviderLabelControl("CredProviderLabel", "My first credential provider");

        var infoLabel = new SmallLabelControl("InfoLabel", "Enter your username and password please!");
        infoLabel.State = FieldState.DisplayInSelectedTile;
        yield return infoLabel;

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
}