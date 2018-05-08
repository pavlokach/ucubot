using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ucubot.Model;
using Dapper;

namespace ucubot.Controllers
{
    [Route("api/[controller]")]
    public class StudentEndpointController : Controller
    {
        private readonly IConfiguration _configuration;

        public StudentEndpointController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<Student> ShowSignals()
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var mySqlConnection = new MySqlConnection(connectionString))
            {
                mySqlConnection.Open();
                var res = mySqlConnection.Query<Student>("SELECT id AS Id, " +
                                                    "first_name AS FirstName, " +
                                                    "last_name AS LastName, " +
                                                    "user_id AS UserId FROM student").AsList();
                mySqlConnection.Close();
                return res;
            }
        }

        [HttpGet("{id}")]
        public Student ShowStudent(long id)
        {
            
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var mySqlConnection = new MySqlConnection(connectionString))
            {
                mySqlConnection.Open();
                var res = mySqlConnection.Query<Student>("SELECT id AS Id, " +
                                                         "first_name AS FirstName, " +
                                                         "last_name AS LastName, " +
                                                         "user_id AS UserId " +
                                                         "FROM student WHERE id=@Id",
                    new {Id = id}).ToList();
                mySqlConnection.Close();
                return res.Count == 0 ? null : res[0];
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent(Student student)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var mySqlConnection = new MySqlConnection(connectionString)){
                mySqlConnection.Open();
                try{
                    mySqlConnection.Execute("INSERT INTO student (first_name, last_name, user_id) VALUES (@name, @surname, @id)",
                        new {name = student.FirstName, surname = student.LastName, id = student.UserId});
                }
                catch (Exception e){
                    Console.WriteLine(e);
                    mySqlConnection.Close();
                    return StatusCode(409);
                }
                mySqlConnection.Close();
                return Accepted();                
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStudent(Student student)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var mySqlConnection = new MySqlConnection(connectionString))
            {
                mySqlConnection.Open();
                try{
                    mySqlConnection.Execute("UPDATE student SET first_name = @name, last_name = @surname, user_id = @id where id = @id2;",
                        new {name = student.FirstName, surname = student.LastName, id = student.UserId, id2 = student.Id});
                }
                catch (Exception e){
                    Console.WriteLine(e);
                }
                mySqlConnection.Close();
                return Accepted();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveStudent(long id)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            try{
                connection.Execute("DELETE FROM student WHERE id = @input_id;", new {input_id = id});
            }catch (Exception e){
                Console.WriteLine(e);
                connection.Close();
                return StatusCode(409);
            }
            connection.Close();
            return Accepted();
        }
    }
}