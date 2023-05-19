using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BlazorSyncTask.Services;
using Microsoft.IdentityModel.Tokens;

using EfcDataAccess;
using EfcDataAccess.DAOs;
using Npgsql;
using Shared.Dtos;
using Shared.Models;



namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration config;
    private readonly IAuthService authService;
    private static AsyncTaskContext context = new AsyncTaskContext();
    private UserEfcDao connectionDB = new UserEfcDao(context);
    public AuthController(IConfiguration config, IAuthService authService)
    {
        this.config = config;
        this.authService = authService;
    }

    private List<Claim> GenerateClaims(UserT user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, config["Jwt:Subject"]),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim(ClaimTypes.Name, user.username),
            new Claim("Id", user.id.ToString()),
            new Claim("DisplayName", user.fullName),

        };
        return claims.ToList();
    }

    private string GenerateJwt(UserT user)
    {
        List<Claim> claims = GenerateClaims(user);

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        SigningCredentials signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        JwtHeader header = new JwtHeader(signIn);

        JwtPayload payload = new JwtPayload(
            config["Jwt:Issuer"],
            config["Jwt:Audience"],
            claims,
            null,
            DateTime.UtcNow.AddMinutes(60));

        JwtSecurityToken token = new JwtSecurityToken(header, payload);

        string serializedToken = new JwtSecurityTokenHandler().WriteToken(token);
        return serializedToken;
    }

    [HttpPost, Route("login")]
    public async Task<ActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        Console.WriteLine("testt");
        try
        {
            
            UserT user = await connectionDB.LoginAsync(userLoginDto);
            string token = GenerateJwt(user);
            
            return Ok(token);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost, Route("register")]
    public async Task<ActionResult> register([FromBody] UserRegisterDto user)
    {
        try
        {
            await connectionDB.CreateAsync(user);
            return Created($"/users/{user.username}", user);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
       
    }

    [HttpPost, Route("test")]
    public async Task<ActionResult> test(UserRegisterDto user)
    {
       
        try
        {
            await connectionDB.CreateAsync(user);
            return Created($"/users/{user.username}", user);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

      
    }


}