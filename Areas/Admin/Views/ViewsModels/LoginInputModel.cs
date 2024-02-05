using System.ComponentModel.DataAnnotations;

namespace MasterBurger.Areas.Admin.Views.ViewsModels {
	public class LoginInputModel {
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Display(Name = "Lembrar-me?")]
		public bool RememberMe { get; set; }
	}
}
