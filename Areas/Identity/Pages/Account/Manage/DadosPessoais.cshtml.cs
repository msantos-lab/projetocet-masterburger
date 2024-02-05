// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MasterBurger.Data;
using MasterBurger.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MasterBurger.Areas.Identity.Pages.Account.Manage {
  public class DadosPessoaisModel : PageModel {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public DadosPessoaisModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context) {
      _userManager = userManager;
      _signInManager = signInManager;
      _context = context;
    }


    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel {

      [Display(Name = "Nome")]
      public string Nome { get; set; }

      [Display(Name = "Apelido")]
      public string Apelido { get; set; }

      [Display(Name = "NIF")]
      public string NIF { get; set; }

      [Display(Name = "Morada")]
      public string Morada { get; set; }

      [Display(Name = "Localidade")]
      public string Localidade { get; set; }

      [Display(Name = "CodigoPostal")]
      public string MoraCodigoPostalda { get; set; }

      [Display(Name = "Telemovel")]
      public string Telemovel { get; set; }
    }

    private async Task<DadosUtilizador> LoadAsync() {
      var user = await _userManager.GetUserAsync(User);
      DadosUtilizador dadosUtilizador = null;

      if (user != null) {
        dadosUtilizador = _context.DadosUser
            .Where(dadosUser => dadosUser.UserId == user.Id)
            .SelectMany(dadosUser => _context.DadosUtilizador
                .Where(dadosUtilizador => dadosUtilizador.DadosUtilizadorId == dadosUser.DadosUtilId))
            .FirstOrDefault(); 
      }

      return dadosUtilizador;
    }

    public async Task<IActionResult> SeuMetodoGet() {
      var dadosUtilizador = await LoadAsync();
      return Page();
    }


    public async Task<IActionResult> OnPostAsync(string id) {
      var userId = _userManager.GetUserId(User);

      var dados = await _context.DadosUser
          .Where(du => du.UserId == userId && du.DadosUtilId == id)
          .Select(du => _context.DadosUtilizador.FirstOrDefault(duu => duu.DadosUtilizadorId == du.DadosUtilId))
          .FirstOrDefaultAsync();

      var model = new DadosUtilizador {
        Nome = dados.Nome,
        Apelido = dados.Apelido,
        NIF = dados.NIF,
        Morada = dados.Morada,
        Localidade = dados.Localidade,
        CodigoPostal = dados.CodigoPostal,
        Telemovel = dados.Telemovel
      };

      return Page();
    }
  }
}
