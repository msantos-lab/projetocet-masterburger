namespace MasterBurger.Areas.Admin.Views.ViewsModels {
  public class EditRolesViewModels {

    public string UserId { get; set; }
    public string UserName { get; set; }
    public string SelectedRole { get; set; } 
    public List<string> UserRoles { get; set; }
    public List<string> AllRoles { get; set; }

  }
}
