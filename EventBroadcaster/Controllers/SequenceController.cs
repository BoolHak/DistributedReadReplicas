using EventBroadcaster.Extension;
using EventBroadcaster.LocalDb;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventBroadcaster.Controllers
{
    [Route("api/[controller]/{table}")]
    [ApiController]
    [ServiceFilter(typeof(InternalTokenAuth))]
    public class SequenceController : ControllerBase
    {
        public IActionResult Get([FromQuery]int version, [FromRoute] string table)
        {
            if (version < 1) return BadRequest("version must be valid");
            if (table == null) return BadRequest("table must be valid");

            using (var db = new LiteDatabase(@"Filename=C:\Temp\LocalCache.db;connection=shared;ReadOnly=true"))
            {
                var col = db.GetCollection<Sequences>("sequences");
                var result = col.Query().Where(x => x.Version == version && x.TableName == table).FirstOrDefault();
                if(result == null) return NotFound();
                return Ok(result);
            }
        }
    }
}
