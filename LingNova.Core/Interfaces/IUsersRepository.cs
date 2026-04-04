using LingNova.Core.Entities;
using LingNova.Core.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingNova.Core.Interfaces
{
    public interface IUsersRepository
    {
        Task<AuthResponseVM> Login(LoginVM loginVM);

        Task<AuthResponseVM> Register(RegisterVM registerVM);

        Task<AuthResponseVM> Update(UpdateUserVM updateVM);
        Task<bool> sendEmail(EmailVM email);

        Task<bool> ForgotPassword(ForgotPasswordVM forgotPasswordVM);
    }
}
