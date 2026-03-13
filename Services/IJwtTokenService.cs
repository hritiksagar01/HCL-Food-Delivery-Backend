using HCL_Food_Delivery.Models;

namespace HCL_Food_Delivery.Services;

public interface IJwtTokenService
{
    string CreateToken(User user);
}

