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
    public class LessonSignalEndpointController : Controller
    {
        private readonly IConfiguration _configuration;

        public LessonSignalEndpointController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<LessonSignalDto> ShowSignals()
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var mySqlConnection = new MySqlConnection(connectionString))
            {
                var outp = mySqlConnection.Query<LessonSignalDto>("SELECT lesson_signal.id AS Id, " +
                                                                  "lesson_signal.timestamp_ AS TimeStamp, " +
                                                                  "lesson_signal.signal_type AS Type, " +
                                                                  "student.user_id AS UserId " +
                                                                  "FROM lesson_signal JOIN student ON " +
                                                                  "lesson_signal.student_id = student.id;").AsList();
                return outp;
            }
        }
        
        
        [HttpGet("{id}")]
        public LessonSignalDto ShowSignal(long id)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var mySqlConnection = new MySqlConnection(connectionString))
            {
                var outp = mySqlConnection.Query<LessonSignalDto>("SELECT lesson_signal.id AS Id, " +
                                                                  "lesson_signal.timestamp_ AS TimeStamp, " +
                                                                  "lesson_signal.signal_type AS Type, " +
                                                                  "student.user_id AS UserId " +
                                                                  "from lesson_signal JOIN student ON " +
                                                                  "lesson_signal.student_id = student.id " +
                                                                  "WHERE lesson_signal.id=@input_id;",
                    new {input_id = id}).ToList();
                if (outp.Count == 0){
                    return null;   
                }
                return outp[0];                    
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSignal(SlackMessage message)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var mySqlConnection = new MySqlConnection(connectionString))
            {
                var vars = mySqlConnection.Query<Student>("SELECT id AS Id, " +
                                                          "first_name AS FirstName, " +
                                                          "last_name AS LastName, " +
                                                          "user_id AS UserId " +
                                                          "FROM student WHERE user_id=@Id",
                    new {Id = message.user_id}).AsList();
                if (vars.Any()){
                    mySqlConnection.Execute("INSERT INTO lesson_signal (student_id, signal_type) VALUES (@var0, @mtype)",
                        new {var0 = vars[0].Id, mtype = message.text.ConvertSlackMessageToSignalType()});
                    return Accepted();
                }
                mySqlConnection.Close();
                return BadRequest();   
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveSignal(long id)
        {
            //Close to before but with DELETE
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var mySqlConnection = new MySqlConnection(connectionString))
            {
                try{
                    mySqlConnection.Execute("DELETE FROM lesson_signal WHERE id = @id;", new {id = id});
                }catch (Exception e){
                    Console.WriteLine(e);
                }
                return Accepted();
            }
        }
    }
}
