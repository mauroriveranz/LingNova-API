using LingNova.Core.Entities;
using LingNova.Core.Interfaces;
using LingNova.Core.ViewModels;
using LingNova.Infreaestructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LingNova.Infreaestructure.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly LingNovaContext _context;
        private readonly IConfiguration _config;

        public UsersRepository(LingNovaContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }
        private string GenerateTempPassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
        public async Task<bool> ForgotPassword(ForgotPasswordVM forgotPasswordVM)
        {
            try
            {
                var password = _config["Email:EmailPass2"];
                Console.WriteLine($"PASSWORD: {password}");
                var user = await _context.Users.FirstOrDefaultAsync(x=>x.Email == forgotPasswordVM.Email && x.IsActive == true);
                
                if (user == null)
                {
                    throw new Exception("Este usuario no existe");
                }
                var tempPassword = GenerateTempPassword();
                user.Password = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                await _context.SaveChangesAsync();
                var smtpClient = new SmtpClient("smtp.titan.email")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("noreply@lingnova.mriveratech.com", password),
                    EnableSsl = true,
                    UseDefaultCredentials = false
                };
                var mensaje = new MailMessage
                {
                    From = new MailAddress("noreply@lingnova.mriveratech.com"),
                    Subject = $"Nuevo mensaje de noreply",
                    Body = $"Buen dia estimado usuario\n\nSu contraseña temporal es: {tempPassword}\n Porfavor usela para ingresar a la web y cambiar su contraseña\nNo la comparta con nadie.",
                    IsBodyHtml = false,
                };

                mensaje.To.Add(forgotPasswordVM.Email);

                await smtpClient.SendMailAsync(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
                
            }
        }

        public async Task<AuthResponseVM> Login(LoginVM loginVM)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginVM.Email && x.IsActive);
                if (user == null) {
                   throw new Exception("El usuario no existe");
                }
                
                if (!BCrypt.Net.BCrypt.Verify(loginVM.Password, user.Password)){
                    throw new Exception("La contraseña es incorrecta");
                }
                return new AuthResponseVM
                {
                    Token = GenerateJwt(user),
                    UserName = user.UserName,
                    Email = user.Email,
                    RoleId = user.RoleId
                };
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }


        }

        public async Task<AuthResponseVM> Register(RegisterVM registerVM)
        {
            if (await _context.Users.AnyAsync(x => x.Email == registerVM.Email))
                throw new Exception("Este correo ya esta registrado");

            var user = new User
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerVM.Password),
                RoleId = registerVM.RoleId,
                DateCreated = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseVM
            {
                Token = GenerateJwt(user),
                UserName = user.UserName,
                Email = user.Email,
                RoleId = user.RoleId
            };


        }

        public async Task<bool> sendEmail(EmailVM email)
        {
            var password = _config["Email:EmailPass"];
            Console.WriteLine($"PASSWORD: {password}");
            Console.WriteLine(password);
            try
            {
                
                if (email == null)
                {
                    throw new Exception("Debe llenar todos los campos");
                }
                var smtpClient = new SmtpClient("smtp.titan.email")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("contacto@lingnova.mriveratech.com", password),
                    EnableSsl = true,
                    UseDefaultCredentials = false
                };
                var mensaje = new MailMessage
                {
                    From = new MailAddress("contacto@lingnova.mriveratech.com"),
                    Subject = $"{email.subject}",
                    Body = $"Nombre: {email.Name}\nEmail: {email.Email}\nMensaje:\n{email.Message}",
                    IsBodyHtml = false,
                };

                mensaje.To.Add("info@lingnova.mriveratech.com");

                 smtpClient.Send(mensaje);
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<AuthResponseVM> Update(UpdateUserVM updateVM)
        {
            try
            {


            if (updateVM == null)
                throw new Exception("Debe llenar todos los campos");
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == updateVM.Email && x.IsActive);

            if (user == null)
                throw new Exception("El usuario no existe");

            if (string.IsNullOrEmpty(user.Password) || !user.Password.StartsWith("$2"))
            {
                // No es un hash bcrypt válido
                throw new Exception("No es un hash valido");
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(updateVM.Password, user.Password);

            if (passwordOk)
                throw new Exception("No puede usar la misma contraseña que tenia");


            user.UserName = updateVM.UserName;
            user.RoleId = updateVM.RoleId;
            user.DateModify = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(updateVM.NewPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(updateVM.NewPassword);
            }
            await _context.SaveChangesAsync();

            return new AuthResponseVM
            {
                Token = GenerateJwt(user),
                UserName = user.UserName,
                Email = user.Email,
                RoleId = user.RoleId
            };
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: new[] {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name,  user.UserName)
                },
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
