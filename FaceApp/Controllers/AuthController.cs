using FaceApp.BL.Dtos;
using FaceApp.BL.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FaceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthservice authservice;

        public AuthController(IAuthservice authservice)
        {
            this.authservice = authservice;
        }
        [HttpPost]
        [Route("~/Regikster")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return Ok(ModelState);
            }
            var result = await  authservice.Register(registerDTO);
            if (!result.IsAuthencated)
            {
                return Ok(new { message = result.Message });
            }
            return Ok(new { message = result.Message, token = result.Token});
        }
    }
}
