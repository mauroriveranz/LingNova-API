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

        public async Task<AuthResponseVM> Login(LoginVM loginVM)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x=>x.Email == loginVM.Email && x.IsActive);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginVM.Password, user.Password))
                return null;

            return new AuthResponseVM
            {
                Token = GenerateJwt(user),
                UserName = user.UserName,
                Email = user.Email,
                RoleId = user.RoleId
            };

        }

        public async Task<AuthResponseVM> Register(RegisterVM registerVM)
        {
            if (await _context.Users.AnyAsync(x=>x.Email == registerVM.Email))
                return null;

            var user = new User
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerVM.Password),
                RoleId = registerVM.RoleId,
                DateCreated = DateTime.UtcNow,
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

        public async Task<AuthResponseVM> Update(UpdateUserVM updateVM)
        {
            if (updateVM == null)
                return null;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == updateVM.Email && x.IsActive);

            if (user == null)
                return null;

            bool passwordOk;
            try
            {
                passwordOk = BCrypt.Net.BCrypt.Verify(updateVM.Password, user.Password);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // el password guardado en BD no tiene formato bcrypt válido
                return null;

            }
            if (!passwordOk)
                return null;


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
