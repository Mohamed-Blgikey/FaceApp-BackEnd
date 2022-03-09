using FaceApp.BL.Dtos;
using FaceApp.BL.Helper;
using FaceApp.DAL.Database;
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
        #region fields
        private readonly UserManager<User> userManager;
        private readonly IOptions<JWT> jwt;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly FaceAppContext context;

        public RoleManager<IdentityRole> RoleManager => roleManager;
        #endregion

        #region Ctor
        public Authservice(UserManager<User> userManager, IOptions<JWT> jwt, RoleManager<IdentityRole> roleManager,FaceAppContext context)
        {
            this.userManager = userManager;
            this.jwt = jwt;
            this.roleManager = roleManager;
            this.context = context;
        } 
        #endregion

        #region Login
        public async Task<AuthModel> Login(LoginDTO loginDTO)
        {
            var user = await userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDTO.Password))
                return new AuthModel { Message = "Inavlid Email Or Password" };

            if (!user.EmailConfirmed)
            {
                return new AuthModel { Message = "Please Check Your Inbox To Confirm Email" };
            }

            user.IsActive = true;
            await context.SaveChangesAsync();

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Message = "Success",
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthencated = true
            };
        }
        #endregion

        #region Register
 
        public async Task<AuthModel> Register(RegisterDTO registerDTO)
        {
            try
            {
                if (await userManager.FindByEmailAsync(registerDTO.Email) != null)
                    return new AuthModel { Message = "Email Is Already Token" };
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
                    var error = string.Empty;
                    foreach (var item in result.Errors)
                    {
                        error += $"{item.Description},";
                    }
                    return new AuthModel { Message = error };
                }

                var RoleExsit = await RoleManager.RoleExistsAsync("admin");
                if (!RoleExsit)
                {
                    await RoleManager.CreateAsync(new IdentityRole("admin"));
                    await userManager.AddToRoleAsync(user, "admin");
                }
                else
                {
                    await RoleManager.CreateAsync(new IdentityRole("user"));
                    await userManager.AddToRoleAsync(user, "user");
                }

                var jwtSecurityToken = await CreateJwtToken(user);

                return new AuthModel
                {
                    Message = "Success",
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    ExpiresOn = jwtSecurityToken.ValidTo,
                    IsAuthencated = true
                };
            }
            catch (Exception)
            {
                return new AuthModel { Message = "Phone is exsit" };
            }

        }

        #endregion

        #region Create Token
        private async Task<JwtSecurityToken> CreateJwtToken(User user)
        {
            var userClaims = await userManager.GetClaimsAsync(user);
            var roles = await userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FullName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: jwt.Value.Issuer,
                audience: jwt.Value.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(jwt.Value.DurationInDays),
                signingCredentials: signingCredentials);



            return jwtSecurityToken;


        }
        #endregion
    }
}
