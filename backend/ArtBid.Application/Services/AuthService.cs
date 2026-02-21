using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtBid.Application.DTOs;
using ArtBid.Application.Repositories;
using ArtBid.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace ArtBid.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecret;

        public AuthService(IUserRepository userRepository, string jwtSecret)
        {
            _userRepository = userRepository;
            _jwtSecret = jwtSecret;
        }

        public AuthResponse SignUp(SignUpRequest request)
        {
            // Validaciones básicas
            if (_userRepository.GetByEmail(request.Email) != null)
                throw new ApplicationException("Email already exists");

            var user = new User(
                request.Username,
                request.Email,
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Password)),
                initialBalance: 1000m
            );

            _userRepository.Add(user);

            return new AuthResponse
            {
                Username = user.Username ?? throw new InvalidOperationException("Username cannot be null"),
                Email = user.Email ?? throw new InvalidOperationException("Email cannot be null"),
                Token = GenerateJwtToken(user)
            };
        }

        public AuthResponse SignIn(SignInRequest request)
        {
            var user = _userRepository.GetByEmail(request.Email);
            if (user == null || !user.VerifyPassword(request.Password))
                throw new ApplicationException("Invalid credentials");

            return new AuthResponse
            {
                Username = user.Username ?? throw new InvalidOperationException("Username cannot be null"),
                Email = user.Email ?? throw new InvalidOperationException("Email cannot be null"),
                Token = GenerateJwtToken(user)
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            if (!string.IsNullOrEmpty(user.Username))
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}