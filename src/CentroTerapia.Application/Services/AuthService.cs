
using CentroTerapia.Application.Interfaces;
using CentroTerapia.Domain.Ports.Out;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CentroTerapia.Application.DTOs.Auth;
using CentroTerapia.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace CentroTerapia.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> ResgiterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Registering new user with email: {Email}", dto.Email);
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed. User with email {Email} already exists.", dto.Email);
                throw new Exception("User with this email already exists.");
            }
            var user = new User
            {
                Correo = dto.Email,
                HashContrasena = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Nombres = dto.FirstName,
                Apellidos = dto.LastName,
                Rol = dto.Role,
                FechaCreacion = DateTime.UtcNow,
                Activo = true

            };

            await _unitOfWork.Users.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("User with email {Email} registered successfully.", dto.Email);

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Correo,
                FirstName = user.Nombres,
                LastName = user.Apellidos,
                Role = user.Rol,
                Token = token
            };
        }
        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET not configured")));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Correo),
                new Claim(ClaimTypes.Name, $"{user.Nombres} {user.Apellidos}"),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            // Si el usuario es Padre, agregar el claim FamiliaId
            if (user.Rol == "Padre")
            {
                // Buscar la familia asociada al usuario
                var familia = _unitOfWork.Familias.GetAllAsync().GetAwaiter().GetResult()
                    .FirstOrDefault(f => (f.ResponsablePrincipalEmail ?? "").Trim().ToLower() == user.Correo.Trim().ToLower());
                if (familia != null)
                {
                    claims.Add(new Claim("FamiliaId", familia.Id.ToString()));
                }
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT_ISSUER"],
                audience: _configuration["JWT_AUDIENCE"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Validar existencia de usuario y contraseña
            var userTask = _unitOfWork.Users.GetByEmailAsync(loginDto.Email);
            var user = userTask.GetAwaiter().GetResult();
            if (user == null)
            {
                throw new Exception("Credenciales inválidas.");
            }

            var valid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashContrasena);
            if (!valid)
            {
                throw new Exception("Credenciales inválidas.");
            }

            var token = GenerateJwtToken(user);

            var resp = new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Correo,
                FirstName = user.Nombres,
                LastName = user.Apellidos,
                Role = user.Rol,
                Token = token
            };

            return Task.FromResult(resp);
        }
    }


}
