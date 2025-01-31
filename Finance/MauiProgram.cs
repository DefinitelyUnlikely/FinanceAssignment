﻿using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Finance.ViewModels;
using Finance.Views;
using Finance.Data.Database;
using Finance.Data.Interfaces;
using Finance.Data.Repositories;
using Finance.Utilities;

namespace Finance
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiCommunityToolkit();

            builder.Services.AddSingleton<IFinanceDatabase, PostgresDatabase>();
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<IPasswordUtilities, PasswordUtilities>();
            builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
            builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();

            builder.Services.AddSingleton<MainView>();
            builder.Services.AddSingleton<MainViewModel>();

            builder.Services.AddTransient<CreateAccView>();
            builder.Services.AddTransient<CreateAccViewModel>();

            builder.Services.AddTransient<AccountView>();
            builder.Services.AddTransient<AccountViewModel>();

            builder.Services.AddTransient<TransactionView>();
            builder.Services.AddTransient<TransactionViewModel>();


            builder.Services.AddTransient<IncomeView>();
            builder.Services.AddTransient<IncomeViewModel>();

            builder.Services.AddTransient<ExpenseView>();
            builder.Services.AddTransient<ExpenseViewModel>();

            builder.Services.AddTransient<SortView>();
            builder.Services.AddTransient<SortViewModel>();
            builder.Services.AddTransient<FilterView>();
            builder.Services.AddTransient<FilterViewModel>();

            builder.Services.AddTransientPopup<PasswordPopup, PasswordPopupViewModel>();
            builder.Services.AddTransientPopup<UsernamePopup, UsernamePopupViewModel>();
            builder.Services.AddTransientPopup<AccountPopup, AccountPopupViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
