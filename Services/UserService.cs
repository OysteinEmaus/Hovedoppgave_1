using System;
using System.Threading.Tasks;
using Hovedoppgave.Data;
using Hovedoppgave.Models;
using Microsoft.EntityFrameworkCore;

namespace Hovedoppgave.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public UserService(ApplicationDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            // Sjekke om brukernavn allerede eksisterer
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new Exception("Username is already taken");
            }

            // Sjekke om e-post allerede eksisterer
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new Exception("Email is already registered");
            }

            // Lage ny bruker
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generere JWT token
            var token = _tokenService.GenerateJwtToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            // Finn bruker etter brukernavn
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                throw new Exception("Invalid username or password");
            }

            // Verifiser passord
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password");
            }

            // Generere JWT token
            var token = _tokenService.GenerateJwtToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };
        }
    }
}