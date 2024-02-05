using System.ComponentModel.DataAnnotations;

namespace MasterBurger.Areas.Admin.Views.ViewsModels {
  public class UtilizadorViewModel {
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    public string SelectedRole { get; set; }

    public List<string> AllRoles { get; set; }

  }
}
