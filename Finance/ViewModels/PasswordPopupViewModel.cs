using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finance.Managers;
using Finance.Utilities;

namespace Finance.ViewModels;

public partial class PasswordPopupViewModel : ObservableObject
{
    [ObservableProperty]
    string currentPassword = string.Empty;
    [ObservableProperty]
    string password = string.Empty;

    [ObservableProperty]
    string rePassword = string.Empty;

    [RelayCommand]
    async Task ChangePassword()
    {
        if (CurrentPassword is "" || Password is "" || RePassword is "")
        {
            CurrentPassword = string.Empty;
            Password = string.Empty;
            RePassword = string.Empty;
            await Shell.Current.DisplayAlert("Entry error", "Please enter all fields", "OK");
            return;
        }

        if (!UserManager.CurrentUser!.Name.VerifyPassword(CurrentPassword))
        {
            CurrentPassword = string.Empty;
            Password = string.Empty;
            RePassword = string.Empty;
            await Shell.Current.DisplayAlert("Password error", "Wrong Password", "OK");
            return;
        }
        if (!Password.Equals(RePassword))
        {
            CurrentPassword = string.Empty;
            Password = string.Empty;
            RePassword = string.Empty;
            await Shell.Current.DisplayAlert("Password error", "Passwords must match", "OK");
            return;
        }

        if (!Password.IsValidPassword())
        {
            CurrentPassword = string.Empty;
            Password = string.Empty;
            RePassword = string.Empty;
            await Shell.Current.DisplayAlert("Password error", "Passwords must be at least 8 characters", "OK");
            return;
        }

        (UserManager.CurrentUser!.Salt, UserManager.CurrentUser!.PasswordHash) = Password.SaltAndHash();
    }
}