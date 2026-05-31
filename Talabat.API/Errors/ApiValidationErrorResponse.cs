 namespace Talabat.API.Errors
{
	public class ApiValidationErrorResponse:ApiResponse
	{
		public IEnumerable<string> Errors { get; set; }
		public ApiValidationErrorResponse() : base(400) // because validation errors are always bad requests
		{
			Errors = new List<string>();
		}
	}
}
