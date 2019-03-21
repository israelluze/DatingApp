using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDto dto)
        {

            //validate request
            dto.Username = dto.Username.ToLower();

            if (await _repo.UserExists(dto.Username))
                return BadRequest("Usuário já existente!");

            var userToCreate = new User
            {
                UserName = dto.Username
            };

            var createdUser = await _repo.Register(userToCreate, dto.Password);

            return StatusCode(201);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto UserForLoginDto)
        {
            
            //primeiro verificar se o usuário existe na base de dados
            var userFromRepo = await _repo.Login(UserForLoginDto.Username.ToLower(), UserForLoginDto.Password);
            
            //se não foi encontrado retorna que não está autorizado sem dar pistas se o usuário existe ou não
            if (userFromRepo == null)
                return Unauthorized();

            //cria as claims(parametros) do payload do token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            //cria a chave com o secret que está dentro da appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            //assina a chave
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            //prepara o token com os claims privados vinculando a assinatura da chave
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), //deveria ser por horas ou minutos
                SigningCredentials = creds
            };

            //cria o token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            //retorna o token 
            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });

        }


    }
}