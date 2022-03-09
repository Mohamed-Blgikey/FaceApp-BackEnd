using FaceApp.BL.Dtos;
using FaceApp.BL.Helper;
using FaceApp.DAL.Extend;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
 
namespace FaceApp.BL.services
{
    public class Authservice : IAuthservice
    {
        private readonly UserManager<User> userManager;
        private readonly IOptions<JWT> jwt;
        private readonly RoleManager<IdentityRole> roleManager;

        public Authservice(UserManager<User> userManager,IOptions<JWT> jwt,RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.jwt = jwt;
            this.roleManager = roleManager;
        }
        public Task<AuthModel> Login()
        {
            throw new NotImplementedException();
        }

        public async Task<AuthModel> Register(RegisterDTO registerDTO)
        {
            try
            {
                if (await userManager.FindByEmailAsync(registerDTO.Email) != null)
                    return new AuthModel { Message = "Email already exsit" };
                var user = new User
                {
                    Email = registerDTO.Email,
                    UserName = registerDTO.Email,
                    PhoneNumber = registerDTO.PhoneNumber,
                    BirthDate = registerDTO.BirthDate,
                    FirstName = registerDTO.FirstName,
                    LastName = registerDTO.LastName,
                    Gender = registerDTO.Gender
                };

                var result = await userManager.CreateAsync(user, registerDTO.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Empty;
                    foreach (var error in result.Errors)
                    {
                        errors += $"{error} ,";
                    }
                    return new AuthModel { Message = errors };
                }

                if (!await roleManager.RoleExistsAsync("admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("admin"));
                    await userManager.AddToRoleAsync(user, "admin");
                }
                else
                {
                    await roleManager.CreateAsync(new IdentityRole("user"));
                    await userManager.AddToRoleAsync(user, "user");
                }

                var token = CreateToken(user);
                return new AuthModel { Message = "success", Token = token, IsAuthencated = true };
            }
            catch (Exception e)
            {
                return new AuthModel { Message = "Phone is exsit"};
            }

        }





















        private string CreateToken(User user)
        {
            var userClaims =  userManager.GetClaimsAsync(user).Result;
            var userRoles =  userManager.GetRolesAsync(user).Result;
            var roleClaims = new List<Claim>();
            foreach (var role in userRoles)
            {
                roleClaims.Add(new Claim("Roles", role));
            }
            var claims = new []
            {
                new Claim(JwtRegisteredClaimNames.Jti ,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName,user.FullName),
                new Claim(JwtRegisteredClaimNames.NameId,user.Id),
            }
            .Union(userClaims)
            .Union(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwt.Value.Key));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var JwtSecurityToken = new JwtSecurityToken(
                issuer: jwt.Value.Issuer,
                audience:jwt.Value.Audience,
                claims:claims,
                expires:DateTime.Now.AddDays(jwt.Value.DurationInDays),
                signingCredentials:cred
                );

            var token = new JwtSecurityTokenHandler().WriteToken(JwtSecurityToken);

            return token;


        }
    }
}
