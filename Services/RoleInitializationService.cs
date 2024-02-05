using Microsoft.AspNetCore.Identity;

namespace MasterBurger.Services {
  public class RoleInitializationService {

    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleInitializationService(RoleManager<IdentityRole> roleManager) {
      _roleManager = roleManager;
    }

    public async Task InitializeRoles() {
      if (!await _roleManager.RoleExistsAsync("Admin")) {
        var role = new IdentityRole("Admin");
        await _roleManager.CreateAsync(role);
      }

      if (!await _roleManager.RoleExistsAsync("Cliente")) {
        var role = new IdentityRole("Cliente");
        await _roleManager.CreateAsync(role);
      }

			if (!await _roleManager.RoleExistsAsync("Cozinha")) {
				var role = new IdentityRole("Cozinha");
				await _roleManager.CreateAsync(role);
			}

			if (!await _roleManager.RoleExistsAsync("Entrega")) {
				var role = new IdentityRole("Entrega");
				await _roleManager.CreateAsync(role);
			}
		}
  }
}
