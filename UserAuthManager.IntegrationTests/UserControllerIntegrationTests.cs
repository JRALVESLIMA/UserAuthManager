using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UserAuthManager.API.Data;
using UserAuthManager.API.Models;
using Xunit;

namespace UserAuthManager.IntegrationTests
{
    public class UserControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UserControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnSuccessStatusCode()
        {
            var response = await _client.GetAsync("/api/users");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = new ApplicationUser
            {
                Name = "Test User",
                UserName = "testuser",
                Email = "testuser@example.com",
                PasswordHash = "hashed-password"
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync($"/api/users/{user.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var resultUser = JsonConvert.DeserializeObject<ApplicationUser>(content);

            resultUser.Should().NotBeNull();
            resultUser!.Email.Should().Be("testuser@example.com");
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = new ApplicationUser
            {
                Name = "Invalid Login User",
                UserName = "invalidlogin",
                Email = "invalidlogin@example.com"
            };

            var result = await userManager.CreateAsync(user, "SenhaCorreta123!");
            result.Succeeded.Should().BeTrue("Usuário deve ser criado para o teste");

            var loginData = new
            {
                Email = "invalidlogin@example.com",
                Password = "SenhaErrada456!"
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/users/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = new ApplicationUser
            {
                Name = "Auth User",
                UserName = "authuser",
                Email = "authuser@example.com"
            };

            var password = "SenhaForte123!";
            var result = await userManager.CreateAsync(user, password);
            result.Succeeded.Should().BeTrue("Usuário deve ser criado com sucesso para o teste de login");

            var loginData = new
            {
                Email = "authuser@example.com",
                Password = password
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/users/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Should().Contain("token", "O login com credenciais válidas deve retornar um token");
        }



        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = new ApplicationUser
            {
                Name = "Invalid Auth User",
                UserName = "invaliduser",
                Email = "invaliduser@example.com"
            };

            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, "CorrectPassword123");

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var loginData = new
            {
                Email = "invaliduser@example.com",
                Password = "WrongPassword123"
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/users/login", content);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenEmailDoesNotExist()
        {
            var loginRequest = new
            {
                Email = "naoexiste@example.com",
                Password = "SenhaQualquer123!"
            };

            var response = await _client.PostAsJsonAsync("/api/users/login", loginRequest);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }

        [Fact]
        public async Task Register_ShouldCreateUser_WhenDataIsValid()
        {
            // Arrange
            var registerRequest = new
            {
                Name = "Novo Usuário",
                UserName = "novousuario",
                Email = "novousuario@example.com",
                Password = "SenhaForte123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Usuário registrado com sucesso");
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordIsWeak()
        {
            
            var weakUser = new
            {
                Name = "Usuário Fraco",
                UserName = "usuariofraco",
                Email = "usuariofraco@example.com",
                Password = "123" // senha fraca
            };

            var response = await _client.PostAsJsonAsync("/api/users/register", weakUser);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Should().Contain("minimum length of '6'");

        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange: Cria um usuário com o email
            var existingUser = new
            {
                Name = "Usuário Existente",
                UserName = "usuarioexistente",
                Email = "existente@example.com",
                Password = "SenhaForte123!"
            };

            await _client.PostAsJsonAsync("/api/users/register", existingUser);

            // Act: Tenta registrar novamente com o mesmo email
            var duplicateUser = new
            {
                Name = "Duplicado",
                UserName = "outronome",
                Email = "existente@example.com", // mesmo email
                Password = "SenhaForte123!"
            };

            var response = await _client.PostAsJsonAsync("/api/users/register", duplicateUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("já está sendo utilizado");
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordIsTooWeak()
        {
            var weakUser = new
            {
                Name = "Fraco",
                UserName = "usuariofraco",
                Email = "usuariofraco@example.com",
                Password = "123" // senha fraca
            };

            var response = await _client.PostAsJsonAsync("/api/users/register", weakUser);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("The field Password must be a string or array type with a minimum length of '6'.");
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenRequiredFieldsAreEmpty()
        {
            var user = new
            {
                Name = "",
                UserName = "",
                Email = "",
                Password = ""
            };

            var response = await _client.PostAsJsonAsync("/api/users/register", user);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailIsInvalid()
        {
            var user = new
            {
                Name = "Nome",
                UserName = "usuarioemailruim",
                Email = "email-invalido",
                Password = "SenhaForte123!"
            };

            var response = await _client.PostAsJsonAsync("/api/users/register", user);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

    }
}
