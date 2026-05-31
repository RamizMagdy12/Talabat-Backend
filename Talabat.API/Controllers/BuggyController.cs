using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.API.Errors;
using Talabat.Repository.Data;

namespace Talabat.API.Controllers
{
	public class BuggyController : BaseAPIController
	{
		private readonly StoreContext storeContext;

		public BuggyController(StoreContext storeContext)
		{
			this.storeContext = storeContext;
		}
		[HttpGet("not-found")] // GET: /api/buggy/not-found
		public ActionResult GetNotFound()
		{
			var thing = storeContext.products.Find(42);
			if (thing == null) return NotFound(new ApiResponse(404));
			return Ok(thing);
		}
		[HttpGet("server-error")]
		public ActionResult GetServerError() // GET: /api/buggy/server-error
		{
			var thing = storeContext.products.Find(42);
			var thingToReturn = thing.ToString(); // will throw null reference exception because thing is null
			return Ok(thingToReturn);
		}
		[HttpGet("bad-request")] // GET: /api/buggy/bad-request
		public ActionResult GetBadRequest()
		{
			return BadRequest(new ApiResponse(400));
		}
		[HttpGet("bad-request/{id}")] // GET: /api/buggy/bad-request/FIVE
		public ActionResult GetBadRequest(int id) // will throw model binding error because id is not an integer VALIDATION ERROR
		{
			return BadRequest(new ApiValidationErrorResponse());
		}

	}
}
