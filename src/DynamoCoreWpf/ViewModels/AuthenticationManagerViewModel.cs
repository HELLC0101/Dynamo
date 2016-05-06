using System.Windows.Input;
using Dynamo.Core;
using Greg.AuthProviders;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.ViewModels
{
    public class AuthenticationManagerViewModel : NotificationObject
    {
        private AuthenticationManager authManager;

        public ICommand ToggleLoginStateCommand { get; private set; }

        public LoginState LoginState
        {
            get
            {
                return authManager.LoginState;
            }
        }

        public string Username
        {
            get
            {
                return authManager.Username;
            }
        }

        public AuthenticationManagerViewModel(AuthenticationManager authManager)
        {
            this.authManager = authManager;
            this.ToggleLoginStateCommand = new DelegateCommand(ToggleLoginState, CanToggleLoginState);

            authManager.LoginStateChanged += (loginState) =>
            {
                RaisePropertyChanged("LoginState");
                RaisePropertyChanged("Username");
            };
        }

        private void ToggleLoginState()
        {
            if (authManager.LoginState == LoginState.LoggedIn)
            {
                authManager.Logout();
            }
            else
            {
                authManager.Login();
            }
        }

        private bool CanToggleLoginState()
        {
            return authManager.LoginState == LoginState.LoggedOut || authManager.LoginState == LoginState.LoggedIn;
        }
    }
}
