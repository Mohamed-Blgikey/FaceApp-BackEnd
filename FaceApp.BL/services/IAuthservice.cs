using FaceApp.BL.Dtos;
using FaceApp.BL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceApp.BL.services
{
    public interface IAuthservice
    {
        Task<AuthModel> Register(RegisterDTO registerDTO);
        Task<AuthModel> Login();

    }
}
