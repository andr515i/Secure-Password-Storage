using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Secure_Password_Storage.models;
using System.Text;
using BCrypt.Net;

namespace Secure_Password_Storage.Controllers
{
	public class LoginController : Controller
	{

		private readonly DataContext _context;

		public LoginController(DataContext context)
		{
			_context = context;
		}


		[HttpPost("CheckDbConnnection")]
		public IActionResult CheckDatabaseConnection()
		{
			try
			{
				_context.Database.EnsureCreated(); // Open and close a connection to the database
				return Ok("Database connection successful.");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Database connection error: {ex.Message}");
			}
		}



		[HttpPost("CreateLogin")]
		public IActionResult Login(string username, string password)
		{

			password = BCrypt.Net.BCrypt.HashPassword(password);

			try
			{
				_context.Database.ExecuteSqlInterpolated($"INSERT INTO users (username, password) VALUES ({username}, {password});");

				string response = $"{username} - {password}";
				return Ok(response);
			}
			catch (Exception ex)
			{
				// Log the exception for debugging
				Console.WriteLine($"An error occurred: {ex.Message}");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}

		[HttpPost("Login")]
		public async Task<ActionResult<Users>> Login(Users req)
		{
			try
			{
				string wrongLogin = "Incorrect Username or Password";

				if (req.Username != null)
				{
					// get a list from the database of all users with the same username. 
					var returnList = await _context.Users
						.FromSqlRaw("EXEC GetLogin @Username", new SqlParameter("@Username", req.Username))
						.ToListAsync();

					// for simplicity sake, we only take the first instance. if we have duplicate names, then only the first one will ever be used.
					var loginReturn = returnList.FirstOrDefault();


					if (loginReturn == null)
					{
						return BadRequest(wrongLogin);
					}


					if (!BCrypt.Net.BCrypt.Verify(req.Password, loginReturn.Password))
					{
						return BadRequest(wrongLogin);
					}


					string hashed = loginReturn.Password; // Just to Show The Result
					string username = req.Username; // Just to Show The Result
					string password = req.Password; // Just to Show The Result
					return Ok(new { username, password, hashed }); // Would normally just send a "User Successfully Logged In" Message
				}
				else
				{
					return BadRequest(wrongLogin);
				}
			}
			catch (SqlException sqlEx)
			{
				return BadRequest("Sql Error: " + sqlEx.Message);
			}
			catch (Exception ex)
			{
				return BadRequest("Error: " + ex.Message);
			}
		}



		[HttpGet("GetUsers")]
		public List<Users> GetUsers()
		{
			List<Users> user = _context.Users
			.FromSqlRaw("SELECT * FROM users")
			.ToList();



			return user;

		}





		
	}
}
