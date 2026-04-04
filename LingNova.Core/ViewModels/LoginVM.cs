using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingNova.Core.ViewModels
{
    public class LoginVM
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; }= string.Empty;
    }

    public class RegisterVM
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty ;

        public int RoleId { get; set; }

        public string Password { get; set; }= string.Empty;

        public DateTime DateCreated { get; set; }

    }

    public class AuthResponseVM
    {
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty ;
        public int RoleId { get; set; }
    }

    public class UpdateUserVM
    {
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty ;

        public string Password { get; set; } = string.Empty;

        public string  NewPassword { get; set; } = string.Empty;

        public DateTime DateModify { get; set; }

        public int RoleId { get; set; }
    }

    public class ForgotPasswordVM
    {
        public string Email { get; set; } = string.Empty ;
    }

}
